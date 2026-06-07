namespace TaskLists.Application.Models.Commands.TaskItems;

public sealed record CreateTaskItemCommand(
    Guid CurrentUserId,
    Guid TaskListId,
    string Title,
    string? Description);
