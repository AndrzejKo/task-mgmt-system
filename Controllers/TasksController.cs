using Microsoft.AspNetCore.Mvc;
namespace TaskManagerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;

    public TasksController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }


    [HttpGet(Name = "GetTasks")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Models.Task>))]
    public async Task<IActionResult> Get(){
         var tasks = new List<Models.Task>{
            new Models.Task(){ Id = 1, Name = "Setup env", Status = Models.TaskStatus.NotStarted},
            new Models.Task(){ Id = 1, Name = "Create API", Status = Models.TaskStatus.NotStarted},
            new Models.Task(){ Id = 1, Name = "Add SQL Server", Status = Models.TaskStatus.NotStarted},
            new Models.Task(){ Id = 1, Name = "Add Service Bus", Status = Models.TaskStatus.NotStarted}
        };

        return Ok(tasks);
    }
}
