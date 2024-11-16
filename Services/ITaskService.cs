using Entities = TaskManagerApi.Models.Entities;

namespace TaskManagerApi.Services;

public interface ITaskService
{
    Task<IEnumerable<Entities.Task>> GetTasksAsync();
    Task<Entities.Task> CreateTaskAsync(Entities.Task newTask);
    Task UpdateTaskStatus(int taskId, Entities.Status newStatus);
}