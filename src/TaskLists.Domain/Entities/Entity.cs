namespace TaskLists.Domain.Entities;

public abstract class Entity
{
    public Guid Id
    {
        get;
        private init => field = RequireNotDefault(value, nameof(Id));
    }

    protected Entity(Guid id)
    {
        Id = id;
    }

    protected static string RequireNotBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(Constants.DomainValidationMessages.ValueCannotBeEmpty, parameterName);
        }

        return value;
    }

    protected static Guid RequireNotDefault(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(Constants.DomainValidationMessages.ValueCannotBeDefault, parameterName);
        }

        return value;
    }
}
