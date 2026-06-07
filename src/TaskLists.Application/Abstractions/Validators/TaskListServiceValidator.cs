using TaskLists.Application.Models.Commands.TaskLists;
using TaskLists.Domain.Entities;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Application.Abstractions.Validators;

internal interface ITaskListServiceValidator
{
    Task<Result> ValidateOnAddMemberAsync(TaskList? taskList, AddTaskListMemberCommand command, CancellationToken cancellationToken);

    Task<Result> ValidateOnDeleteTaskListAsync(TaskList? taskList, DeleteTaskListCommand command, CancellationToken cancellationToken);

    Task<Result> ValidateOnUpdateTaskListAsync(TaskList? taskList, UpdateTaskListCommand command, CancellationToken cancellationToken);

    Task<Result<TaskListMember>> ValidateOnRemoveMemberAsync(TaskList? taskList, RemoveTaskListMemberCommand command, CancellationToken cancellationToken);
}
