namespace TaskLists.Application.Models.Queries.TaskItems;

public sealed record GetTaskItemsQuery(
    Guid CurrentUserId,
    Guid TaskListId,
    int Page,
    int PageSize);
