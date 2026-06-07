using TaskLists.Application.Models;
using TaskLists.Domain.Entities;

namespace TaskLists.Application.Extensions;

public static class TaskItemExtensions
{
    public static TaskItemSummary ToTaskItemSummary(this TaskItem taskItem)
    {
        return new TaskItemSummary(
            taskItem.Id,
            taskItem.TaskListId,
            taskItem.Title,
            taskItem.Description,
            taskItem.IsCompleted,
            taskItem.CreatedByUserId,
            taskItem.CreatedAt,
            taskItem.UpdatedAt);
    }
}
