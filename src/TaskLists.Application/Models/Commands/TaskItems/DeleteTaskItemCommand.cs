namespace TaskLists.Application.Models.Commands.TaskItems;

public sealed record DeleteTaskItemCommand(
    Guid CurrentUserId,
    Guid TaskItemId);
