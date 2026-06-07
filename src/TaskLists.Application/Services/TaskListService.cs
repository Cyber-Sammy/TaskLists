using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Application.Abstractions.Services;
using TaskLists.Application.Abstractions.Validators;
using TaskLists.Application.Extensions;
using TaskLists.Application.Models;
using TaskLists.Application.Models.Commands.TaskLists;
using TaskLists.Application.Models.Queries.TaskLists;
using TaskLists.Domain.Abstractions.Factories;
using TaskLists.Domain.Abstractions.Mutators;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Application.Services;

internal class TaskListService : ITaskListService
{
    private readonly ITaskListMemberRepository _taskListMemberRepository;
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly ITaskListRepository _taskListRepository;
    private readonly IDomainEntityFactory _domainEntityFactory;
    private readonly ITaskListMutator _taskListMutator;
    private readonly ITaskListServiceValidator _taskListServiceValidator;

    public TaskListService(
        ITaskListMemberRepository taskListMemberRepository,
        ITaskItemRepository taskItemRepository,
        ITaskListRepository taskListRepository,
        IDomainEntityFactory domainEntityFactory,
        ITaskListMutator taskListMutator,
        ITaskListServiceValidator taskListServiceValidator)
    {
        _taskListMemberRepository = taskListMemberRepository;
        _taskItemRepository = taskItemRepository;
        _taskListRepository = taskListRepository;
        _domainEntityFactory = domainEntityFactory;
        _taskListMutator = taskListMutator;
        _taskListServiceValidator = taskListServiceValidator;
    }

    public async Task<Result<TaskListMemberInfo>> AddMemberAsync(AddTaskListMemberCommand command, CancellationToken cancellationToken)
    {
        var taskListResult = await _taskListRepository.GetByIdAsync(command.TaskListId, cancellationToken);

        if (!taskListResult.Successful)
        {
            return Result<TaskListMemberInfo>.Failure(taskListResult.Status, taskListResult.Errors);
        }

        var taskList = taskListResult.Value;
        var validateOnAddMemberResult = await _taskListServiceValidator.ValidateOnAddMemberAsync(taskList, command, cancellationToken);

        if (!validateOnAddMemberResult.Successful)
        {
            return Result<TaskListMemberInfo>.Failure(validateOnAddMemberResult.Status, validateOnAddMemberResult.Errors);
        }

        var newMemberResult = _domainEntityFactory.TryCreateTaskListMember(
            taskList!.Id,
            command.MemberUserId,
            command.Role,
            command.CurrentUserId);

        if (!newMemberResult.Successful)
        {
            return Result<TaskListMemberInfo>.Failure(newMemberResult.Status, newMemberResult.Errors);
        }

        var newMember = newMemberResult.Value;
        var taskListMemberCreationResult = await _taskListMemberRepository.AddAsync(newMember, cancellationToken);

        if (!taskListMemberCreationResult.Successful)
        {
            return Result<TaskListMemberInfo>.Failure(taskListMemberCreationResult.Status, taskListMemberCreationResult.Errors);
        }

        return Result<TaskListMemberInfo>.Success(new TaskListMemberInfo(
            newMember.Id,
            newMember.TaskListId,
            newMember.MemberUserId,
            newMember.Role,
            newMember.CreatedAt,
            newMember.CreatedByUserId));
    }

    public async Task<Result<TaskListDetails>> CreateAsync(CreateTaskListCommand command, CancellationToken cancellationToken)
    {
        var taskListResult = _domainEntityFactory.TryCreateTaskList(command.Title, command.CurrentUserId);

        if (!taskListResult.Successful)
        {
            return Result<TaskListDetails>.Failure(taskListResult.Status, taskListResult.Errors);
        }

        var taskListAddResult = await _taskListRepository.AddAsync(taskListResult.Value, cancellationToken);

        if (!taskListAddResult.Successful)
        {
            return Result<TaskListDetails>.Failure(taskListAddResult.Status, taskListAddResult.Errors);
        }

        return Result<TaskListDetails>.Success(taskListResult.Value.ToTaskListDetails());
    }

    public async Task<Result<Guid>> DeleteAsync(DeleteTaskListCommand command, CancellationToken cancellationToken)
    {
        var taskListResult = await _taskListRepository.GetByIdAsync(command.TaskListId, cancellationToken);

        if (!taskListResult.Successful)
        {
            return Result<Guid>.Failure(taskListResult.Status, taskListResult.Errors);
        }

        var validationResult = await _taskListServiceValidator.ValidateOnDeleteTaskListAsync(taskListResult.Value, command, cancellationToken);

        if (!validationResult.Successful)
        {
            return Result<Guid>.Failure(validationResult.Status, validationResult.Errors);
        }

        var deleteTaskItemsResult = await _taskItemRepository.DeleteByTaskListIdAsync(
            command.TaskListId,
            cancellationToken);

        if (!deleteTaskItemsResult.Successful)
        {
            return Result<Guid>.Failure(deleteTaskItemsResult.Status, deleteTaskItemsResult.Errors);
        }

        var deleteMembersResult = await _taskListMemberRepository.DeleteByTaskListIdAsync(
            command.TaskListId,
            cancellationToken);

        if (!deleteMembersResult.Successful)
        {
            return Result<Guid>.Failure(deleteMembersResult.Status, deleteMembersResult.Errors);
        }

        var deleteResult = await _taskListRepository.DeleteAsync(command.TaskListId, cancellationToken);

        if (!deleteResult.Successful)
        {
            return Result<Guid>.Failure(deleteResult.Status, deleteResult.Errors);
        }

        return Result<Guid>.Success(deleteResult.Value);
    }

    public async Task<Result<TaskListDetails>> GetByIdAsync(GetTaskListByIdQuery query, CancellationToken cancellationToken)
    {
        var taskListResult = await _taskListRepository.GetVisibleToUserByIdAsync(query.TaskListId, query.CurrentUserId, cancellationToken);

        if (!taskListResult.Successful)
        {
            return Result<TaskListDetails>.Failure(taskListResult.Status, taskListResult.Errors);
        }

        if (taskListResult.Value is null)
        {
            return Result<TaskListDetails>.NotFound(Constants.TaskListServiceConstants.TaskListWasNotFound);
        }

        return Result<TaskListDetails>.Success(taskListResult.Value.ToTaskListDetails());
    }

    public async Task<Result<IReadOnlyCollection<TaskListMemberInfo>>> GetMembersAsync(GetTaskListMembersQuery query, CancellationToken cancellationToken)
    {
        var isTaskListExistsResult = await _taskListRepository.IsTaskListExistsForUserAsync(query.TaskListId, query.CurrentUserId, cancellationToken);

        if (!isTaskListExistsResult.Successful)
        {
            return Result<IReadOnlyCollection<TaskListMemberInfo>>.Failure(isTaskListExistsResult.Status, isTaskListExistsResult.Errors);
        }

        if (!isTaskListExistsResult.Value)
        {
            return Result<IReadOnlyCollection<TaskListMemberInfo>>.NotFound(Constants.TaskListServiceConstants.TaskListWasNotFound);
        }

        var taskListMembersResult = await _taskListMemberRepository.GetByTaskListIdAsync(query.TaskListId, cancellationToken);

        if (!taskListMembersResult.Successful)
        {
            return Result<IReadOnlyCollection<TaskListMemberInfo>>.Failure(taskListMembersResult.Status, taskListMembersResult.Errors);
        }

        return Result<IReadOnlyCollection<TaskListMemberInfo>>.Success(taskListMembersResult.Value.Select(tlm => tlm.ToTaskListMemberInfo()).ToList().AsReadOnly());
    }

    public async Task<Result<PagedResponse<TaskListSummary>>> GetPagedAsync(GetTaskListsQuery query, CancellationToken cancellationToken)
    {
        var paginationValidationResult = ValidatePagination(query.Page, query.PageSize);

        if (!paginationValidationResult.Successful)
        {
            return Result<PagedResponse<TaskListSummary>>.Failure(
                paginationValidationResult.Status,
                paginationValidationResult.Errors);
        }

        var taskListsResult = await _taskListRepository.GetVisibleToUserAsync(
            query.CurrentUserId,
            query.Page,
            query.PageSize,
            query.SortBy,
            query.SortDirection,
            cancellationToken);

        if (!taskListsResult.Successful)
        {
            return Result<PagedResponse<TaskListSummary>>.Failure(taskListsResult.Status, taskListsResult.Errors);
        }

        var taskLists = taskListsResult.Value;
        var response = new PagedResponse<TaskListSummary>(
            taskLists.Items.Select(taskList => taskList.ToTaskListSummary()).ToArray(),
            taskLists.Page,
            taskLists.PageSize,
            taskLists.TotalCount);

        return Result<PagedResponse<TaskListSummary>>.Success(response);
    }

    public async Task<Result> RemoveMemberAsync(RemoveTaskListMemberCommand command, CancellationToken cancellationToken)
    {
        var taskListResult = await _taskListRepository.GetByIdAsync(command.TaskListId, cancellationToken);

        if (!taskListResult.Successful)
        {
            return Result.Failure(taskListResult.Status, taskListResult.Errors);
        }

        var validationResult = await _taskListServiceValidator.ValidateOnRemoveMemberAsync(
            taskListResult.Value,
            command,
            cancellationToken);

        if (!validationResult.Successful)
        {
            return Result.Failure(validationResult.Status, validationResult.Errors);
        }

        var deleteResult = await _taskListMemberRepository.DeleteAsync(
            command.TaskListId,
            command.MemberUserId,
            cancellationToken);

        if (!deleteResult.Successful)
        {
            return Result.Failure(deleteResult.Status, deleteResult.Errors);
        }

        return Result.Success();
    }

    public async Task<Result<TaskListDetails>> UpdateAsync(UpdateTaskListCommand command, CancellationToken cancellationToken)
    {
        var taskListResult = await _taskListRepository.GetByIdAsync(command.TaskListId, cancellationToken);

        if (!taskListResult.Successful)
        {
            return Result<TaskListDetails>.Failure(taskListResult.Status, taskListResult.Errors);
        }

        var taskListToUpdate = taskListResult.Value;

        var validationResult = await _taskListServiceValidator.ValidateOnUpdateTaskListAsync(
            taskListToUpdate,
            command,
            cancellationToken);

        if (!validationResult.Successful)
        {
            return Result<TaskListDetails>.Failure(validationResult.Status, validationResult.Errors);
        }

        var renameResult = _taskListMutator.Rename(taskListToUpdate!, command.Title);

        if (!renameResult.Successful)
        {
            return Result<TaskListDetails>.Failure(renameResult.Status, renameResult.Errors);
        }

        var updateResult = await _taskListRepository.UpdateAsync(taskListToUpdate!, cancellationToken);

        if (!updateResult.Successful)
        {
            return Result<TaskListDetails>.Failure(updateResult.Status, updateResult.Errors);
        }

        return Result<TaskListDetails>.Success(taskListToUpdate!.ToTaskListDetails());
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
