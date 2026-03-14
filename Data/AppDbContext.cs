using Microsoft.EntityFrameworkCore;
using TaskBoard.API.Models;

namespace TaskBoard.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Column> Columns => Set<Column>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskTag> TaskTags => Set<TaskTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Board
        modelBuilder.Entity<Board>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Name).IsRequired().HasMaxLength(200);
            e.Property(b => b.Description).HasMaxLength(1000);
            e.HasMany(b => b.Columns)
             .WithOne(c => c.Board)
             .HasForeignKey(c => c.BoardId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Column
        modelBuilder.Entity<Column>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.HasMany(c => c.Tasks)
             .WithOne(t => t.Column)
             .HasForeignKey(t => t.ColumnId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // TaskItem — soft delete via global query filter
        modelBuilder.Entity<TaskItem>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Title).IsRequired().HasMaxLength(500);
            e.Property(t => t.Description).HasMaxLength(5000);
            e.Property(t => t.AssignedTo).HasMaxLength(200);
            e.HasQueryFilter(t => !t.IsDeleted);
        });

        // TaskTag — explicit relationship + mirrored query filter so EF does not
        // warn about a required-end entity being filtered out by the TaskItem filter
        modelBuilder.Entity<TaskTag>(e =>
        {
            e.HasKey(tt => tt.Id);
            e.Property(tt => tt.Label).IsRequired().HasMaxLength(100);
            e.Property(tt => tt.Color).IsRequired().HasMaxLength(20);

            e.HasOne(tt => tt.TaskItem)
             .WithMany(t => t.Tags)
             .HasForeignKey(tt => tt.TaskItemId)
             .OnDelete(DeleteBehavior.Cascade);

            // Mirror soft-delete filter to silence EF warning
            e.HasQueryFilter(tt => !tt.TaskItem!.IsDeleted);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Fixed timestamp so the migration snapshot stays stable across restarts
        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Board>().HasData(
            new Board { Id = 1, Name = "Product Roadmap", Description = "Q1 sprint planning board", CreatedAt = now, UpdatedAt = now }
        );

        modelBuilder.Entity<Column>().HasData(
            new Column { Id = 1, Name = "Backlog",     Order = 0, BoardId = 1, CreatedAt = now, UpdatedAt = now },
            new Column { Id = 2, Name = "In Progress", Order = 1, BoardId = 1, CreatedAt = now, UpdatedAt = now },
            new Column { Id = 3, Name = "Review",      Order = 2, BoardId = 1, CreatedAt = now, UpdatedAt = now },
            new Column { Id = 4, Name = "Done",        Order = 3, BoardId = 1, CreatedAt = now, UpdatedAt = now }
        );

        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem { Id = 1, Title = "Setup CI/CD Pipeline", Description = "Configure GitHub Actions for automated deployment", Priority = TaskPriority.High,   Order = 0, AssignedTo = "Kathiravan", ColumnId = 2, CreatedAt = now, UpdatedAt = now },
            new TaskItem { Id = 2, Title = "Design System Setup",  Description = "Establish design tokens and component library",     Priority = TaskPriority.Medium, Order = 0, AssignedTo = "Team",       ColumnId = 1, CreatedAt = now, UpdatedAt = now },
            new TaskItem { Id = 3, Title = "API Documentation",    Description = "Write Swagger docs for all endpoints",              Priority = TaskPriority.Low,    Order = 1, AssignedTo = "Kathiravan", ColumnId = 1, CreatedAt = now, UpdatedAt = now }
        );
    }
}
