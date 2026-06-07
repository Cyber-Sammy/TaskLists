namespace TaskLists.Domain.Abstractions.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
