using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskLists.Infrastructure.Mongo.Documents;

internal sealed class TaskListDocument
{
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid OwnerUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
