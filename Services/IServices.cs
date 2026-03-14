using TaskBoard.API.DTOs;

namespace TaskBoard.API.Services;

public interface IBoardService
{
    Task<IEnumerable<BoardSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<BoardDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<BoardDto> CreateAsync(CreateBoardDto dto, CancellationToken ct = default);
    Task<BoardDto?> UpdateAsync(int id, UpdateBoardDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public interface IColumnService
{
    Task<ColumnDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ColumnDto> CreateAsync(CreateColumnDto dto, CancellationToken ct = default);
    Task<ColumnDto?> UpdateAsync(int id, UpdateColumnDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> MoveAsync(int id, MoveColumnDto dto, CancellationToken ct = default);
}

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetByColumnAsync(int columnId, CancellationToken ct = default);
    Task<IEnumerable<TaskDto>> SearchAsync(int boardId, string? q, string? assignedTo, string? priority, CancellationToken ct = default);
    Task<TaskDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TaskDto> CreateAsync(CreateTaskDto dto, CancellationToken ct = default);
    Task<TaskDto?> UpdateAsync(int id, UpdateTaskDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<TaskDto?> MoveAsync(int id, MoveTaskDto dto, CancellationToken ct = default);
    Task<TaskDto?> AddTagAsync(int taskId, CreateTagDto dto, CancellationToken ct = default);
    Task<bool> RemoveTagAsync(int taskId, int tagId, CancellationToken ct = default);
}
