using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Infrastructure.Mongo.Constants;
using TaskLists.Infrastructure.Mongo.Documents;
using TaskLists.Infrastructure.Mongo.Options;
using TaskLists.Infrastructure.Mongo.Repositories;

namespace TaskLists.Infrastructure.Mongo;

public static class DependencyInjection
{
    public static IServiceCollection AddTaskListsMongoInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<MongoDbOptions>()
            .Bind(configuration.GetSection(MongoDbOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "MongoDb connection string is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.DatabaseName), "MongoDb database name is required.")
            .ValidateOnStart();

        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<MongoDbOptions>>()
                .Value;

            return new MongoClient(options.ConnectionString);
        });

        services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<MongoDbOptions>>()
                .Value;

            var database = serviceProvider
                .GetRequiredService<IMongoClient>()
                .GetDatabase(options.DatabaseName);

            EnsureIndexes(database);

            return database;
        });

        services.AddScoped<ITaskListRepository, MongoTaskListRepository>();
        services.AddScoped<ITaskItemRepository, MongoTaskItemRepository>();
        services.AddScoped<ITaskListMemberRepository, MongoTaskListMemberRepository>();
        services.AddScoped<IUserRepository, MongoUserRepository>();

        return services;
    }

    private static void EnsureIndexes(IMongoDatabase database)
    {
        database
            .GetCollection<TaskListDocument>(MongoCollectionConstants.TaskLists)
            .Indexes
            .CreateMany(
            [
                new CreateIndexModel<TaskListDocument>(
                    Builders<TaskListDocument>.IndexKeys.Ascending(taskList => taskList.OwnerUserId),
                    new CreateIndexOptions { Name = "ix_task_lists_owner_user_id" }),
                new CreateIndexModel<TaskListDocument>(
                    Builders<TaskListDocument>.IndexKeys.Descending(taskList => taskList.CreatedAt),
                    new CreateIndexOptions { Name = "ix_task_lists_created_at_desc" })
            ]);

        database
            .GetCollection<TaskListMemberDocument>(MongoCollectionConstants.TaskListMembers)
            .Indexes
            .CreateMany(
            [
                new CreateIndexModel<TaskListMemberDocument>(
                    Builders<TaskListMemberDocument>.IndexKeys
                        .Ascending(member => member.TaskListId)
                        .Ascending(member => member.MemberUserId),
                    new CreateIndexOptions
                    {
                        Name = "ux_task_list_members_task_list_id_member_user_id",
                        Unique = true
                    }),
                new CreateIndexModel<TaskListMemberDocument>(
                    Builders<TaskListMemberDocument>.IndexKeys.Ascending(member => member.MemberUserId),
                    new CreateIndexOptions { Name = "ix_task_list_members_member_user_id" })
            ]);

        database
            .GetCollection<TaskItemDocument>(MongoCollectionConstants.TaskItems)
            .Indexes
            .CreateOne(new CreateIndexModel<TaskItemDocument>(
                Builders<TaskItemDocument>.IndexKeys
                    .Ascending(taskItem => taskItem.TaskListId)
                    .Descending(taskItem => taskItem.CreatedAt),
                new CreateIndexOptions { Name = "ix_task_items_task_list_id_created_at_desc" }));
    }
}
