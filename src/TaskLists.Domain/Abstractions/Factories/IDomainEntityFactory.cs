using TaskLists.Domain.Entities;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Domain.Abstractions.Factories;

public interface IDomainEntityFactory
{
    TaskList CreateTaskList(string title, Guid ownerUserId);

    Result<TaskList> TryCreateTaskList(string title, Guid ownerUserId);

    TaskItem CreateTaskItem(
        Guid taskListId,
        string title,
        string? description,
        Guid createdByUserId);

    Result<TaskItem> TryCreateTaskItem(
        Guid taskListId,
        string title,
        string? description,
        Guid createdByUserId);

    TaskListMember CreateTaskListMember(
        Guid taskListId,
        Guid memberUserId,
        TaskListMemberRole role,
        Guid createdByUserId);

    Result<TaskListMember> TryCreateTaskListMember(
        Guid taskListId,
        Guid memberUserId,
        TaskListMemberRole role,
        Guid createdByUserId);

    User CreateUser(string? displayName);

    Result<User> TryCreateUser(string? displayName);
}
