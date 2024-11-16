using System.Text.Json;
using Azure.Messaging.ServiceBus;
using FakeItEasy;
using TaskManagerApi.Models.Actions;
using TaskManagerApi.Models.Events;
using TaskManagerApi.Services;
using Xunit;
using Entities = TaskManagerApi.Models.Entities;


public class ServiceBusHandlerTests
{
    private readonly ServiceBusHandler _serviceBusHandler;
    private readonly IConfiguration _mockConfiguration;
    private readonly ILogger<ServiceBusHandler> _mockLogger;
    private readonly ServiceBusClient _mockServiceBusClient;
    private readonly IServiceScopeFactory _mockServiceScopeFactory;
    private readonly ServiceBusSender _mockTaskStatusUpdateActionSender;
    private readonly ServiceBusSender _mockTaskStatusUpdatedEventPublisher;

    public ServiceBusHandlerTests()
    {
        _mockConfiguration = A.Fake<IConfiguration>();
        _mockLogger = A.Fake<ILogger<ServiceBusHandler>>();
        _mockServiceBusClient = A.Fake<ServiceBusClient>();
        _mockServiceScopeFactory = A.Fake<IServiceScopeFactory>();
        _mockTaskStatusUpdateActionSender = A.Fake<ServiceBusSender>();
        _mockTaskStatusUpdatedEventPublisher = A.Fake<ServiceBusSender>();

        A.CallTo(() => _mockConfiguration["taskStatusUpdatesQueue"]).Returns("taskStatusUpdatesQueue");
        A.CallTo(() => _mockConfiguration["taskStatusUpdateEventsTopic"]).Returns("taskStatusUpdateEventsTopic");

        A.CallTo(() => _mockServiceBusClient.CreateSender("taskStatusUpdatesQueue")).Returns(_mockTaskStatusUpdateActionSender);
        A.CallTo(() => _mockServiceBusClient.CreateSender("taskStatusUpdateEventsTopic")).Returns(_mockTaskStatusUpdatedEventPublisher);

        _serviceBusHandler = new ServiceBusHandler(_mockConfiguration, _mockLogger, _mockServiceBusClient, _mockServiceScopeFactory);
   
    }

    [Fact]
    public async Task SendTaskStatusUpdateActionAsync_SendsMessageSuccessfully()
    {
        // Arrange
        var action = new UpdateTaskStatusAction { TaskId = 1, NewStatus = Entities.Status.InProgress };
        var messageBody = JsonSerializer.Serialize(action);
        var serviceBusMessage = new ServiceBusMessage(messageBody);

        // Act
        await _serviceBusHandler.SendTaskStatusUpdateActionAsync(action);

        // Assert
        A.CallTo(() => _mockTaskStatusUpdateActionSender.SendMessageAsync(A<ServiceBusMessage>.That.Matches(m => m.Body.ToString() == messageBody), default))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task PublishTaskStatusUpdatedEventAsync_PublishesEventSuccessfully()
    {
        // Arrange
        var taskCompletedEvent = new TaskStatusUpdatedEvent { Id = 1, CompletedAtUtc = DateTime.UtcNow };
        var messageBody = JsonSerializer.Serialize(taskCompletedEvent);
        var serviceBusMessage = new ServiceBusMessage(messageBody);

        // Act
        await _serviceBusHandler.PublishTaskCompletedEventAsync(taskCompletedEvent);

        // Assert
        A.CallTo(() => _mockTaskStatusUpdatedEventPublisher.SendMessageAsync(A<ServiceBusMessage>.That.Matches(m => m.Body.ToString() == messageBody), default))
            .MustHaveHappenedOnceExactly();
    }
}