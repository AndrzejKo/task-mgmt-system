using System.Text.Json.Serialization;

namespace TaskManagerApi.Models;

public enum TaskStatus
{
    NotStarted,
    InProgress,
    Completed
}

public class Task
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = String.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))] public required TaskStatus Status { get; set; }
    public string AssignedTo { get; set; } = String.Empty;

    public Task() { }
}