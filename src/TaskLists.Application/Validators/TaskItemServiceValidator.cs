using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Application.Abstractions.Validators;
using TaskLists.Application.Models.Commands.TaskItems;
using TaskLists.Application.Models.Queries.TaskItems;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Application.Validators;

internal sealed class TaskItemServiceValidator : ITaskItemServiceValidator
{
    private readonly ITaskListRepository _taskListRepository;
    private readonly IUserRepository _userRepository;

    public TaskItemServiceValidator(
        ITaskListRepository taskListRepository,
        IUserRepository userRepository)
    {
        _taskListRepository = taskListRepository;
        _userRepository = userRepository;
    }

    public async Task<Result> ValidateOnCreateAsync(
        CreateTaskItemCommand command,
        CancellationToken cancellationToken)
    {
        var userValidationResult = await ValidateUserExistsAsync(command.CurrentUserId, cancellationToken);

        if (!userValidationResult.Successful)
        {
            return userValidationResult;
        }

        return await ValidateTaskListAccessAsync(command.TaskListId, command.CurrentUserId, cancellationToken);
    }

    public async Task<Result> ValidateOnGetPagedAsync(
        GetTaskItemsQuery query,
        CancellationToken cancellationToken)
    {
        var paginationValidationResult = ValidatePagination(query.Page, query.PageSize);

        if (!paginationValidationResult.Successful)
        {
            return paginationValidationResult;
        }

        var userValidationResult = await ValidateUserExistsAsync(query.CurrentUserId, cancellationToken);

        if (!userValidationResult.Successful)
        {
            return userValidationResult;
        }

        return await ValidateTaskListAccessAsync(query.TaskListId, query.CurrentUserId, cancellationToken);
    }

    private async Task<Result> ValidateUserExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userExistsResult = await _userRepository.ExistsAsync(userId, cancellationToken);

        if (!userExistsResult.Successful)
        {
            return Result.Failure(userExistsResult.Status, userExistsResult.Errors);
        }

        return userExistsResult.Value
            ? Result.Success()
            : Result.NotFound(Constants.SharedConstants.UserWasNotFound);
    }

    private async Task<Result> ValidateTaskListAccessAsync(
        Guid taskListId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var taskListExistsResult = await _taskListRepository.IsTaskListExistsForUserAsync(
            taskListId,
            currentUserId,
            cancellationToken);

        if (!taskListExistsResult.Successful)
        {
            return Result.Failure(taskListExistsResult.Status, taskListExistsResult.Errors);
        }

        if (!taskListExistsResult.Value)
        {
            return Result.NotFound(Constants.TaskListServiceConstants.TaskListWasNotFound);
        }

        return Result.Success();
    }

    private static Result ValidatePagination(int page, int pageSize)
    {
        if (page < Constants.SharedConstants.MinPage)
        {
            return Result.ValidationError(Constants.SharedConstants.PageMustBePositive);
        }

        if (pageSize < Constants.SharedConstants.MinPageSize)
        {
            return Result.ValidationError(Constants.SharedConstants.PageSizeMustBePositive);
        }

        if (pageSize > Constants.SharedConstants.MaxPageSize)
        {
            return Result.ValidationError(Constants.SharedConstants.PageSizeExceeded);
        }

        return Result.Success();
    }
}
