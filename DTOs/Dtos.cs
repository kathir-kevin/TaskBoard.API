using TaskBoard.API.Models;

namespace TaskBoard.API.DTOs;

//  Board DTOs 

public record BoardDto(int Id, string Name, string? Description, DateTime CreatedAt, DateTime UpdatedAt,
    IEnumerable<ColumnDto> Columns);

public record BoardSummaryDto(int Id, string Name, string? Description, DateTime CreatedAt, int ColumnCount, int TaskCount);

public record CreateBoardDto(string Name, string? Description);

public record UpdateBoardDto(string Name, string? Description);

//  Column DTOs 

public record ColumnDto(int Id, string Name, int Order, int BoardId, DateTime CreatedAt, IEnumerable<TaskDto> Tasks);

public record CreateColumnDto(string Name, int BoardId, int? Order);

public record UpdateColumnDto(string Name, int? Order);

public record MoveColumnDto(int NewOrder);

//  Task DTOs 

public record TaskDto(
    int Id,
    string Title,
    string? Description,
    TaskPriority Priority,
    int Order,
    string? AssignedTo,
    DateTime? DueDate,
    int ColumnId,
    string ColumnName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<TaskTagDto> Tags
);

public record CreateTaskDto(
    string Title,
    string? Description,
    TaskPriority Priority,
    int ColumnId,
    string? AssignedTo,
    DateTime? DueDate
);

public record UpdateTaskDto(
    string Title,
    string? Description,
    TaskPriority Priority,
    string? AssignedTo,
    DateTime? DueDate
);

public record MoveTaskDto(int TargetColumnId, int NewOrder);

//  Tag DTOs 

public record TaskTagDto(int Id, string Label, string Color);

public record CreateTagDto(string Label, string Color);

//  Common 

public record ApiResponse<T>(bool Success, T? Data, string? Message = null);

public record ApiError(string Message, int StatusCode, IDictionary<string, string[]>? Errors = null);

public record PaginatedResult<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);
