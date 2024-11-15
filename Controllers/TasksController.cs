using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.Dtos;
namespace TaskManagerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly TaskDbContext _taskDbContext;

    public TasksController(ILogger<WeatherForecastController> logger, TaskDbContext taskDbContext)
    {
        _logger = logger;
        _taskDbContext = taskDbContext;
    }


    [HttpGet(Name = "GetTasks")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Models.Task>))]
    public async Task<IActionResult> Get()
    {
        var tasks = await _taskDbContext.Tasks.ToListAsync();

        return Ok(tasks);
    }


    [HttpPost(Name = "PostTasks")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.Task))]
    public async Task<IActionResult> CreateTask(Models.Task task)
    {
        task.Id = 0;
        task.Status = Models.TaskStatus.NotStarted;

        await _taskDbContext.AddAsync(task);
        await _taskDbContext.SaveChangesAsync();

        return Created();
    }

    [HttpPatch("{id:int}", Name = "UpdateTaskStatus")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.Task))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] UpdateTaskStatusDto updateTaskStatusDto)
    {
        // Fetch the task from the database
        var task = await _taskDbContext.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound($"Task with ID {id} not found.");
        }

        // Update the task status
        task.Status = updateTaskStatusDto.NewStatus;

        try
        {
            await _taskDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating task: {ex.Message}");
        }

        return Ok(task);
    }

}
