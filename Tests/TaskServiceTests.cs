using FakeItEasy;
using TaskManagerApi.Repositories;
using TaskManagerApi.Services;
using Xunit;
using Entities = TaskManagerApi.Models.Entities;

namespace TaskManagerApi.Tests;

public class TaskServiceTests
    {
        private readonly ITaskRepository fakeTaskRepository;
        private readonly TaskService taskService;

        public TaskServiceTests()
        {
            fakeTaskRepository = A.Fake<ITaskRepository>();
            taskService = new TaskService(fakeTaskRepository);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnTask_WhenTaskExists()
        {
            // Arrange
            var taskId = 1;
            var task = new Entities.Task { Id = taskId, Name = "Test Task", Status = Entities.Status.InProgress };
            A.CallTo(() => fakeTaskRepository.GetTaskByIdAsync(taskId)).Returns(task);

            // Act
            var result = await taskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.Equal(task, result);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnNull_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskId = 1;
            A.CallTo(() => fakeTaskRepository.GetTaskByIdAsync(taskId)).Returns(null as Entities.Task);

            // Act
            var result = await taskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_ShouldUpdateStatus_WhenTaskExists()
        {
            // Arrange
            var taskId = 1;
            var existingTask = new Entities.Task { Id = taskId, Name = "Test Task", Status = Entities.Status.InProgress };
            var newStatus = Entities.Status.Completed;
            A.CallTo(() => fakeTaskRepository.GetTaskByIdAsync(taskId)).Returns(existingTask);
            A.CallTo(() => fakeTaskRepository.UpdateTaskAsync(existingTask)).Returns(Task.CompletedTask);

            // Act
            await taskService.UpdateTaskStatusAsync(taskId, newStatus);

            // Assert
            A.CallTo(() => fakeTaskRepository.UpdateTaskAsync(A<Entities.Task>.That.Matches(t => t.Status == newStatus))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_ShouldThrowKeyNotFoundException_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskId = 1;
            var newStatus = Entities.Status.Completed;
            A.CallTo(() => fakeTaskRepository.GetTaskByIdAsync(taskId)).Returns(null as Entities.Task);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => taskService.UpdateTaskStatusAsync(taskId, newStatus));
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_ShouldThrowInvalidOperationException_WhenTaskIsCompleted()
        {
            // Arrange
            var taskId = 1;
            var existingTask = new Entities.Task { Id = taskId, Name = "Test Task", Status = Entities.Status.Completed };
            var newStatus = Entities.Status.InProgress;
            A.CallTo(() => fakeTaskRepository.GetTaskByIdAsync(taskId)).Returns(existingTask);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => taskService.UpdateTaskStatusAsync(taskId, newStatus));
        }
    }