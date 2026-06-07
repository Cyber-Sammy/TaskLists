using TaskLists.Application.Models;
using TaskLists.Domain.Entities;

namespace TaskLists.Application.Extensions;

public static class TaskListExtensions
{
    public static TaskListDetails ToTaskListDetails(this TaskList taskList)
    {
        return new TaskListDetails(
            taskList.Id,
            taskList.Title,
            taskList.OwnerUserId,
            taskList.CreatedAt,
            taskList.UpdatedAt);
    }

    public static TaskListSummary ToTaskListSummary(this TaskList taskList)
    {
        return new TaskListSummary(
            taskList.Id,
            taskList.Title,
            taskList.OwnerUserId,
            taskList.CreatedAt,
            taskList.UpdatedAt);
    }

    public static TaskListMemberInfo ToTaskListMemberInfo(this TaskListMember taskListMember)
    {
        return new TaskListMemberInfo(
            taskListMember.Id,
            taskListMember.TaskListId,
            taskListMember.MemberUserId,
            taskListMember.Role,
            taskListMember.CreatedAt,
            taskListMember.CreatedByUserId);
    }
}
