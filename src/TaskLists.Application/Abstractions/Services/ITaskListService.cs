using TaskLists.Application.Models;
using TaskLists.Application.Models.Commands.TaskLists;
using TaskLists.Application.Models.Queries.TaskLists;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Application.Abstractions.Services;

public interface ITaskListService
{
    Task<Result<TaskListDetails>> CreateAsync(CreateTaskListCommand command, CancellationToken cancellationToken);

    Task<Result<TaskListDetails>> GetByIdAsync(GetTaskListByIdQuery query, CancellationToken cancellationToken);

    Task<Result<PagedResponse<TaskListSummary>>> GetPagedAsync(GetTaskListsQuery query, CancellationToken cancellationToken);

    Task<Result<TaskListDetails>> UpdateAsync(UpdateTaskListCommand command, CancellationToken cancellationToken);

    Task<Result<Guid>> DeleteAsync(DeleteTaskListCommand command, CancellationToken cancellationToken);

    Task<Result<TaskListMemberInfo>> AddMemberAsync(AddTaskListMemberCommand command, CancellationToken cancellationToken);

    Task<Result<IReadOnlyCollection<TaskListMemberInfo>>> GetMembersAsync(GetTaskListMembersQuery query, CancellationToken cancellationToken);

    Task<Result> RemoveMemberAsync(RemoveTaskListMemberCommand command, CancellationToken cancellationToken);
}
