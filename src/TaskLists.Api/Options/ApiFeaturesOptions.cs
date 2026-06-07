namespace TaskLists.Api.Options;

public sealed class ApiFeaturesOptions
{
    public const string SectionName = "ApiFeatures";

    public bool UsersApiEnabled { get; init; }
}
