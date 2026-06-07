namespace TaskLists.Application.Models.Commands.TaskLists;

public sealed record CreateTaskListCommand(
    Guid CurrentUserId,
    string Title);
