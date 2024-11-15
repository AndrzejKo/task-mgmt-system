using System.Text.Json.Serialization;

namespace TaskManagerApi.Dtos;

public class UpdateTaskStatusDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Models.TaskStatus NewStatus { get; set; }
}