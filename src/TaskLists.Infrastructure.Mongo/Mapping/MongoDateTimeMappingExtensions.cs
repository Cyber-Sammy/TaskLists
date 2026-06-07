namespace TaskLists.Infrastructure.Mongo.Mapping;

internal static class MongoDateTimeMappingExtensions
{
    public static DateTime ToMongoDateTime(this DateTimeOffset dateTime)
    {
        return dateTime.UtcDateTime;
    }

    public static DateTimeOffset ToDomainDateTime(this DateTime dateTime)
    {
        return new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
    }
}
