namespace TaskLists.Application.Models.Commands.TaskLists;

public sealed record RemoveTaskListMemberCommand(
    Guid CurrentUserId,
    Guid TaskListId,
    Guid MemberUserId);
