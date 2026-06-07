namespace TaskLists.Api.Models.Responses;

public sealed record TaskListDetailsResponse(
    Guid Id,
    string Title,
    Guid OwnerUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
