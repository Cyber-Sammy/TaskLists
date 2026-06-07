using TaskLists.Application.Models.Queries;

namespace TaskLists.Application.Models.Queries.TaskLists;

public sealed record GetTaskListsQuery(
    Guid CurrentUserId,
    int Page,
    int PageSize,
    TaskListSortField SortBy = TaskListSortField.CreatedAt,
    QuerySortDirection SortDirection = QuerySortDirection.Descending);
