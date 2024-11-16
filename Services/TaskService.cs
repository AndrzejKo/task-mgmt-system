
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

    public async Task UpdateTaskStatus(int taskId, Entities.Status newStatus)
    {
        await taskRepository.UpdateTaskStatus(taskId, newStatus);
    }
}