namespace TaskLists.Application.Models.Commands.TaskItems;

public sealed record CompleteTaskItemCommand(
    Guid CurrentUserId,
    Guid TaskItemId);
