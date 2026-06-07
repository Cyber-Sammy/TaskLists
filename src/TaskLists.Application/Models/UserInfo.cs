namespace TaskLists.Application.Models;

public sealed record UserInfo(
    Guid Id,
    string? DisplayName,
    DateTimeOffset CreatedAt);
