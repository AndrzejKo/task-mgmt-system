namespace TaskManagerApi.Models.Events;

public class TaskCompletedEvent
{
    public int Id { get; set; }
    public DateTime CompletedAtUtc { get; set; }
}