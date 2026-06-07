namespace TaskLists.Application.Models.Commands.TaskItems;

public sealed record ReopenTaskItemCommand(
    Guid CurrentUserId,
    Guid TaskItemId);
