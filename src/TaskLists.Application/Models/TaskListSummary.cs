namespace TaskLists.Application.Models;

public sealed record TaskListSummary(
    Guid Id,
    string Title,
    Guid OwnerUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
