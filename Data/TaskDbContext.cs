using Microsoft.EntityFrameworkCore;

namespace TaskManagerApi.Data;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {

    }

    public DbSet<Models.Task> Tasks { get; set; }
}