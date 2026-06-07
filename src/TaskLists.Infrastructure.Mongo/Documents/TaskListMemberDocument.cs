using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TaskLists.Domain.Entities;

namespace TaskLists.Infrastructure.Mongo.Documents;

internal sealed class TaskListMemberDocument
{
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid TaskListId { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid MemberUserId { get; set; }

    public TaskListMemberRole Role { get; set; }

    public DateTime CreatedAt { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid CreatedByUserId { get; set; }
}
