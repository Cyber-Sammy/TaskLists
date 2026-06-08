using TaskLists.Domain.Abstractions.Mutators;
using TaskLists.Domain.Abstractions.Time;
using TaskLists.Domain.Entities;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Domain.Mutators;

public sealed class TaskItemMutator : ITaskItemMutator
{
    private readonly IClock _clock;

    public TaskItemMutator(IClock clock)
    {
        _clock = clock;
    }

    public Result Complete(TaskItem taskItem)
    {
        return Mutate(taskItem, (item, updatedAt) => item.Complete(updatedAt));
    }

    public Result Reopen(TaskItem taskItem)
    {
        return Mutate(taskItem, (item, updatedAt) => item.Reopen(updatedAt));
    }

    public Result Update(TaskItem taskItem, string title, string? description)
    {
        return Mutate(taskItem, (item, updatedAt) => item.Update(title, description, updatedAt));
    }

    private Result Mutate(TaskItem taskItem, Action<TaskItem, DateTimeOffset> mutation)
    {
        try
        {
            mutation(taskItem, _clock.UtcNow);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.ValidationError(ex.Message);
        }
    }
}
