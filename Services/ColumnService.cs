using Microsoft.EntityFrameworkCore;
using TaskBoard.API.Data;
using TaskBoard.API.DTOs;
using TaskBoard.API.Models;

namespace TaskBoard.API.Services;

public class ColumnService : IColumnService
{
    private readonly AppDbContext _db;

    public ColumnService(AppDbContext db) => _db = db;

    public async Task<ColumnDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var col = await _db.Columns
            .AsNoTracking()
            .Include(c => c.Tasks.Where(t => !t.IsDeleted).OrderBy(t => t.Order))
                .ThenInclude(t => t.Tags)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return col is null ? null : MapToDto(col);
    }

    public async Task<ColumnDto> CreateAsync(CreateColumnDto dto, CancellationToken ct = default)
    {
        // Assign order = max + 1 if not specified
        var maxOrder = await _db.Columns
            .Where(c => c.BoardId == dto.BoardId)
            .Select(c => (int?)c.Order)
            .MaxAsync(ct) ?? -1;

        var col = new Column
        {
            Name = dto.Name.Trim(),
            BoardId = dto.BoardId,
            Order = dto.Order ?? maxOrder + 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Columns.Add(col);
        await _db.SaveChangesAsync(ct);

        return MapToDto(col);
    }

    public async Task<ColumnDto?> UpdateAsync(int id, UpdateColumnDto dto, CancellationToken ct = default)
    {
        var col = await _db.Columns
            .Include(c => c.Tasks.Where(t => !t.IsDeleted).OrderBy(t => t.Order))
                .ThenInclude(t => t.Tags)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (col is null) return null;

        col.Name = dto.Name.Trim();
        if (dto.Order.HasValue) col.Order = dto.Order.Value;
        col.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return MapToDto(col);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var col = await _db.Columns.FindAsync(new object[] { id }, ct);
        if (col is null) return false;

        _db.Columns.Remove(col);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> MoveAsync(int id, MoveColumnDto dto, CancellationToken ct = default)
    {
        var col = await _db.Columns.FindAsync(new object[] { id }, ct);
        if (col is null) return false;

        // Re-order siblings
        var siblings = await _db.Columns
            .Where(c => c.BoardId == col.BoardId && c.Id != id)
            .OrderBy(c => c.Order)
            .ToListAsync(ct);

        // Insert at new position
        siblings.Insert(Math.Clamp(dto.NewOrder, 0, siblings.Count), col);

        for (int i = 0; i < siblings.Count; i++)
            siblings[i].Order = i;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static ColumnDto MapToDto(Column col) => new(
        col.Id,
        col.Name,
        col.Order,
        col.BoardId,
        col.CreatedAt,
        col.Tasks.Select(t => new TaskDto(
            t.Id, t.Title, t.Description, t.Priority, t.Order,
            t.AssignedTo, t.DueDate, t.ColumnId, col.Name,
            t.CreatedAt, t.UpdatedAt,
            t.Tags.Select(tg => new TaskTagDto(tg.Id, tg.Label, tg.Color))
        ))
    );
}
