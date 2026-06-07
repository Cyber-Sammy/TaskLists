namespace TaskLists.Domain.Entities;

public abstract class TitledEntity : Entity
{
    public string Title
    {
        get;
        protected set => field = ValidateTitle(value);
    }

    protected virtual int MaxTitleLength => 255;

    protected virtual string TitleDisplayName => "Title";

    protected TitledEntity(Guid id, string title)
        : base(id)
    {
        Title = title;
    }

    protected virtual string ValidateTitle(string title)
    {
        var value = RequireNotBlank(title, nameof(Title)).Trim();

        if (value.Length > MaxTitleLength)
        {
            throw new ArgumentException(
                Constants.DomainValidationMessages.MaxLengthExceeded(TitleDisplayName, MaxTitleLength),
                nameof(title));
        }

        return value;
    }
}
