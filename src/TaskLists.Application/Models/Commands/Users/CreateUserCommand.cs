namespace TaskLists.Application.Models.Commands.Users;

public sealed record CreateUserCommand(
    string? DisplayName);
