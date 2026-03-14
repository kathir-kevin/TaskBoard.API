using Microsoft.AspNetCore.Mvc;
using TaskBoard.API.DTOs;
using TaskBoard.API.Services;

namespace TaskBoard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BoardsController : ControllerBase
{
    private readonly IBoardService _boardService;

    public BoardsController(IBoardService boardService) => _boardService = boardService;

    /// <summary>Get all boards (summary view)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BoardSummaryDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var boards = await _boardService.GetAllAsync(ct);
        return Ok(boards);
    }

    /// <summary>Get a single board with all columns and tasks</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BoardDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var board = await _boardService.GetByIdAsync(id, ct);
        return board is null ? NotFound(new { message = $"Board {id} not found." }) : Ok(board);
    }

    /// <summary>Create a new board</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BoardDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateBoardDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Board name is required." });

        var board = await _boardService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = board.Id }, board);
    }

    /// <summary>Update board name / description</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BoardDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBoardDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Board name is required." });

        var board = await _boardService.UpdateAsync(id, dto, ct);
        return board is null ? NotFound(new { message = $"Board {id} not found." }) : Ok(board);
    }

    /// <summary>Delete a board and all its columns/tasks</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _boardService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound(new { message = $"Board {id} not found." });
    }
}
