using TaskLists.Application.Models;
using TaskLists.Application.Models.Commands.Users;
using TaskLists.Application.Models.Queries.Users;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Application.Abstractions.Services;

public interface IUserService
{
    Task<Result<UserInfo>> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken);

    Task<Result<IReadOnlyCollection<UserInfo>>> GetAllAsync(CancellationToken cancellationToken);

    Task<Result<UserInfo>> GetByIdAsync(GetUserByIdQuery query, CancellationToken cancellationToken);
}
