using TaskLists.Application.Models;
using TaskLists.Application.Models.Queries;
using TaskLists.Application.Models.Queries.TaskLists;
using TaskLists.Domain.Entities;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Application.Abstractions.Repositories;

public interface ITaskListRepository
{
    Task<Result<TaskList?>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<PagedResponse<TaskList>>> GetVisibleToUserAsync(
        Guid userId,
        int page,
        int pageSize,
        TaskListSortField sortBy,
        QuerySortDirection sortDirection,
        CancellationToken cancellationToken);

    Task<Result<TaskList>> GetVisibleToUserByIdAsync(Guid taskListId, Guid userId, CancellationToken cancellationToken);

    Task<Result<bool>> IsTaskListExistsForUserAsync(Guid taskListId, Guid userId, CancellationToken cancellation);

    Task<Result<bool>> IsTaskListExistsAsync(Guid taskListId, CancellationToken cancellation);

    Task<Result<Guid>> AddAsync(TaskList taskList, CancellationToken cancellationToken);

    Task<Result<Guid>> UpdateAsync(TaskList taskList, CancellationToken cancellationToken);

    Task<Result<Guid>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
