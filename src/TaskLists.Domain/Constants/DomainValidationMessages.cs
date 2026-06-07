namespace TaskLists.Domain.Constants;

public static class DomainValidationMessages
{
    public const string ValueCannotBeEmpty = "Value cannot be empty.";
    public const string ValueCannotBeDefault = "Value cannot be default.";

    public static string MaxLengthExceeded(string displayName, int maxLength)
    {
        return $"{displayName} must be less than or equal to {maxLength} characters.";
    }
}
