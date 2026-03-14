using Microsoft.EntityFrameworkCore;
using TaskBoard.API.Data;
using TaskBoard.API.DTOs;
using TaskBoard.API.Models;

namespace TaskBoard.API.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _db;

    public TaskService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<TaskDto>> GetByColumnAsync(int columnId, CancellationToken ct = default)
    {
        return await _db.Tasks
            .AsNoTracking()
            .Include(t => t.Tags)
            .Include(t => t.Column)
            .Where(t => t.ColumnId == columnId)
            .OrderBy(t => t.Order)
            .Select(t => MapToDto(t))
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<TaskDto>> SearchAsync(
        int boardId, string? q, string? assignedTo, string? priority, CancellationToken ct = default)
    {
        var query = _db.Tasks
            .AsNoTracking()
            .Include(t => t.Tags)
            .Include(t => t.Column)
            .Where(t => t.Column.BoardId == boardId);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => t.Title.Contains(q) || (t.Description != null && t.Description.Contains(q)));

        if (!string.IsNullOrWhiteSpace(assignedTo))
            query = query.Where(t => t.AssignedTo != null && t.AssignedTo.Contains(assignedTo));

        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<TaskPriority>(priority, true, out var p))
            query = query.Where(t => t.Priority == p);

        return await query.OrderBy(t => t.Order).Select(t => MapToDto(t)).ToListAsync(ct);
    }

    public async Task<TaskDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var task = await _db.Tasks
            .AsNoTracking()
            .Include(t => t.Tags)
            .Include(t => t.Column)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        return task is null ? null : MapToDto(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto, CancellationToken ct = default)
    {
        var maxOrder = await _db.Tasks
            .Where(t => t.ColumnId == dto.ColumnId)
            .Select(t => (int?)t.Order)
            .MaxAsync(ct) ?? -1;

        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Priority = dto.Priority,
            ColumnId = dto.ColumnId,
            Order = maxOrder + 1,
            AssignedTo = dto.AssignedTo?.Trim(),
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(ct);

        // Reload with navigation props
        await _db.Entry(task).Reference(t => t.Column).LoadAsync(ct);
        return MapToDto(task);
    }

    public async Task<TaskDto?> UpdateAsync(int id, UpdateTaskDto dto, CancellationToken ct = default)
    {
        var task = await _db.Tasks
            .Include(t => t.Tags)
            .Include(t => t.Column)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (task is null) return null;

        task.Title = dto.Title.Trim();
        task.Description = dto.Description?.Trim();
        task.Priority = dto.Priority;
        task.AssignedTo = dto.AssignedTo?.Trim();
        task.DueDate = dto.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return MapToDto(task);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        // Soft delete
        var task = await _db.Tasks.FindAsync(new object[] { id }, ct);
        if (task is null) return false;

        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<TaskDto?> MoveAsync(int id, MoveTaskDto dto, CancellationToken ct = default)
    {
        // Use a transaction to prevent race conditions
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var task = await _db.Tasks
                .Include(t => t.Column)
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (task is null) return null;

            var oldColumnId = task.ColumnId;

            // Re-order tasks in destination column (excluding moving task)
            var destTasks = await _db.Tasks
                .Where(t => t.ColumnId == dto.TargetColumnId && t.Id != id)
                .OrderBy(t => t.Order)
                .ToListAsync(ct);

            // Insert at desired position
            var insertAt = Math.Clamp(dto.NewOrder, 0, destTasks.Count);
            destTasks.Insert(insertAt, task);

            task.ColumnId = dto.TargetColumnId;

            for (int i = 0; i < destTasks.Count; i++)
                destTasks[i].Order = i;

            // Re-sequence source column if different
            if (oldColumnId != dto.TargetColumnId)
            {
                var srcTasks = await _db.Tasks
                    .Where(t => t.ColumnId == oldColumnId)
                    .OrderBy(t => t.Order)
                    .ToListAsync(ct);
                for (int i = 0; i < srcTasks.Count; i++)
                    srcTasks[i].Order = i;
            }

            task.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            // Reload column name
            await _db.Entry(task).Reference(t => t.Column).LoadAsync(ct);
            return MapToDto(task);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<TaskDto?> AddTagAsync(int taskId, CreateTagDto dto, CancellationToken ct = default)
    {
        var task = await _db.Tasks
            .Include(t => t.Tags)
            .Include(t => t.Column)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct);

        if (task is null) return null;

        task.Tags.Add(new TaskTag { Label = dto.Label.Trim(), Color = dto.Color });
        await _db.SaveChangesAsync(ct);
        return MapToDto(task);
    }

    public async Task<bool> RemoveTagAsync(int taskId, int tagId, CancellationToken ct = default)
    {
        var tag = await _db.TaskTags
            .FirstOrDefaultAsync(tg => tg.Id == tagId && tg.TaskItemId == taskId, ct);

        if (tag is null) return false;

        _db.TaskTags.Remove(tag);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static TaskDto MapToDto(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.Priority, t.Order,
        t.AssignedTo, t.DueDate, t.ColumnId,
        t.Column?.Name ?? string.Empty,
        t.CreatedAt, t.UpdatedAt,
        t.Tags.Select(tg => new TaskTagDto(tg.Id, tg.Label, tg.Color))
    );
}
