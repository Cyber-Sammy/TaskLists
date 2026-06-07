using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskLists.Infrastructure.Mongo.Documents;

internal sealed class UserDocument
{
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    public string? DisplayName { get; set; }

    public DateTime CreatedAt { get; set; }
}
