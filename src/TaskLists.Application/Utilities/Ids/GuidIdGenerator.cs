using TaskLists.Domain.Abstractions.Ids;

namespace TaskLists.Application.Utilities.Ids;

public sealed class GuidIdGenerator : IIdGenerator
{
    public Guid NewId()
    {
        return Guid.NewGuid();
    }
}
