using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagerApi.Models.ApiModels;

public class TaskDto
{
    public int Id { get; set; }

    [Required, MaxLength(200)] public required string Name { get; set; }

    [MaxLength(1000)] public string Description { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [Column(TypeName = "nvarchar(30)")]
    public required Entities.Status Status { get; set; }

    [MaxLength(50)] public string AssignedTo { get; set; } = string.Empty;
}