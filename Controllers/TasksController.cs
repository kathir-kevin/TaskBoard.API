using Microsoft.AspNetCore.Mvc;
using TaskBoard.API.DTOs;
using TaskBoard.API.Services;

namespace TaskBoard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService) => _taskService = taskService;

    /// <summary>Get tasks for a specific column</summary>
    [HttpGet("by-column/{columnId:int}")]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), 200)]
    public async Task<IActionResult> GetByColumn(int columnId, CancellationToken ct)
    {
        var tasks = await _taskService.GetByColumnAsync(columnId, ct);
        return Ok(tasks);
    }

    /// <summary>Search/filter tasks within a board</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), 200)]
    public async Task<IActionResult> Search(
        [FromQuery] int boardId,
        [FromQuery] string? q,
        [FromQuery] string? assignedTo,
        [FromQuery] string? priority,
        CancellationToken ct)
    {
        var tasks = await _taskService.SearchAsync(boardId, q, assignedTo, priority, ct);
        return Ok(tasks);
    }

    /// <summary>Get a single task by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var task = await _taskService.GetByIdAsync(id, ct);
        return task is null ? NotFound(new { message = $"Task {id} not found." }) : Ok(task);
    }

    /// <summary>Create a new task in a column</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Task title is required." });

        var task = await _taskService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    /// <summary>Update task details (title, desc, priority, assignee, due date)</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Task title is required." });

        var task = await _taskService.UpdateAsync(id, dto, ct);
        return task is null ? NotFound(new { message = $"Task {id} not found." }) : Ok(task);
    }

    /// <summary>Soft delete a task</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _taskService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound(new { message = $"Task {id} not found." });
    }

    /// <summary>Move a task to a different column / position (handles concurrency via transaction)</summary>
    [HttpPatch("{id:int}/move")]
    [ProducesResponseType(typeof(TaskDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Move(int id, [FromBody] MoveTaskDto dto, CancellationToken ct)
    {
        var task = await _taskService.MoveAsync(id, dto, ct);
        return task is null ? NotFound(new { message = $"Task {id} not found." }) : Ok(task);
    }

    /// <summary>Add a label tag to a task</summary>
    [HttpPost("{taskId:int}/tags")]
    [ProducesResponseType(typeof(TaskDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddTag(int taskId, [FromBody] CreateTagDto dto, CancellationToken ct)
    {
        var task = await _taskService.AddTagAsync(taskId, dto, ct);
        return task is null ? NotFound(new { message = $"Task {taskId} not found." }) : Ok(task);
    }

    /// <summary>Remove a tag from a task</summary>
    [HttpDelete("{taskId:int}/tags/{tagId:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveTag(int taskId, int tagId, CancellationToken ct)
    {
        var removed = await _taskService.RemoveTagAsync(taskId, tagId, ct);
        return removed ? NoContent() : NotFound(new { message = $"Tag {tagId} not found on task {taskId}." });
    }
}
