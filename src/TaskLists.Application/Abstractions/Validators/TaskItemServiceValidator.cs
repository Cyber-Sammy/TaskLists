using TaskLists.Application.Models.Commands.TaskItems;
using TaskLists.Application.Models.Queries.TaskItems;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Application.Abstractions.Validators;

internal interface ITaskItemServiceValidator
{
    Task<Result> ValidateOnCreateAsync(CreateTaskItemCommand command, CancellationToken cancellationToken);

    Task<Result> ValidateOnGetPagedAsync(GetTaskItemsQuery query, CancellationToken cancellationToken);
}
