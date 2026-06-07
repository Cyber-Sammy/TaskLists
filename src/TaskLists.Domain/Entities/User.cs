namespace TaskLists.Domain.Entities;

public class User : Entity
{
    public string? DisplayName
    {
        get;
        private set => field = NormalizeOptional(value);
    }

    public DateTimeOffset CreatedAt { get; private init; }

    private User(Guid id, string? displayName, DateTimeOffset createdAt)
        : base(id)
    {
        DisplayName = displayName;
        CreatedAt = createdAt;
    }

    internal static User Create(Guid id, string? displayName, DateTimeOffset createdAt)
    {
        return new User(id, displayName, createdAt);
    }

    public static User Restore(Guid id, string? displayName, DateTimeOffset createdAt)
    {
        return new User(id, displayName, createdAt);
    }

    internal void Rename(string? displayName)
    {
        DisplayName = displayName;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
