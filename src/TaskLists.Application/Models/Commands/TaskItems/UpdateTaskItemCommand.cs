namespace TaskLists.Application.Models.Commands.TaskItems;

public sealed record UpdateTaskItemCommand(
    Guid CurrentUserId,
    Guid TaskItemId,
    string Title,
    string? Description);
