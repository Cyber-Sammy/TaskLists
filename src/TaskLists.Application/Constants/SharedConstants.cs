namespace TaskLists.Application.Constants;

public class SharedConstants
{
    public const int MinPage = 1;
    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;

    public const string UserWasNotFound = "User was not found.";
    public const string PageMustBePositive = "Page must be greater than or equal to 1.";
    public const string PageSizeMustBePositive = "Page size must be greater than or equal to 1.";
    public const string PageSizeExceeded = "Page size cannot be greater than 100.";
}
