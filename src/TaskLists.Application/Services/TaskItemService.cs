using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Application.Abstractions.Services;
using TaskLists.Application.Abstractions.Validators;
using TaskLists.Application.Extensions;
using TaskLists.Application.Models;
using TaskLists.Application.Models.Commands.TaskItems;
using TaskLists.Application.Models.Queries.TaskItems;
using TaskLists.Domain.Abstractions.Factories;
using TaskLists.Domain.Abstractions.Mutators;
using TaskLists.Domain.Entities;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Application.Services;

internal class TaskItemService : ITaskItemService
{
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly ITaskItemMutator _taskItemMutator;
    private readonly IDomainEntityFactory _domainEntityFactory;
    private readonly ITaskItemServiceValidator _taskItemServiceValidator;

    public TaskItemService(
        ITaskItemRepository taskItemRepository,
        ITaskItemMutator taskItemMutator,
        IDomainEntityFactory domainEntityFactory,
        ITaskItemServiceValidator taskItemServiceValidator)
    {
        _taskItemRepository = taskItemRepository;
        _taskItemMutator = taskItemMutator;
        _domainEntityFactory = domainEntityFactory;
        _taskItemServiceValidator = taskItemServiceValidator;
    }

    public async Task<Result<TaskItemSummary>> CompleteAsync(CompleteTaskItemCommand command, CancellationToken cancellationToken)
    {
        return await MutateAsync(
            command.TaskItemId,
            command.CurrentUserId,
            _taskItemMutator.Complete,
            cancellationToken);
    }

    public async Task<Result<TaskItemSummary>> CreateAsync(CreateTaskItemCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _taskItemServiceValidator.ValidateOnCreateAsync(command, cancellationToken);

        if (!validationResult.Successful)
        {
            return Result<TaskItemSummary>.Failure(validationResult.Status, validationResult.Errors);
        }

        var taskItemResult = _domainEntityFactory.TryCreateTaskItem(
            command.TaskListId,
            command.Title,
            command.Description,
            command.CurrentUserId);

        if (!taskItemResult.Successful)
        {
            return Result<TaskItemSummary>.Failure(taskItemResult.Status, taskItemResult.Errors);
        }

        var taskItemAddResult = await _taskItemRepository.AddAsync(taskItemResult.Value, cancellationToken);

        if (!taskItemAddResult.Successful)
        {
            return Result<TaskItemSummary>.Failure(taskItemAddResult.Status, taskItemAddResult.Errors);
        }

        return Result<TaskItemSummary>.Success(taskItemResult.Value.ToTaskItemSummary());
    }

    public async Task<Result> DeleteAsync(DeleteTaskItemCommand command, CancellationToken cancellationToken)
    {
        var taskItemResult = await _taskItemRepository.GetByIdVisibleToUserAsync(
            command.TaskItemId,
            command.CurrentUserId,
            cancellationToken);

        if (!taskItemResult.Successful)
        {
            return Result.Failure(taskItemResult.Status, taskItemResult.Errors);
        }

        if (taskItemResult.Value is null)
        {
            return Result.NotFound(Constants.TaskItemServiceConstants.TaskItemWasNotFound);
        }

        var deleteResult = await _taskItemRepository.DeleteAsync(command.TaskItemId, cancellationToken);

        if (!deleteResult.Successful)
        {
            return Result.Failure(deleteResult.Status, deleteResult.Errors);
        }

        return Result.Success();
    }

    public async Task<Result<TaskItemSummary>> GetByIdAsync(GetTaskItemByIdQuery query, CancellationToken cancellationToken)
    {
        var taskItemResult = await _taskItemRepository.GetByIdVisibleToUserAsync(
            query.TaskItemId,
            query.CurrentUserId,
            cancellationToken);

        if (!taskItemResult.Successful)
        {
            return Result<TaskItemSummary>.Failure(taskItemResult.Status, taskItemResult.Errors);
        }

        if (taskItemResult.Value is null)
        {
            return Result<TaskItemSummary>.NotFound(Constants.TaskItemServiceConstants.TaskItemWasNotFound);
        }

        return Result<TaskItemSummary>.Success(taskItemResult.Value.ToTaskItemSummary());
    }

    public async Task<Result<PagedResponse<TaskItemSummary>>> GetPagedByTaskListAsync(GetTaskItemsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _taskItemServiceValidator.ValidateOnGetPagedAsync(query, cancellationToken);

        if (!validationResult.Successful)
        {
            return Result<PagedResponse<TaskItemSummary>>.Failure(validationResult.Status, validationResult.Errors);
        }

        var taskItemsResult = await _taskItemRepository.GetByTaskListIdAsync(
            query.TaskListId,
            query.Page,
            query.PageSize,
            cancellationToken);

        if (!taskItemsResult.Successful)
        {
            return Result<PagedResponse<TaskItemSummary>>.Failure(taskItemsResult.Status, taskItemsResult.Errors);
        }

        var taskItems = taskItemsResult.Value;
        var response = new PagedResponse<TaskItemSummary>(
            taskItems.Items.Select(taskItem => taskItem.ToTaskItemSummary()).ToArray(),
            taskItems.Page,
            taskItems.PageSize,
            taskItems.TotalCount);

        return Result<PagedResponse<TaskItemSummary>>.Success(response);
    }

    public async Task<Result<TaskItemSummary>> ReopenAsync(ReopenTaskItemCommand command, CancellationToken cancellationToken)
    {
        return await MutateAsync(
            command.TaskItemId,
            command.CurrentUserId,
            _taskItemMutator.Reopen,
            cancellationToken);
    }

    public async Task<Result<TaskItemSummary>> UpdateAsync(UpdateTaskItemCommand command, CancellationToken cancellationToken)
    {
        return await MutateAsync(
            command.TaskItemId,
            command.CurrentUserId,
            taskItem => _taskItemMutator.Update(taskItem, command.Title, command.Description),
            cancellationToken);
    }

    private async Task<Result<TaskItemSummary>> MutateAsync(
        Guid taskItemId,
        Guid currentUserId,
        Func<TaskItem, Result> mutator,
        CancellationToken cancellationToken)
    {
        var taskItemResult = await _taskItemRepository.GetByIdVisibleToUserAsync(
            taskItemId,
            currentUserId,
            cancellationToken);

        if (!taskItemResult.Successful)
        {
            return Result<TaskItemSummary>.Failure(taskItemResult.Status, taskItemResult.Errors);
        }

        if (taskItemResult.Value is null)
        {
            return Result<TaskItemSummary>.NotFound(Constants.TaskItemServiceConstants.TaskItemWasNotFound);
        }

        var mutationResult = mutator(taskItemResult.Value);

        if (!mutationResult.Successful)
        {
            return Result<TaskItemSummary>.Failure(mutationResult.Status, mutationResult.Errors);
        }

        var taskItemUpdateResult = await _taskItemRepository.UpdateAsync(taskItemResult.Value, cancellationToken);

        if (!taskItemUpdateResult.Successful)
        {
            return Result<TaskItemSummary>.Failure(taskItemUpdateResult.Status, taskItemUpdateResult.Errors);
        }

        return Result<TaskItemSummary>.Success(taskItemResult.Value.ToTaskItemSummary());
    }
}
