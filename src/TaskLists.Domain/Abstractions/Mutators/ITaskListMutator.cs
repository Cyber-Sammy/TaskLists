using TaskLists.Domain.Entities;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Domain.Abstractions.Mutators;

public interface ITaskListMutator
{
    Result Rename(TaskList taskList, string title);
}
