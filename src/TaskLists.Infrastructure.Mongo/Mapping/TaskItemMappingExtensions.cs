using TaskLists.Domain.Entities;
using TaskLists.Infrastructure.Mongo.Documents;

namespace TaskLists.Infrastructure.Mongo.Mapping;

internal static class TaskItemMappingExtensions
{
    public static TaskItemDocument ToDocument(this TaskItem taskItem)
    {
        return new TaskItemDocument
        {
            Id = taskItem.Id,
            TaskListId = taskItem.TaskListId,
            Title = taskItem.Title,
            Description = taskItem.Description,
            IsCompleted = taskItem.IsCompleted,
            CreatedByUserId = taskItem.CreatedByUserId,
            CreatedAt = taskItem.CreatedAt.ToMongoDateTime(),
            UpdatedAt = taskItem.UpdatedAt.ToMongoDateTime()
        };
    }

    public static TaskItem ToDomain(this TaskItemDocument document)
    {
        return TaskItem.Restore(
            document.Id,
            document.TaskListId,
            document.Title,
            document.Description,
            document.IsCompleted,
            document.CreatedByUserId,
            document.CreatedAt.ToDomainDateTime(),
            document.UpdatedAt.ToDomainDateTime());
    }
}
