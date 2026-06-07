using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaskLists.Api.Constants;
using TaskLists.Api.Extensions;
using TaskLists.Api.Models.Requests;
using TaskLists.Api.Models.Responses;
using TaskLists.Api.Options;
using TaskLists.Application.Abstractions.Services;
using TaskLists.Application.Models.Commands.Users;
using TaskLists.Application.Models.Queries.Users;

namespace TaskLists.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ApiFeaturesOptions _apiFeaturesOptions;

    public UsersController(
        IUserService userService,
        IOptions<ApiFeaturesOptions> apiFeaturesOptions)
    {
        _userService = userService;
        _apiFeaturesOptions = apiFeaturesOptions.Value;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!_apiFeaturesOptions.UsersApiEnabled)
        {
            return Disabled();
        }

        var command = new CreateUserCommand(request.DisplayName);
        var result = await _userService.CreateAsync(command, cancellationToken);

        return result.ToActionResult(user => Created(
            $"/api/users/{user.Id}",
            user.ToResponse()));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        if (!_apiFeaturesOptions.UsersApiEnabled)
        {
            return Disabled();
        }

        var result = await _userService.GetAllAsync(cancellationToken);

        return result.ToActionResult(users => Ok(users.Select(user => user.ToResponse()).ToArray()));
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        if (!_apiFeaturesOptions.UsersApiEnabled)
        {
            return Disabled();
        }

        var query = new GetUserByIdQuery(userId);
        var result = await _userService.GetByIdAsync(query, cancellationToken);

        return result.ToActionResult(user => Ok(user.ToResponse()));
    }

    private static IActionResult Disabled()
    {
        var response = new ErrorResponse(
        [
            new ErrorItemResponse(UserApiConstants.UsersApiIsDisabled, null)
        ]);

        return new NotFoundObjectResult(response);
    }
}
