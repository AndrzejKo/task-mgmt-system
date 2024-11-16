
using Microsoft.EntityFrameworkCore;
using TaskManagerApi.DataAccess;
using Entities = TaskManagerApi.Models.Entities;

namespace TaskManagerApi.Repositories;

public class TaskRepository(TaskDbContext taskDbContext) : ITaskRepository
{
    public async Task<Entities.Task> CreateTaskAsync(Entities.Task newTask)
    {
        await taskDbContext.AddAsync(newTask);
        await taskDbContext.SaveChangesAsync();

        return newTask;
    }

    public async Task<Entities.Task?> GetTaskByIdAsync(int id)
    {
        return await taskDbContext.Tasks.FindAsync(id);
    }

    public async Task<IEnumerable<Entities.Task>> GetTasksAsync()
    {
        return await taskDbContext.Tasks.ToListAsync();
    }

    public async Task UpdateTaskAsync(Entities.Task task)
    {
        taskDbContext.Tasks.Update(task);
        await taskDbContext.SaveChangesAsync();
    }
}