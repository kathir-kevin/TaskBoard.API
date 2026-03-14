namespace TaskBoard.API.Models;

public class Board
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Column> Columns { get; set; } = new List<Column>();
}

public class Column
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public int BoardId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Board Board { get; set; } = null!;
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public int Order { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public int ColumnId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public Column Column { get; set; } = null!;
    public ICollection<TaskTag> Tags { get; set; } = new List<TaskTag>();
}

public class TaskTag
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";
    public int TaskItemId { get; set; }

    // Navigation
    public TaskItem TaskItem { get; set; } = null!;
}

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
