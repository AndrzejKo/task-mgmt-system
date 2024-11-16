using Entities = TaskManagerApi.Models.Entities;

namespace TaskManagerApi.Repositories;

public interface ITaskRepository
{
    Task<Entities.Task?> GetTaskByIdAsync(int id);
    Task<IEnumerable<Entities.Task>> GetTasksAsync();
    Task<Entities.Task> CreateTaskAsync(Entities.Task newTask);
    Task UpdateTaskAsync(Entities.Task task);
}