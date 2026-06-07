using TaskLists.Api.Abstractions.Providers;
using TaskLists.Api.Constants;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Api.Providers;

internal sealed class HeaderCurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderCurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Result<Guid> GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return Result<Guid>.ValidationError(CurrentUserConstants.UserIdHeaderIsRequired);
        }

        if (!httpContext.Request.Headers.TryGetValue(CurrentUserConstants.UserIdHeaderName, out var userIdHeaderValues))
        {
            return Result<Guid>.ValidationError(CurrentUserConstants.UserIdHeaderIsRequired);
        }

        var userIdHeaderValue = userIdHeaderValues.FirstOrDefault();

        if (!Guid.TryParse(userIdHeaderValue, out var userId))
        {
            return Result<Guid>.ValidationError(CurrentUserConstants.UserIdHeaderIsInvalid);
        }

        return Result<Guid>.Success(userId);
    }
}
