using TaskLists.Application.Models;
using TaskLists.Domain.Entities;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Application.Abstractions.Repositories;

public interface ITaskItemRepository
{
    Task<Result<TaskItem?>> GetByIdVisibleToUserAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    Task<Result<PagedResponse<TaskItem>>> GetByTaskListIdAsync(Guid taskListId, int page, int pageSize, CancellationToken cancellationToken);

    Task<Result<Guid>> AddAsync(TaskItem taskItem, CancellationToken cancellationToken);

    Task<Result<Guid>> UpdateAsync(TaskItem taskItem, CancellationToken cancellationToken);

    Task<Result<Guid>> DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<long>> DeleteByTaskListIdAsync(Guid taskListId, CancellationToken cancellationToken);
}
