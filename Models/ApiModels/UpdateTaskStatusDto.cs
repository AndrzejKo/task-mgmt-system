using System.Text.Json.Serialization;

namespace TaskManagerApi.Models.ApiModels;

public class UpdateTaskStatusDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Entities.Status NewStatus { get; set; }
}