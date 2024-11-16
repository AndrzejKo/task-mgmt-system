namespace TaskManagerApi.Models.Events;

public class TaskStatusUpdatedEvent
{
    public int Id { get; set; }
    public Entities.Status NewStatus { get; set; }
    public DateTime CompletedAtUtc { get; set; }
}