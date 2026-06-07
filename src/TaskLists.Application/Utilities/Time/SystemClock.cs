using TaskLists.Domain.Abstractions.Time;

namespace TaskLists.Application.Utilities.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
