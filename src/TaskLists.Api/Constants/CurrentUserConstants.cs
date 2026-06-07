namespace TaskLists.Api.Constants;

internal static class CurrentUserConstants
{
    public const string UserIdHeaderName = "X-User-Id";
    public const string UserIdHeaderIsRequired = "X-User-Id header is required.";
    public const string UserIdHeaderIsInvalid = "X-User-Id header must contain a valid GUID.";
}
