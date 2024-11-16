
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

    public async Task<IEnumerable<Entities.Task>> GetTasksAsync()
    {
        return await taskDbContext.Tasks.ToListAsync();
    }

    public async Task UpdateTaskStatus(int taskId, Entities.Status newStatus)
    {
        var existingTask = await taskDbContext.Tasks.FindAsync(taskId);
        if (existingTask == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found.");
        }

        existingTask.Status = newStatus;
        taskDbContext.Tasks.Update(existingTask);
        await taskDbContext.SaveChangesAsync();
    }
}