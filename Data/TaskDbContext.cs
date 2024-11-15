using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TaskManagerApi.Data;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {

    }

    public DbSet<Models.Task> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.Task>()
        .Property(t => t.Id)
        .ValueGeneratedOnAdd();

        modelBuilder.Entity<Models.Task>()
            .Property(t => t.Status)
            .HasConversion<string>();
    }
}