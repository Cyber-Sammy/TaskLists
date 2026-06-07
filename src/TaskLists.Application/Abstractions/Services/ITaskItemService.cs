using TaskLists.Application.Models;
using TaskLists.Application.Models.Commands.TaskItems;
using TaskLists.Application.Models.Queries.TaskItems;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Application.Abstractions.Services;

public interface ITaskItemService
{
    Task<Result<TaskItemSummary>> CreateAsync(CreateTaskItemCommand command, CancellationToken cancellationToken);

    Task<Result<TaskItemSummary>> GetByIdAsync(GetTaskItemByIdQuery query, CancellationToken cancellationToken);

    Task<Result<PagedResponse<TaskItemSummary>>> GetPagedByTaskListAsync(GetTaskItemsQuery query, CancellationToken cancellationToken);

    Task<Result<TaskItemSummary>> UpdateAsync(UpdateTaskItemCommand command, CancellationToken cancellationToken);

    Task<Result<TaskItemSummary>> CompleteAsync(CompleteTaskItemCommand command, CancellationToken cancellationToken);

    Task<Result<TaskItemSummary>> ReopenAsync(ReopenTaskItemCommand command, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(DeleteTaskItemCommand command, CancellationToken cancellationToken);
}
