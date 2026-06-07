using TaskLists.Application;
using TaskLists.Api.Abstractions.Providers;
using TaskLists.Api.Options;
using TaskLists.Api.Providers;
using TaskLists.Infrastructure.Mongo;

namespace TaskLists.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaskLists(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTaskListsApplication();
        services.AddTaskListsMongoInfrastructure(configuration);
        services.Configure<ApiFeaturesOptions>(configuration.GetSection(ApiFeaturesOptions.SectionName));
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserProvider, HeaderCurrentUserProvider>();

        return services;
    }
}
