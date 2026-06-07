namespace TaskLists.Application.Models.Queries.TaskLists;

public sealed record GetTaskListMembersQuery(
    Guid CurrentUserId,
    Guid TaskListId);
