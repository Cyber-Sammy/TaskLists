using TaskLists.Domain.Entities;
using TaskLists.Infrastructure.Mongo.Documents;

namespace TaskLists.Infrastructure.Mongo.Mapping;

internal static class TaskListMappingExtensions
{
    public static TaskListDocument ToDocument(this TaskList taskList)
    {
        return new TaskListDocument
        {
            Id = taskList.Id,
            Title = taskList.Title,
            OwnerUserId = taskList.OwnerUserId,
            CreatedAt = taskList.CreatedAt.ToMongoDateTime(),
            UpdatedAt = taskList.UpdatedAt.ToMongoDateTime()
        };
    }

    public static TaskList ToDomain(this TaskListDocument document)
    {
        return TaskList.Restore(
            document.Id,
            document.Title,
            document.OwnerUserId,
            document.CreatedAt.ToDomainDateTime(),
            document.UpdatedAt.ToDomainDateTime());
    }
}
