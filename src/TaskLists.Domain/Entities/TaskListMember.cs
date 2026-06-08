namespace TaskLists.Domain.Entities;

public class TaskListMember : Entity
{
    public Guid TaskListId
    {
        get;
        private init => field = RequireNotDefault(value, nameof(TaskListId));
    }

    public Guid MemberUserId
    {
        get;
        private init => field = RequireNotDefault(value, nameof(MemberUserId));
    }

    public TaskListMemberRole Role { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }

    public Guid CreatedByUserId
    {
        get;
        private init => field = RequireNotDefault(value, nameof(CreatedByUserId));
    }

    private TaskListMember(
        Guid id,
        Guid taskListId,
        Guid memberUserId,
        TaskListMemberRole role,
        DateTimeOffset createdAt,
        Guid createdByUserId)
        : base(id)
    {
        TaskListId = taskListId;
        MemberUserId = memberUserId;
        Role = role;
        CreatedAt = createdAt;
        CreatedByUserId = createdByUserId;
    }

    internal static TaskListMember Create(
        Guid taskListId,
        Guid memberUserId,
        TaskListMemberRole role,
        Guid createdByUserId,
        Guid id,
        DateTimeOffset createdAt)
    {
        return new TaskListMember(id, taskListId, memberUserId, role, createdAt, createdByUserId);
    }

    public static TaskListMember Restore(
        Guid id,
        Guid taskListId,
        Guid memberUserId,
        TaskListMemberRole role,
        DateTimeOffset createdAt,
        Guid createdByUserId)
    {
        return new TaskListMember(id, taskListId, memberUserId, role, createdAt, createdByUserId);
    }

}
