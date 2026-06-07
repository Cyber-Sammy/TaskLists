using TaskLists.Domain.Entities;

namespace TaskLists.Api.Models.Requests;

public sealed record AddTaskListMemberRequest(
    Guid MemberUserId,
    TaskListMemberRole Role);
