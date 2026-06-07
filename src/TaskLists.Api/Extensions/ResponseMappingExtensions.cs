using TaskLists.Api.Models.Responses;
using TaskLists.Application.Models;
using ApplicationPagedResponse = TaskLists.Application.Models.PagedResponse<TaskLists.Application.Models.TaskListSummary>;
using ApplicationTaskItemsPagedResponse = TaskLists.Application.Models.PagedResponse<TaskLists.Application.Models.TaskItemSummary>;
using TaskItemsPagedResponse = TaskLists.Api.Models.Responses.PagedResponse<TaskLists.Api.Models.Responses.TaskItemResponse>;
using TaskListsPagedResponse = TaskLists.Api.Models.Responses.PagedResponse<TaskLists.Api.Models.Responses.TaskListSummaryResponse>;

namespace TaskLists.Api.Extensions;

internal static class ResponseMappingExtensions
{
    public static TaskListSummaryResponse ToResponse(this TaskListSummary taskList)
    {
        return new TaskListSummaryResponse(
            taskList.Id,
            taskList.Title,
            taskList.OwnerUserId,
            taskList.CreatedAt,
            taskList.UpdatedAt);
    }

    public static TaskListDetailsResponse ToResponse(this TaskListDetails taskList)
    {
        return new TaskListDetailsResponse(
            taskList.Id,
            taskList.Title,
            taskList.OwnerUserId,
            taskList.CreatedAt,
            taskList.UpdatedAt);
    }

    public static TaskListMemberResponse ToResponse(this TaskListMemberInfo member)
    {
        return new TaskListMemberResponse(
            member.Id,
            member.TaskListId,
            member.MemberUserId,
            member.Role,
            member.CreatedAt,
            member.CreatedByUserId);
    }

    public static TaskItemResponse ToResponse(this TaskItemSummary taskItem)
    {
        return new TaskItemResponse(
            taskItem.Id,
            taskItem.TaskListId,
            taskItem.Title,
            taskItem.Description,
            taskItem.IsCompleted,
            taskItem.CreatedByUserId,
            taskItem.CreatedAt,
            taskItem.UpdatedAt);
    }

    public static UserResponse ToResponse(this UserInfo user)
    {
        return new UserResponse(
            user.Id,
            user.DisplayName,
            user.CreatedAt);
    }

    public static TaskListsPagedResponse ToResponse(this ApplicationPagedResponse pagedResponse)
    {
        return new TaskListsPagedResponse(
            pagedResponse.Items.Select(taskList => taskList.ToResponse()).ToArray(),
            pagedResponse.Page,
            pagedResponse.PageSize,
            pagedResponse.TotalCount,
            pagedResponse.TotalPages);
    }

    public static TaskItemsPagedResponse ToResponse(this ApplicationTaskItemsPagedResponse pagedResponse)
    {
        return new TaskItemsPagedResponse(
            pagedResponse.Items.Select(taskItem => taskItem.ToResponse()).ToArray(),
            pagedResponse.Page,
            pagedResponse.PageSize,
            pagedResponse.TotalCount,
            pagedResponse.TotalPages);
    }
}
