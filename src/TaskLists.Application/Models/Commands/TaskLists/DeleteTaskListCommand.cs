namespace TaskLists.Application.Models.Commands.TaskLists;

public sealed record DeleteTaskListCommand(
    Guid CurrentUserId,
    Guid TaskListId);
