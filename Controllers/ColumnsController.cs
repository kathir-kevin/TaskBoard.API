using Microsoft.AspNetCore.Mvc;
using TaskBoard.API.DTOs;
using TaskBoard.API.Services;

namespace TaskBoard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ColumnsController : ControllerBase
{
    private readonly IColumnService _columnService;

    public ColumnsController(IColumnService columnService) => _columnService = columnService;

    /// <summary>Get a column with its tasks</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ColumnDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var col = await _columnService.GetByIdAsync(id, ct);
        return col is null ? NotFound(new { message = $"Column {id} not found." }) : Ok(col);
    }

    /// <summary>Create a column inside a board</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ColumnDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateColumnDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Column name is required." });

        var col = await _columnService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = col.Id }, col);
    }

    /// <summary>Update a column's name / order</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ColumnDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateColumnDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Column name is required." });

        var col = await _columnService.UpdateAsync(id, dto, ct);
        return col is null ? NotFound(new { message = $"Column {id} not found." }) : Ok(col);
    }

    /// <summary>Delete a column and all its tasks</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _columnService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound(new { message = $"Column {id} not found." });
    }

    /// <summary>Re-order a column within its board</summary>
    [HttpPatch("{id:int}/move")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Move(int id, [FromBody] MoveColumnDto dto, CancellationToken ct)
    {
        var moved = await _columnService.MoveAsync(id, dto, ct);
        return moved ? NoContent() : NotFound(new { message = $"Column {id} not found." });
    }
}
