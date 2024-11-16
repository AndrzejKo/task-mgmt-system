using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Entities = TaskManagerApi.Models.Entities;
using Actions = TaskManagerApi.Models.Actions;
using Events = TaskManagerApi.Models.Events;

namespace TaskManagerApi.Services;

public class ServiceBusHandler : BackgroundService
{
    const int MaxRetries = 5;
    const int DelayMilliseconds = 1000;
    private readonly ServiceBusClient serviceBusClient;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<ServiceBusHandler> logger;

    private readonly ServiceBusSender taskStatusUpdateReqSender;
    private readonly ServiceBusSender taskCompletedEventPublisher;
    private string? taskStatusUpdatesQueue;
    private string? taskStatusUpdatedEventsTopic;
    private string? taskStatusUpdatedEventsSubscription;

    public ServiceBusHandler(IConfiguration configuration, ILogger<ServiceBusHandler> logger, ServiceBusClient serviceBusClient,
        IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceBusClient = serviceBusClient;
        this.serviceScopeFactory = serviceScopeFactory;
        this.logger = logger;

        taskStatusUpdatesQueue = configuration["taskStatusUpdatesQueue"];
        taskStatusUpdatedEventsTopic = configuration["taskStatusUpdateEventsTopic"];
        taskStatusUpdatedEventsSubscription = configuration["taskStatusUpdateEventsSubscription"];

        taskStatusUpdateReqSender = this.serviceBusClient.CreateSender(taskStatusUpdatesQueue);
        taskCompletedEventPublisher = this.serviceBusClient.CreateSender(taskStatusUpdatedEventsTopic);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var taskStatusUpdateTask = ReceiveTaskStatusUpdateActionAsync(stoppingToken);
        var taskCompletedEventsTask = ReceiveTaskStatusUpdatedEventsAsync(stoppingToken);

        await Task.WhenAll(taskStatusUpdateTask, taskCompletedEventsTask);
    }

    public async Task SendTaskStatusUpdateActionAsync(Actions.UpdateTaskStatusAction action)
    {
        int delayMilliseconds = DelayMilliseconds;

        for (int retry = 0; retry < MaxRetries; retry++)
        {
            try
            {
                string messageBody = JsonSerializer.Serialize(action);
                var serviceBusMessage = new ServiceBusMessage(messageBody);
                await taskStatusUpdateReqSender.SendMessageAsync(serviceBusMessage);

                logger.LogInformation("Message sent successfully: {Message}", messageBody);
                return;
            }
            catch (ServiceBusException ex) when (ex.IsTransient)
            {
                logger.LogWarning(ex, "Service Bus is busy or experiencing transient issues. Retrying in {Delay} milliseconds...", delayMilliseconds);
                await Task.Delay(delayMilliseconds);
                delayMilliseconds *= 2; // Exponential backoff
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send message to Service Bus.");
                throw;
            }
        }

        logger.LogError("Exceeded maximum retry attempts. Failed to send message to Service Bus.");
    }

    public async Task ReceiveTaskStatusUpdateActionAsync(CancellationToken stoppingToken)
    {
        var processor = serviceBusClient.CreateProcessor(taskStatusUpdatesQueue, new ServiceBusProcessorOptions());

        processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var messageBody = args.Message.Body.ToString();
                logger.LogInformation("Message received: {Message}", messageBody);

                var updateRequest = JsonSerializer.Deserialize<Actions.UpdateTaskStatusAction>(messageBody);
                if (updateRequest != null)
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

                    await taskService.UpdateTaskStatusAsync(updateRequest.TaskId, updateRequest.NewStatus);

                    var e = new Events.TaskStatusUpdatedEvent()
                    {
                        Id = updateRequest.TaskId,
                        NewStatus = updateRequest.NewStatus,
                        CompletedAtUtc = DateTime.UtcNow
                    };

                    await PublishTaskCompletedEventAsync(e);
                }
                else
                {
                    logger.LogError("Failed to deserialize message: {Message}", messageBody);
                    await args.AbandonMessageAsync(args.Message);
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message: {MessageId}", args.Message.MessageId);
                await args.AbandonMessageAsync(args.Message);
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            logger.LogError(args.Exception, "Error in message processor: {ErrorSource}", args.ErrorSource);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public async Task ReceiveTaskStatusUpdatedEventsAsync(CancellationToken stoppingToken)
    {
        var processor = serviceBusClient.CreateProcessor(taskStatusUpdatedEventsTopic, taskStatusUpdatedEventsSubscription, new ServiceBusProcessorOptions());

        processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var messageBody = args.Message.Body.ToString();
                logger.LogInformation("Event received: {Message}", messageBody);

                var e = JsonSerializer.Deserialize<Events.TaskStatusUpdatedEvent>(messageBody);
                if (e != null)
                {
                    logger.LogInformation("Task status updated event received for task: {TaskId}. New status:{Status} At: {CompletedAtUtc}", e.Id, e.NewStatus, e.CompletedAtUtc);
                }
                else
                {
                    logger.LogError("Failed to deserialize event: {Message}", messageBody);
                    await args.AbandonMessageAsync(args.Message);
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing event: {MessageId}", args.Message.MessageId);
                await args.AbandonMessageAsync(args.Message);
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            logger.LogError(args.Exception, "Error in event processor: {ErrorSource}", args.ErrorSource);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public async Task PublishTaskCompletedEventAsync(Events.TaskStatusUpdatedEvent taskStatusUpdatedEvent)
    {
        int delayMilliseconds = DelayMilliseconds;

        for (int retry = 0; retry < MaxRetries; retry++)
        {
            try
            {
                string messageBody = JsonSerializer.Serialize(taskStatusUpdatedEvent);
                var serviceBusMessage = new ServiceBusMessage(messageBody);
                await taskCompletedEventPublisher.SendMessageAsync(serviceBusMessage);

                logger.LogInformation("Task status updated event published successfully: {Message}", messageBody);
                return;
            }
            catch (ServiceBusException ex) when (ex.IsTransient)
            {
                logger.LogWarning(ex, "Service Bus is busy or experiencing transient issues. Retrying in {Delay} milliseconds...", delayMilliseconds);
                await Task.Delay(delayMilliseconds);
                delayMilliseconds *= 2; // Exponential backoff
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish event to Service Bus.");
                throw;
            }
        }

        logger.LogError("Exceeded maximum retry attempts. Failed to publish event to Service Bus.");
    }
}