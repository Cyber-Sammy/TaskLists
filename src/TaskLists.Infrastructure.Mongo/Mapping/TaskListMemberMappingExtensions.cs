using TaskLists.Domain.Entities;
using TaskLists.Infrastructure.Mongo.Documents;

namespace TaskLists.Infrastructure.Mongo.Mapping;

internal static class TaskListMemberMappingExtensions
{
    public static TaskListMemberDocument ToDocument(this TaskListMember member)
    {
        return new TaskListMemberDocument
        {
            Id = member.Id,
            TaskListId = member.TaskListId,
            MemberUserId = member.MemberUserId,
            Role = member.Role,
            CreatedAt = member.CreatedAt.ToMongoDateTime(),
            CreatedByUserId = member.CreatedByUserId
        };
    }

    public static TaskListMember ToDomain(this TaskListMemberDocument document)
    {
        return TaskListMember.Restore(
            document.Id,
            document.TaskListId,
            document.MemberUserId,
            document.Role,
            document.CreatedAt.ToDomainDateTime(),
            document.CreatedByUserId);
    }
}
