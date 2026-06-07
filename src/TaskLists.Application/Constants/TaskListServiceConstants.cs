namespace TaskLists.Application.Constants;

internal class TaskListServiceConstants
{
    public const string UserAlreadyMember = "User is already a member of this task list.";
    public const string TaskListWasNotFound = "Task list was not found.";
    public const string UserHasNoAccess = "User has no access to this task list.";
    public const string UnableAddOwnerAsMember = "Owner cannot be added as a member.";
    public const string OnlyOwnerCanDeleteTaskList = "Only owner can delete this task list.";
    public const string OwnerCannotBeRemoved = "Owner cannot be removed from task list members.";
    public const string MemberWasNotFound = "Task list member was not found.";
    public const string MembersWereNotFound = "Task list members were not found.";
}
