namespace TaskLists.Api.Models.Responses;

public sealed record TaskListSummaryResponse(
    Guid Id,
    string Title,
    Guid OwnerUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
