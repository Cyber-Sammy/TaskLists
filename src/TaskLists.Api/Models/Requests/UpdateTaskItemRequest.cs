namespace TaskLists.Api.Models.Requests;

public sealed record UpdateTaskItemRequest(
    string Title,
    string? Description);
