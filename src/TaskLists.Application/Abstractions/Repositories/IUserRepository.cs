using TaskLists.Domain.Entities;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<Result<IReadOnlyCollection<User>>> GetAllAsync(CancellationToken cancellationToken);

    Task<Result<User?>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<Guid>> AddAsync(User user, CancellationToken cancellationToken);
}
