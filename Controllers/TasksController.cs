using Microsoft.AspNetCore.Mvc;
using Entities = TaskManagerApi.Models.Entities;
using ApiModels = TaskManagerApi.Models.ApiModels;
using TaskManagerApi.Services;
using TaskManagerApi.Models.Actions;
using TaskManagerApi.Mappers;
namespace TaskManagerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(ILogger<TasksController> logger, ITaskService taskService, ServiceBusHandler serviceBusHandler) : ControllerBase
{
    [HttpGet(Name = "GetTasks")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ApiModels.TaskDto>))]
    public async Task<IActionResult> Get()
    {
        var tasks = await taskService.GetTasksAsync();
        var taskDtos = tasks.Select(TaskMapper.MapTaskToTaskDto);

        return Ok(taskDtos);
    }

    [HttpPost(Name = "PostTasks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PostTask(ApiModels.NewTaskDto newTask)
    {
        var task = new Entities.Task()
        {
            Name = newTask.Name,
            Description = newTask.Description,
            AssignedTo = newTask.AssignedTo,
            Status = Entities.Status.NotStarted
        };

        await taskService.CreateTaskAsync(task);

        return Created();
    }

    [HttpPut("{id}/status", Name = "UpdateTaskStatus")]
    public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] ApiModels.UpdateTaskStatusDto updateTaskStatus)
    {
        var existingTask = await taskService.GetTaskByIdAsync(id);
        if (existingTask == null)
        {
            logger.LogError("Task {Id} not found", id);
            
            return NotFound();
        }

        var updateRequest = new UpdateTaskStatusAction { TaskId = id, NewStatus = updateTaskStatus.NewStatus };

        await serviceBusHandler.SendTaskStatusUpdateActionAsync(updateRequest);

        return Ok();
    }
}
