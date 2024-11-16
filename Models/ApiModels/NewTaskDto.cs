using System.ComponentModel.DataAnnotations;

namespace TaskManagerApi.Models.ApiModels;

public class NewTaskDto
{
    [Required, MaxLength(200)] public required string Name { get; set; }
    [MaxLength(1000)] public string Description { get; set; } = string.Empty;
    [MaxLength(50)] public string AssignedTo { get; set; } = string.Empty;
}