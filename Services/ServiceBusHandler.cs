using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Entities = TaskManagerApi.Models.Entities;
using Actions = TaskManagerApi.Models.Actions;
using Events = TaskManagerApi.Models.Events;

namespace TaskManagerApi.Services;

public class ServiceBusHandler : BackgroundService
{
    private readonly ServiceBusClient serviceBusClient;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<ServiceBusHandler> logger;

    private readonly ServiceBusSender taskStatusUpdateReqSender;
    private readonly ServiceBusSender taskCompletedEventPublisher;
    private string? taskStatusUpdatesQueue;
    private string? taskStatusUpdateEventsTopic;
    private string? taskStatusUpdateEventsSubscription;

    public ServiceBusHandler(IConfiguration configuration, ILogger<ServiceBusHandler> logger, ServiceBusClient serviceBusClient,
        IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceBusClient = serviceBusClient;
        this.serviceScopeFactory = serviceScopeFactory;
        this.logger = logger;

        taskStatusUpdatesQueue = configuration["taskStatusUpdatesQueue"];
        taskStatusUpdateEventsTopic = configuration["taskStatusUpdateEventsTopic"];
        taskStatusUpdateEventsSubscription = configuration["taskStatusUpdateEventsSubscription"];

        taskStatusUpdateReqSender = this.serviceBusClient.CreateSender(taskStatusUpdatesQueue);
        taskCompletedEventPublisher = this.serviceBusClient.CreateSender(taskStatusUpdateEventsTopic);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var taskStatusUpdateTask = ReceiveTaskStatusUpdateActionAsync(stoppingToken);
        var taskCompletedEventsTask = ReceiveTaskCompletedEventsAsync(stoppingToken);

        await Task.WhenAll(taskStatusUpdateTask, taskCompletedEventsTask);
    }

    public async Task SendTaskStatusUpdateActionAsync(Actions.UpdateTaskStatusAction action)
    {
        try
        {
            string messageBody = JsonSerializer.Serialize(action);
            var serviceBusMessage = new ServiceBusMessage(messageBody);
            await taskStatusUpdateReqSender.SendMessageAsync(serviceBusMessage);

            logger.LogInformation("Message sent successfully: {Message}", messageBody);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message to Service Bus.");
            throw;
        }
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

                    await taskService.UpdateTaskStatus(updateRequest.TaskId, updateRequest.NewStatus);

                    if (updateRequest.NewStatus == Entities.Status.Completed)
                    {
                        var e = new Events.TaskCompletedEvent()
                        {
                            Id = updateRequest.TaskId,
                            CompletedAtUtc = DateTime.UtcNow
                        };

                        string eventBody = JsonSerializer.Serialize(e);
                        var serviceBusMessage = new ServiceBusMessage(eventBody);
                        await taskCompletedEventPublisher.SendMessageAsync(serviceBusMessage);

                        logger.LogInformation("Task completed event published successfully: {Message}", eventBody);
                    }
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

    public async Task ReceiveTaskCompletedEventsAsync(CancellationToken stoppingToken)
    {
        var processor = serviceBusClient.CreateProcessor(taskStatusUpdateEventsTopic, taskStatusUpdateEventsSubscription, new ServiceBusProcessorOptions());

        processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var messageBody = args.Message.Body.ToString();
                logger.LogInformation("Event received: {Message}", messageBody);

                var e = JsonSerializer.Deserialize<Events.TaskCompletedEvent>(messageBody);
                if (e != null)
                {
                    logger.LogInformation("Task completed event received for task: {TaskId}. Completed at: {CompletedAtUtc}", e.Id, e.CompletedAtUtc);
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
}