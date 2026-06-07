using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Application.Abstractions.Validators;
using TaskLists.Application.Models.Commands.TaskLists;
using TaskLists.Domain.Entities;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Application.Validators;

internal sealed class TaskListServiceValidator : ITaskListServiceValidator
{
    private readonly ITaskListMemberRepository _taskListMemberRepository;
    private readonly IUserRepository _userRepository;

    public TaskListServiceValidator(
        ITaskListMemberRepository taskListMemberRepository,
        IUserRepository userRepository)
    {
        _taskListMemberRepository = taskListMemberRepository;
        _userRepository = userRepository;
    }

    public async Task<Result> ValidateOnAddMemberAsync(TaskList? taskList, AddTaskListMemberCommand command, CancellationToken cancellationToken)
    {
        if (taskList is null)
        {
            return Result.NotFound(Constants.TaskListServiceConstants.TaskListWasNotFound);
        }

        var canAccessResult = await CanAccessTaskListAsync(command.CurrentUserId, taskList, cancellationToken);

        if (!canAccessResult.Successful)
        {
            return Result.Failure(canAccessResult.Status, canAccessResult.Errors);
        }

        if (!canAccessResult.Value)
        {
            return Result.Forbidden(Constants.TaskListServiceConstants.UserHasNoAccess);
        }

        var futureMemberResult = await _userRepository.GetByIdAsync(command.MemberUserId, cancellationToken);

        if (!futureMemberResult.Successful)
        {
            return Result.Failure(futureMemberResult.Status, futureMemberResult.Errors);
        }

        if (futureMemberResult.Value is null)
        {
            return Result.NotFound(Constants.SharedConstants.UserWasNotFound);
        }

        if (taskList.OwnerUserId == command.MemberUserId)
        {
            return Result.Conflict(Constants.TaskListServiceConstants.UnableAddOwnerAsMember);
        }

        var alreadyMemberResult = await _taskListMemberRepository.ExistsAsync(command.TaskListId, command.MemberUserId, cancellationToken);

        if (!alreadyMemberResult.Successful)
        {
            return Result.Failure(alreadyMemberResult.Status, alreadyMemberResult.Errors);
        }

        if (alreadyMemberResult.Value)
        {
            return Result.Conflict(Constants.TaskListServiceConstants.UserAlreadyMember);
        }

        return Result.Success();
    }

    public Task<Result> ValidateOnDeleteTaskListAsync(TaskList? taskList, DeleteTaskListCommand command, CancellationToken cancellationToken)
    {
        if (taskList is null)
        {
            return Task.FromResult(Result.NotFound(Constants.TaskListServiceConstants.TaskListWasNotFound));
        }

        if (taskList.OwnerUserId != command.CurrentUserId)
        {
            return Task.FromResult(Result.Forbidden(Constants.TaskListServiceConstants.OnlyOwnerCanDeleteTaskList));
        }

        return Task.FromResult(Result.Success());
    }

    public async Task<Result> ValidateOnUpdateTaskListAsync(
        TaskList? taskList,
        UpdateTaskListCommand command,
        CancellationToken cancellationToken)
    {
        if (taskList is null)
        {
            return Result.NotFound(Constants.TaskListServiceConstants.TaskListWasNotFound);
        }

        var canAccessResult = await CanAccessTaskListAsync(command.CurrentUserId, taskList, cancellationToken);

        if (!canAccessResult.Successful)
        {
            return Result.Failure(canAccessResult.Status, canAccessResult.Errors);
        }

        if (!canAccessResult.Value)
        {
            return Result.Forbidden(Constants.TaskListServiceConstants.UserHasNoAccess);
        }

        return Result.Success();
    }

    public async Task<Result<TaskListMember>> ValidateOnRemoveMemberAsync(TaskList? taskList, RemoveTaskListMemberCommand command, CancellationToken cancellationToken)
    {
        if (taskList is null)
        {
            return Result<TaskListMember>.NotFound(Constants.TaskListServiceConstants.TaskListWasNotFound);
        }

        var canAccessResult = await CanAccessTaskListAsync(command.CurrentUserId, taskList, cancellationToken);

        if (!canAccessResult.Successful)
        {
            return Result<TaskListMember>.Failure(canAccessResult.Status, canAccessResult.Errors);
        }

        if (!canAccessResult.Value)
        {
            return Result<TaskListMember>.Forbidden(Constants.TaskListServiceConstants.UserHasNoAccess);
        }

        var memberUserExistsResult = await _userRepository.ExistsAsync(command.MemberUserId, cancellationToken);

        if (!memberUserExistsResult.Successful)
        {
            return Result<TaskListMember>.Failure(memberUserExistsResult.Status, memberUserExistsResult.Errors);
        }

        if (!memberUserExistsResult.Value)
        {
            return Result<TaskListMember>.NotFound(Constants.SharedConstants.UserWasNotFound);
        }

        if (taskList.OwnerUserId == command.MemberUserId)
        {
            return Result<TaskListMember>.Conflict(Constants.TaskListServiceConstants.OwnerCannotBeRemoved);
        }

        var memberResult = await _taskListMemberRepository.GetByTaskListAndUserAsync(
            command.TaskListId,
            command.MemberUserId,
            cancellationToken);

        if (!memberResult.Successful)
        {
            return Result<TaskListMember>.Failure(memberResult.Status, memberResult.Errors);
        }

        if (memberResult.Value is null)
        {
            return Result<TaskListMember>.NotFound(Constants.TaskListServiceConstants.MemberWasNotFound);
        }

        return Result<TaskListMember>.Success(memberResult.Value);
    }

    private async Task<Result<bool>> CanAccessTaskListAsync(Guid userId, TaskList taskList, CancellationToken cancellationToken)
    {
        if (taskList.OwnerUserId == userId)
        {
            return Result<bool>.Success(true);
        }

        return await _taskListMemberRepository.ExistsAsync(taskList.Id, userId, cancellationToken);
    }
}
