namespace TaskLists.Application.Models;

public sealed record TaskItemSummary(
    Guid Id,
    Guid TaskListId,
    string Title,
    string? Description,
    bool IsCompleted,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
