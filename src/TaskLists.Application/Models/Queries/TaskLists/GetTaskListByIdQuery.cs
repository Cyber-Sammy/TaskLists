namespace TaskLists.Application.Models.Queries.TaskLists;

public sealed record GetTaskListByIdQuery(
    Guid CurrentUserId,
    Guid TaskListId);
