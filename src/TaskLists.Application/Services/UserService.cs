using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Application.Abstractions.Services;
using TaskLists.Application.Constants;
using TaskLists.Application.Extensions;
using TaskLists.Application.Models;
using TaskLists.Application.Models.Commands.Users;
using TaskLists.Application.Models.Queries.Users;
using TaskLists.Domain.Abstractions.Factories;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Application.Services;

internal sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IDomainEntityFactory _domainEntityFactory;

    public UserService(
        IUserRepository userRepository,
        IDomainEntityFactory domainEntityFactory)
    {
        _userRepository = userRepository;
        _domainEntityFactory = domainEntityFactory;
    }

    public async Task<Result<UserInfo>> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var userResult = _domainEntityFactory.TryCreateUser(command.DisplayName);

        if (!userResult.Successful)
        {
            return Result<UserInfo>.Failure(userResult.Status, userResult.Errors);
        }

        var addResult = await _userRepository.AddAsync(userResult.Value, cancellationToken);

        if (!addResult.Successful)
        {
            return Result<UserInfo>.Failure(addResult.Status, addResult.Errors);
        }

        return Result<UserInfo>.Success(userResult.Value.ToUserInfo());
    }

    public async Task<Result<IReadOnlyCollection<UserInfo>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var usersResult = await _userRepository.GetAllAsync(cancellationToken);

        if (!usersResult.Successful)
        {
            return Result<IReadOnlyCollection<UserInfo>>.Failure(usersResult.Status, usersResult.Errors);
        }

        var users = usersResult.Value
            .Select(user => user.ToUserInfo())
            .ToArray();

        return Result<IReadOnlyCollection<UserInfo>>.Success(users);
    }

    public async Task<Result<UserInfo>> GetByIdAsync(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var userResult = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);

        if (!userResult.Successful)
        {
            return Result<UserInfo>.Failure(userResult.Status, userResult.Errors);
        }

        if (userResult.Value is null)
        {
            return Result<UserInfo>.NotFound(SharedConstants.UserWasNotFound);
        }

        return Result<UserInfo>.Success(userResult.Value.ToUserInfo());
    }
}
