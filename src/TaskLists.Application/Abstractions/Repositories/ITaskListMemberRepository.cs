using TaskLists.Domain.Entities;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Application.Abstractions.Repositories;

public interface ITaskListMemberRepository
{
    Task<Result<TaskListMember?>> GetByTaskListAndUserAsync(Guid taskListId, Guid memberUserId, CancellationToken cancellationToken);

    Task<Result<IReadOnlyCollection<TaskListMember>>> GetByTaskListIdAsync(Guid taskListId, CancellationToken cancellationToken);

    Task<Result<bool>> ExistsAsync(Guid taskListId, Guid memberUserId, CancellationToken cancellationToken);

    Task<Result<Guid>> AddAsync(TaskListMember member, CancellationToken cancellationToken);

    Task<Result<Guid>> DeleteAsync(Guid taskListId, Guid memberUserId, CancellationToken cancellationToken);

    Task<Result<long>> DeleteByTaskListIdAsync(Guid taskListId, CancellationToken cancellationToken);
}
