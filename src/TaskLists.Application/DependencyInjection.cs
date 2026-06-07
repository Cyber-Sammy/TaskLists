using Microsoft.Extensions.DependencyInjection;
using TaskLists.Application.Abstractions.Services;
using TaskLists.Application.Abstractions.Validators;
using TaskLists.Application.Services;
using TaskLists.Application.Utilities.Ids;
using TaskLists.Application.Utilities.Time;
using TaskLists.Application.Validators;
using TaskLists.Domain.Abstractions.Factories;
using TaskLists.Domain.Abstractions.Ids;
using TaskLists.Domain.Abstractions.Mutators;
using TaskLists.Domain.Abstractions.Time;
using TaskLists.Domain.Factories;
using TaskLists.Domain.Mutators;

namespace TaskLists.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTaskListsApplication(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IIdGenerator, GuidIdGenerator>();

        services.AddScoped<IDomainEntityFactory, DomainEntityFactory>();
        services.AddScoped<ITaskListMutator, TaskListMutator>();
        services.AddScoped<ITaskItemMutator, TaskItemMutator>();

        services.AddScoped<ITaskListService, TaskListService>();
        services.AddScoped<ITaskItemService, TaskItemService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<ITaskListServiceValidator, TaskListServiceValidator>();
        services.AddScoped<ITaskItemServiceValidator, TaskItemServiceValidator>();

        return services;
    }
}
