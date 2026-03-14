using Microsoft.EntityFrameworkCore;
using TaskBoard.API.Data;
using TaskBoard.API.DTOs;
using TaskBoard.API.Models;

namespace TaskBoard.API.Services;

public class BoardService : IBoardService
{
    private readonly AppDbContext _db;

    public BoardService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<BoardSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Boards
            .AsNoTracking()
            .Select(b => new BoardSummaryDto(
                b.Id,
                b.Name,
                b.Description,
                b.CreatedAt,
                b.Columns.Count,
                b.Columns.Sum(c => c.Tasks.Count)
            ))
            .ToListAsync(ct);
    }

    public async Task<BoardDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var board = await _db.Boards
            .AsNoTracking()
            .Include(b => b.Columns.OrderBy(c => c.Order))
                .ThenInclude(c => c.Tasks.Where(t => !t.IsDeleted).OrderBy(t => t.Order))
                    .ThenInclude(t => t.Tags)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return board is null ? null : MapBoardToDto(board);
    }

    public async Task<BoardDto> CreateAsync(CreateBoardDto dto, CancellationToken ct = default)
    {
        var board = new Board
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Boards.Add(board);
        await _db.SaveChangesAsync(ct);

        return MapBoardToDto(board);
    }

    public async Task<BoardDto?> UpdateAsync(int id, UpdateBoardDto dto, CancellationToken ct = default)
    {
        var board = await _db.Boards
            .Include(b => b.Columns.OrderBy(c => c.Order))
                .ThenInclude(c => c.Tasks.Where(t => !t.IsDeleted).OrderBy(t => t.Order))
                    .ThenInclude(t => t.Tags)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (board is null) return null;

        board.Name = dto.Name.Trim();
        board.Description = dto.Description?.Trim();
        board.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return MapBoardToDto(board);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var board = await _db.Boards.FindAsync(new object[] { id }, ct);
        if (board is null) return false;

        _db.Boards.Remove(board);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static BoardDto MapBoardToDto(Board board) => new(
        board.Id,
        board.Name,
        board.Description,
        board.CreatedAt,
        board.UpdatedAt,
        board.Columns.Select(c => new ColumnDto(
            c.Id,
            c.Name,
            c.Order,
            c.BoardId,
            c.CreatedAt,
            c.Tasks.Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.Description,
                t.Priority,
                t.Order,
                t.AssignedTo,
                t.DueDate,
                t.ColumnId,
                c.Name,
                t.CreatedAt,
                t.UpdatedAt,
                t.Tags.Select(tg => new TaskTagDto(tg.Id, tg.Label, tg.Color))
            ))
        ))
    );
}
