using TaskLists.Domain.Entities;

namespace TaskLists.Application.Models;

public sealed record TaskListMemberInfo(
    Guid Id,
    Guid TaskListId,
    Guid MemberUserId,
    TaskListMemberRole Role,
    DateTimeOffset CreatedAt,
    Guid CreatedByUserId);
