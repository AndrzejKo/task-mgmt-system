using Microsoft.EntityFrameworkCore;
using Entities = TaskManagerApi.Models.Entities;

namespace TaskManagerApi.DataAccess;

public class TaskDbContext(DbContextOptions<TaskDbContext> options) : DbContext(options)
{
    public DbSet<Entities.Task> Tasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entities.Task>()
        .Property(t => t.Id)
        .ValueGeneratedOnAdd();

        modelBuilder.Entity<Entities.Task>()
            .Property(t => t.Status)
            .HasConversion<string>();
    }
}