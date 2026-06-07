namespace TaskLists.Application.Models.Commands.TaskLists;

public sealed record UpdateTaskListCommand(
    Guid CurrentUserId,
    Guid TaskListId,
    string Title);
