namespace TaskLists.Application.Models;

public sealed record TaskListDetails(
    Guid Id,
    string Title,
    Guid OwnerUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
