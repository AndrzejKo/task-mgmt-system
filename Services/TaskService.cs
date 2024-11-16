
using TaskManagerApi.Repositories;
using Entities = TaskManagerApi.Models.Entities;

namespace TaskManagerApi.Services;

public class TaskService(ITaskRepository taskRepository) : ITaskService
{
    public async Task<Entities.Task> CreateTaskAsync(Entities.Task newTask)
    {
        return await taskRepository.CreateTaskAsync(newTask);
    }

    public async Task<IEnumerable<Entities.Task>> GetTasksAsync()
    {
        return await taskRepository.GetTasksAsync();
    }

    public async Task UpdateTaskStatusAsync(int taskId, Entities.Status newStatus)
    {
        var existingTask = await taskRepository.GetTaskByIdAsync(taskId);
        if (existingTask == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found.");
        }

        if (existingTask.Status == Entities.Status.Completed)
        {
            throw new InvalidOperationException("Cannot move a task back to InProgress once it is completed.");
        }

        existingTask.Status = newStatus;
        await taskRepository.UpdateTaskAsync(existingTask);
    }
}