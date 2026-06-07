namespace TaskLists.Application.Models.Queries.TaskItems;

public sealed record GetTaskItemByIdQuery(
    Guid CurrentUserId,
    Guid TaskItemId);
