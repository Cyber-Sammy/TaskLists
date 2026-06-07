using TaskLists.Domain.Entities;

namespace TaskLists.Api.Models.Responses;

public sealed record TaskListMemberResponse(
    Guid Id,
    Guid TaskListId,
    Guid MemberUserId,
    TaskListMemberRole Role,
    DateTimeOffset CreatedAt,
    Guid CreatedByUserId);
