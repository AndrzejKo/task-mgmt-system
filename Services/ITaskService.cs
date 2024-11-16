using Entities = TaskManagerApi.Models.Entities;

namespace TaskManagerApi.Services;

public interface ITaskService
{
    Task<Entities.Task?> GetTaskByIdAsync(int id);
    Task<IEnumerable<Entities.Task>> GetTasksAsync();
    Task<Entities.Task> CreateTaskAsync(Entities.Task newTask);
    Task UpdateTaskStatusAsync(int taskId, Entities.Status newStatus);
}