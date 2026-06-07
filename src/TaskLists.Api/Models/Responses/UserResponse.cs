namespace TaskLists.Api.Models.Responses;

public sealed record UserResponse(
    Guid Id,
    string? DisplayName,
    DateTimeOffset CreatedAt);
