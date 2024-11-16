using System.Text.Json.Serialization;

namespace TaskManagerApi.Models.Actions;

public class UpdateTaskStatusAction
{
    public int TaskId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Entities.Status NewStatus { get; set; }
}