namespace TaskLists.Domain.Entities;

public class TaskItem : TitledEntity
{
    public Guid TaskListId
    {
        get;
        private init => field = RequireNotDefault(value, nameof(TaskListId));
    }

    public string? Description
    {
        get;
        private set => field = ValidateDescription(value);
    }

    public bool IsCompleted { get; private set; }

    public Guid CreatedByUserId
    {
        get;
        private init => field = RequireNotDefault(value, nameof(CreatedByUserId));
    }

    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset UpdatedAt { get; private set; }

    protected override int MaxTitleLength => Constants.TaskItemRules.MaxTitleLength;

    protected override string TitleDisplayName => Constants.TaskItemRules.DefaultTaskTitle;

    private TaskItem(
        Guid id,
        Guid taskListId,
        string title,
        string? description,
        bool isCompleted,
        Guid createdByUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        : base(id, title)
    {
        TaskListId = taskListId;
        Description = description;
        IsCompleted = isCompleted;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    internal static TaskItem Create(
        Guid taskListId,
        string title,
        string? description,
        Guid createdByUserId,
        Guid id,
        DateTimeOffset createdAt)
    {
        return new TaskItem(
            id,
            taskListId,
            title,
            description,
            isCompleted: false,
            createdByUserId,
            createdAt,
            createdAt);
    }

    public static TaskItem Restore(
        Guid id,
        Guid taskListId,
        string title,
        string? description,
        bool isCompleted,
        Guid createdByUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new TaskItem(
            id,
            taskListId,
            title,
            description,
            isCompleted,
            createdByUserId,
            createdAt,
            updatedAt);
    }

    internal void Rename(string title, DateTimeOffset updatedAt)
    {
        Title = title;
        UpdatedAt = updatedAt;
    }

    internal void ChangeDescription(string? description, DateTimeOffset updatedAt)
    {
        Description = description;
        UpdatedAt = updatedAt;
    }

    internal void Update(string title, string? description, DateTimeOffset updatedAt)
    {
        var validatedTitle = ValidateTitle(title);
        var validatedDescription = ValidateDescription(description);

        Title = validatedTitle;
        Description = validatedDescription;
        UpdatedAt = updatedAt;
    }

    internal void Complete(DateTimeOffset updatedAt)
    {
        IsCompleted = true;
        UpdatedAt = updatedAt;
    }

    internal void Reopen(DateTimeOffset updatedAt)
    {
        IsCompleted = false;
        UpdatedAt = updatedAt;
    }

    private static string? ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var value = description.Trim();

        if (value.Length > Constants.TaskItemRules.MaxDescriptionLength)
        {
            throw new ArgumentException(
                Constants.DomainValidationMessages.MaxLengthExceeded(
                    Constants.TaskItemRules.DefaultTaskDescription,
                    Constants.TaskItemRules.MaxDescriptionLength),
                nameof(description));
        }

        return value;
    }
}
