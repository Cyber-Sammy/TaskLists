using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskLists.Infrastructure.Mongo.Documents;

internal sealed class TaskItemDocument
{
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid TaskListId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
