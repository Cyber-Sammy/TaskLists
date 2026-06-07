using TaskLists.SharedKernel.Result;

namespace TaskLists.Api.Abstractions.Providers;

public interface ICurrentUserProvider
{
    Result<Guid> GetCurrentUserId();
}
