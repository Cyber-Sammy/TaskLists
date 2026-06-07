using Microsoft.AspNetCore.Mvc;
using TaskLists.Api.Abstractions.Providers;
using TaskLists.Api.Extensions;
using TaskLists.Api.Models.Requests;
using TaskLists.Application.Abstractions.Services;
using TaskLists.Application.Models.Commands.TaskLists;
using TaskLists.Application.Models.Queries;
using TaskLists.Application.Models.Queries.TaskLists;

namespace TaskLists.Api.Controllers;

[ApiController]
[Route("api/task-lists")]
public sealed class TaskListsController : ControllerBase
{
    private readonly ITaskListService _taskListService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public TaskListsController(
        ITaskListService taskListService,
        ICurrentUserProvider currentUserProvider)
    {
        _taskListService = taskListService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] TaskListSortField sortBy = TaskListSortField.CreatedAt,
        [FromQuery] QuerySortDirection sortDirection = QuerySortDirection.Descending,
        CancellationToken cancellationToken = default)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var query = new GetTaskListsQuery(
            currentUserIdResult.Value,
            page,
            pageSize,
            sortBy,
            sortDirection);

        var result = await _taskListService.GetPagedAsync(query, cancellationToken);

        return result.ToActionResult(taskLists => Ok(taskLists.ToResponse()));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateTaskListRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var command = new CreateTaskListCommand(
            currentUserIdResult.Value,
            request.Title);

        var result = await _taskListService.CreateAsync(command, cancellationToken);

        return result.ToActionResult(taskList => Created(
            $"/api/task-lists/{taskList.Id}",
            taskList.ToResponse()));
    }

    [HttpGet("{taskListId:guid}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] Guid taskListId,
        CancellationToken cancellationToken)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var query = new GetTaskListByIdQuery(
            currentUserIdResult.Value,
            taskListId);

        var result = await _taskListService.GetByIdAsync(query, cancellationToken);

        return result.ToActionResult(taskList => Ok(taskList.ToResponse()));
    }

    [HttpPut("{taskListId:guid}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid taskListId,
        [FromBody] UpdateTaskListRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var command = new UpdateTaskListCommand(
            currentUserIdResult.Value,
            taskListId,
            request.Title);

        var result = await _taskListService.UpdateAsync(command, cancellationToken);

        return result.ToActionResult(taskList => Ok(taskList.ToResponse()));
    }

    [HttpDelete("{taskListId:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid taskListId,
        CancellationToken cancellationToken)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var command = new DeleteTaskListCommand(
            currentUserIdResult.Value,
            taskListId);

        var result = await _taskListService.DeleteAsync(command, cancellationToken);

        return result.ToActionResult(_ => NoContent());
    }

    [HttpGet("{taskListId:guid}/members")]
    public async Task<IActionResult> GetMembersAsync(
        [FromRoute] Guid taskListId,
        CancellationToken cancellationToken)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var query = new GetTaskListMembersQuery(
            currentUserIdResult.Value,
            taskListId);

        var result = await _taskListService.GetMembersAsync(query, cancellationToken);

        return result.ToActionResult(members => Ok(members.Select(member => member.ToResponse()).ToArray()));
    }

    [HttpPost("{taskListId:guid}/members")]
    public async Task<IActionResult> AddMemberAsync(
        [FromRoute] Guid taskListId,
        [FromBody] AddTaskListMemberRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var command = new AddTaskListMemberCommand(
            currentUserIdResult.Value,
            taskListId,
            request.MemberUserId,
            request.Role);

        var result = await _taskListService.AddMemberAsync(command, cancellationToken);

        return result.ToActionResult(member => Ok(member.ToResponse()));
    }

    [HttpDelete("{taskListId:guid}/members/{memberUserId:guid}")]
    public async Task<IActionResult> RemoveMemberAsync(
        [FromRoute] Guid taskListId,
        [FromRoute] Guid memberUserId,
        CancellationToken cancellationToken)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var command = new RemoveTaskListMemberCommand(
            currentUserIdResult.Value,
            taskListId,
            memberUserId);

        var result = await _taskListService.RemoveMemberAsync(command, cancellationToken);

        return result.ToActionResult();
    }
}
