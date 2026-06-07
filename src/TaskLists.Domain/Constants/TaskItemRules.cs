namespace TaskLists.Domain.Constants;

public static class TaskItemRules
{
    public const int MinTitleLength = 1;
    public const int MaxTitleLength = 255;
    public const int MaxDescriptionLength = 4_000;
    public const string DefaultTaskTitle = "Task title";
    public const string DefaultTaskDescription = "Task description";
}
