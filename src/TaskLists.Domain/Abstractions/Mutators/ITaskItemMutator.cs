using TaskLists.Domain.Entities;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Domain.Abstractions.Mutators;

public interface ITaskItemMutator
{
    Result Rename(TaskItem taskItem, string title);

    Result Update(TaskItem taskItem, string title, string? description);

    Result Complete(TaskItem taskItem);

    Result Reopen(TaskItem taskItem);
}
