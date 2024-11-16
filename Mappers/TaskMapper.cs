using Entities = TaskManagerApi.Models.Entities;
using ApiModels = TaskManagerApi.Models.ApiModels;

namespace TaskManagerApi.Mappers;

//TODO: Introduce AutoMapper
public static class TaskMapper
{
    public static ApiModels.TaskDto MapTaskToTaskDto(Entities.Task task)
    {
        return new ApiModels.TaskDto(){
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            Status = task.Status,
            AssignedTo = task.AssignedTo
        };
    }
}