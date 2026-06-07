namespace TaskLists.Api.Models.Responses;

public sealed record TaskItemResponse(
    Guid Id,
    Guid TaskListId,
    string Title,
    string? Description,
    bool IsCompleted,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
