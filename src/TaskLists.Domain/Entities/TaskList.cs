namespace TaskLists.Domain.Entities;

public class TaskList : TitledEntity
{
    public Guid OwnerUserId
    {
        get;
        private init => field = RequireNotDefault(value, nameof(OwnerUserId));
    }

    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset UpdatedAt { get; private set; }

    protected override int MaxTitleLength => Constants.TaskListRules.MaxTitleLength;

    protected override string TitleDisplayName => Constants.TaskListRules.DefaultTaskListTitle;

    private TaskList(
        Guid id,
        string title,
        Guid ownerUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        : base(id, title)
    {
        OwnerUserId = ownerUserId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    internal static TaskList Create(
        string title,
        Guid ownerUserId,
        Guid id,
        DateTimeOffset createdAt)
    {
        return new TaskList(id, title, ownerUserId, createdAt, createdAt);
    }

    public static TaskList Restore(
        Guid id,
        string title,
        Guid ownerUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new TaskList(id, title, ownerUserId, createdAt, updatedAt);
    }

    internal void Rename(string title, DateTimeOffset updatedAt)
    {
        Title = title;
        UpdatedAt = updatedAt;
    }
}
