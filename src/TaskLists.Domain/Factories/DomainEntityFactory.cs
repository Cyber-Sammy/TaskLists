using TaskLists.Domain.Abstractions.Factories;
using TaskLists.Domain.Abstractions.Ids;
using TaskLists.Domain.Abstractions.Time;
using TaskLists.Domain.Entities;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Domain.Factories;

public sealed class DomainEntityFactory : IDomainEntityFactory
{
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public DomainEntityFactory(IIdGenerator idGenerator, IClock clock)
    {
        _idGenerator = idGenerator;
        _clock = clock;
    }

    public TaskList CreateTaskList(string title, Guid ownerUserId)
    {
        return TaskList.Create(
            title,
            ownerUserId,
            _idGenerator.NewId(),
            _clock.UtcNow);
    }

    public Result<TaskList> TryCreateTaskList(string title, Guid ownerUserId)
    {
        return TryCreate(() => CreateTaskList(title, ownerUserId));
    }

    public TaskItem CreateTaskItem(
        Guid taskListId,
        string title,
        string? description,
        Guid createdByUserId)
    {
        return TaskItem.Create(
            taskListId,
            title,
            description,
            createdByUserId,
            _idGenerator.NewId(),
            _clock.UtcNow);
    }

    public Result<TaskItem> TryCreateTaskItem(
        Guid taskListId,
        string title,
        string? description,
        Guid createdByUserId)
    {
        return TryCreate(() => CreateTaskItem(taskListId, title, description, createdByUserId));
    }

    public TaskListMember CreateTaskListMember(
        Guid taskListId,
        Guid memberUserId,
        TaskListMemberRole role,
        Guid createdByUserId)
    {
        return TaskListMember.Create(
            taskListId,
            memberUserId,
            role,
            createdByUserId,
            _idGenerator.NewId(),
            _clock.UtcNow);
    }

    public Result<TaskListMember> TryCreateTaskListMember(
        Guid taskListId,
        Guid memberUserId,
        TaskListMemberRole role,
        Guid createdByUserId)
    {
        return TryCreate(() => CreateTaskListMember(taskListId, memberUserId, role, createdByUserId));
    }

    public User CreateUser(string? displayName)
    {
        return User.Create(
            _idGenerator.NewId(),
            displayName,
            _clock.UtcNow);
    }

    public Result<User> TryCreateUser(string? displayName)
    {
        return TryCreate(() => CreateUser(displayName));
    }

    private static Result<T> TryCreate<T>(Func<T> create)
    {
        try
        {
            return Result<T>.Success(create());
        }
        catch (ArgumentException ex)
        {
            return Result<T>.ValidationError(ex.Message);
        }
    }
}
