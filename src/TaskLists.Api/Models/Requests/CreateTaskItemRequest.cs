namespace TaskLists.Api.Models.Requests;

public sealed record CreateTaskItemRequest(
    string Title,
    string? Description);
