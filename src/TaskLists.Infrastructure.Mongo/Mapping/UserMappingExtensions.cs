using TaskLists.Domain.Entities;
using TaskLists.Infrastructure.Mongo.Documents;

namespace TaskLists.Infrastructure.Mongo.Mapping;

internal static class UserMappingExtensions
{
    public static UserDocument ToDocument(this User user)
    {
        return new UserDocument
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt.ToMongoDateTime()
        };
    }

    public static User ToDomain(this UserDocument document)
    {
        return User.Restore(
            document.Id,
            document.DisplayName,
            document.CreatedAt.ToDomainDateTime());
    }
}
