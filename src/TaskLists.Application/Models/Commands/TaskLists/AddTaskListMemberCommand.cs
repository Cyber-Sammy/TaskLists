using TaskLists.Domain.Entities;

namespace TaskLists.Application.Models.Commands.TaskLists;

public sealed record AddTaskListMemberCommand(
    Guid CurrentUserId,
    Guid TaskListId,
    Guid MemberUserId,
    TaskListMemberRole Role);
