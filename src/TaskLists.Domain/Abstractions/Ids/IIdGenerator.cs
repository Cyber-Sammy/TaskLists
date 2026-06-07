namespace TaskLists.Domain.Abstractions.Ids;

public interface IIdGenerator
{
    Guid NewId();
}
