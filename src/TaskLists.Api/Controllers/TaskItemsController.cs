using Microsoft.AspNetCore.Mvc;
using TaskLists.Api.Abstractions.Providers;
using TaskLists.Api.Extensions;
using TaskLists.Api.Models.Requests;
using TaskLists.Application.Abstractions.Services;
using TaskLists.Application.Models.Commands.TaskItems;
using TaskLists.Application.Models.Queries.TaskItems;

namespace TaskLists.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class TaskItemsController : ControllerBase
{
    private readonly ITaskItemService _taskItemService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public TaskItemsController(
        ITaskItemService taskItemService,
        ICurrentUserProvider currentUserProvider)
    {
        _taskItemService = taskItemService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet("task-lists/{taskListId:guid}/tasks")]
    public async Task<IActionResult> GetPagedByTaskListAsync(
        [FromRoute] Guid taskListId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var query = new GetTaskItemsQuery(
            currentUserIdResult.Value,
            taskListId,
            page,
            pageSize);

        var result = await _taskItemService.GetPagedByTaskListAsync(query, cancellationToken);

        return result.ToActionResult(taskItems => Ok(taskItems.ToResponse()));
    }

    [HttpPost("task-lists/{taskListId:guid}/tasks")]
    public async Task<IActionResult> CreateAsync(
        [FromRoute] Guid taskListId,
        [FromBody] CreateTaskItemRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var command = new CreateTaskItemCommand(
            currentUserIdResult.Value,
            taskListId,
            request.Title,
            request.Description);

        var result = await _taskItemService.CreateAsync(command, cancellationToken);

        return result.ToActionResult(taskItem => Created(
            $"/api/tasks/{taskItem.Id}",
            taskItem.ToResponse()));
    }

    [HttpGet("tasks/{taskItemId:guid}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] Guid taskItemId,
        CancellationToken cancellationToken)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var query = new GetTaskItemByIdQuery(
            currentUserIdResult.Value,
            taskItemId);

        var result = await _taskItemService.GetByIdAsync(query, cancellationToken);

        return result.ToActionResult(taskItem => Ok(taskItem.ToResponse()));
    }

    [HttpPut("tasks/{taskItemId:guid}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid taskItemId,
        [FromBody] UpdateTaskItemRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var command = new UpdateTaskItemCommand(
            currentUserIdResult.Value,
            taskItemId,
            request.Title,
            request.Description);

        var result = await _taskItemService.UpdateAsync(command, cancellationToken);

        return result.ToActionResult(taskItem => Ok(taskItem.ToResponse()));
    }

    [HttpDelete("tasks/{taskItemId:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid taskItemId,
        CancellationToken cancellationToken)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var command = new DeleteTaskItemCommand(
            currentUserIdResult.Value,
            taskItemId);

        var result = await _taskItemService.DeleteAsync(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("tasks/{taskItemId:guid}/complete")]
    public async Task<IActionResult> CompleteAsync(
        [FromRoute] Guid taskItemId,
        CancellationToken cancellationToken)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var command = new CompleteTaskItemCommand(
            currentUserIdResult.Value,
            taskItemId);

        var result = await _taskItemService.CompleteAsync(command, cancellationToken);

        return result.ToActionResult(taskItem => Ok(taskItem.ToResponse()));
    }

    [HttpPost("tasks/{taskItemId:guid}/reopen")]
    public async Task<IActionResult> ReopenAsync(
        [FromRoute] Guid taskItemId,
        CancellationToken cancellationToken)
    {
        var currentUserIdResult = _currentUserProvider.GetCurrentUserId();

        if (!currentUserIdResult.Successful)
        {
            return currentUserIdResult.ToActionResult();
        }

        var command = new ReopenTaskItemCommand(
            currentUserIdResult.Value,
            taskItemId);

        var result = await _taskItemService.ReopenAsync(command, cancellationToken);

        return result.ToActionResult(taskItem => Ok(taskItem.ToResponse()));
    }
}
