using TaskLists.Domain.Abstractions.Mutators;
using TaskLists.Domain.Abstractions.Time;
using TaskLists.Domain.Entities;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Domain.Mutators;

public sealed class TaskListMutator : ITaskListMutator
{
    private readonly IClock _clock;

    public TaskListMutator(IClock clock)
    {
        _clock = clock;
    }

    public Result Rename(TaskList taskList, string title)
    {
        try
        {
            taskList.Rename(title, _clock.UtcNow);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.ValidationError(ex.Message);
        }
    }
}
