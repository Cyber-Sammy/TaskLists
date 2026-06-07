using MongoDB.Driver;
using MongoDB.Bson;
using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Application.Models;
using TaskLists.Application.Models.Queries.TaskLists;
using TaskLists.Domain.Entities;
using TaskLists.Infrastructure.Mongo.Constants;
using TaskLists.Infrastructure.Mongo.Documents;
using TaskLists.Infrastructure.Mongo.Mapping;
using TaskLists.SharedKernel.Result;
using QuerySortDirection = TaskLists.Application.Models.Queries.QuerySortDirection;

namespace TaskLists.Infrastructure.Mongo.Repositories;

internal sealed class MongoTaskListRepository : ITaskListRepository
{
    private readonly IMongoCollection<TaskListDocument> _taskLists;
    private readonly IMongoCollection<TaskListMemberDocument> _taskListMembers;

    public MongoTaskListRepository(IMongoDatabase database)
    {
        _taskLists = database.GetCollection<TaskListDocument>(MongoCollectionConstants.TaskLists);
        _taskListMembers = database.GetCollection<TaskListMemberDocument>(MongoCollectionConstants.TaskListMembers);
    }

    public async Task<Result<Guid>> AddAsync(TaskList taskList, CancellationToken cancellationToken)
    {
        try
        {
            var document = taskList.ToDocument();

            await _taskLists.InsertOneAsync(document, cancellationToken: cancellationToken);

            return Result<Guid>.Success(document.Id);
        }
        catch (MongoException ex)
        {
            return Result<Guid>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<Guid>.Error(ex.Message);
        }
    }

    public async Task<Result<Guid>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var deleteMongoResult = await _taskLists.DeleteOneAsync(
                taskList => taskList.Id == id,
                cancellationToken);

            if (!deleteMongoResult.IsAcknowledged)
            {
                return Result<Guid>.Failure(
                    Result.ResultStatus.Error,
                    MongoRepositoryConstants.OperationWasNotAcknowledged);
            }

            if (deleteMongoResult.DeletedCount == 0)
            {
                return Result<Guid>.NotFound(MongoTaskListConstants.TaskListWasNotFound);
            }

            return Result<Guid>.Success(id);
        }
        catch (MongoException ex)
        {
            return Result<Guid>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<Guid>.Error(ex.Message);
        }
    }

    public async Task<Result<TaskList?>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _taskLists
                .Find(taskList => taskList.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            return Result<TaskList?>.Success(document?.ToDomain());
        }
        catch (MongoException ex)
        {
            return Result<TaskList?>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<TaskList?>.Error(ex.Message);
        }
    }

    public async Task<Result<PagedResponse<TaskList>>> GetVisibleToUserAsync(
        Guid userId,
        int page,
        int pageSize,
        TaskListSortField sortBy,
        QuerySortDirection sortDirection,
        CancellationToken cancellationToken)
    {
        try
        {
            var memberTaskListIds = await _taskListMembers
                .Find(member => member.MemberUserId == userId)
                .Project(member => member.TaskListId)
                .ToListAsync(cancellationToken);

            var filterBuilder = Builders<TaskListDocument>.Filter;

            var visibleToUserFilter = filterBuilder.Or(
                filterBuilder.Eq(taskList => taskList.OwnerUserId, userId),
                filterBuilder.In(taskList => taskList.Id, memberTaskListIds));

            var totalCount = await _taskLists.CountDocumentsAsync(
                visibleToUserFilter,
                cancellationToken: cancellationToken);

            var sort = BuildSort(sortBy, sortDirection);

            var documents = await _taskLists
                .Find(visibleToUserFilter)
                .Sort(sort)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            var pagedResponse = new PagedResponse<TaskList>(
                documents.Select(document => document.ToDomain()).ToList().AsReadOnly(),
                page,
                pageSize,
                totalCount);

            return Result<PagedResponse<TaskList>>.Success(pagedResponse);
        }
        catch (MongoException ex)
        {
            return Result<PagedResponse<TaskList>>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<PagedResponse<TaskList>>.Error(ex.Message);
        }
    }

    public async Task<Result<TaskList>> GetVisibleToUserByIdAsync(
        Guid taskListId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await _taskLists
                .Find(taskList => taskList.Id == taskListId)
                .FirstOrDefaultAsync(cancellationToken);

            if (document is null)
            {
                return Result<TaskList>.NotFound(MongoTaskListConstants.TaskListWasNotFound);
            }

            if (document.OwnerUserId == userId)
            {
                return Result<TaskList>.Success(document.ToDomain());
            }

            var isMember = await _taskListMembers
                .Find(member => member.TaskListId == taskListId && member.MemberUserId == userId)
                .AnyAsync(cancellationToken);

            return isMember
                ? Result<TaskList>.Success(document.ToDomain())
                : Result<TaskList>.NotFound(MongoTaskListConstants.TaskListWasNotFound);
        }
        catch (MongoException ex)
        {
            return Result<TaskList>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<TaskList>.Error(ex.Message);
        }
    }

    public async Task<Result<bool>> IsTaskListExistsAsync(Guid taskListId, CancellationToken cancellation)
    {
        try
        {
            var exists = await _taskLists
                .Find(taskList => taskList.Id == taskListId)
                .AnyAsync(cancellation);

            return Result<bool>.Success(exists);
        }
        catch (MongoException ex)
        {
            return Result<bool>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<bool>.Error(ex.Message);
        }
    }

    public async Task<Result<bool>> IsTaskListExistsForUserAsync(
        Guid taskListId,
        Guid userId,
        CancellationToken cancellation)
    {
        try
        {
            var isOwner = await _taskLists
                .Find(taskList => taskList.Id == taskListId && taskList.OwnerUserId == userId)
                .AnyAsync(cancellation);

            if (isOwner)
            {
                return Result<bool>.Success(true);
            }

            var isMember = await _taskListMembers
                .Find(member => member.TaskListId == taskListId && member.MemberUserId == userId)
                .AnyAsync(cancellation);

            return Result<bool>.Success(isMember);
        }
        catch (MongoException ex)
        {
            return Result<bool>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<bool>.Error(ex.Message);
        }
    }

    public async Task<Result<Guid>> UpdateAsync(TaskList taskList, CancellationToken cancellationToken)
    {
        try
        {
            var document = taskList.ToDocument();

            var updateMongoResult = await _taskLists.ReplaceOneAsync(
                storedTaskList => storedTaskList.Id == taskList.Id,
                document,
                cancellationToken: cancellationToken);

            if (!updateMongoResult.IsAcknowledged)
            {
                return Result<Guid>.Failure(
                    Result.ResultStatus.Error,
                    MongoRepositoryConstants.OperationWasNotAcknowledged);
            }

            if (updateMongoResult.MatchedCount == 0)
            {
                return Result<Guid>.NotFound(MongoTaskListConstants.TaskListWasNotFound);
            }

            return Result<Guid>.Success(taskList.Id);
        }
        catch (MongoException ex)
        {
            return Result<Guid>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<Guid>.Error(ex.Message);
        }
    }

    private static SortDefinition<TaskListDocument> BuildSort(
        TaskListSortField sortBy,
        QuerySortDirection sortDirection)
    {
        var sortBuilder = Builders<TaskListDocument>.Sort;

        return (sortBy, sortDirection) switch
        {
            (TaskListSortField.Title, QuerySortDirection.Ascending) =>
                sortBuilder.Ascending(taskList => taskList.Title),
            (TaskListSortField.Title, QuerySortDirection.Descending) =>
                sortBuilder.Descending(taskList => taskList.Title),
            (TaskListSortField.CreatedAt, QuerySortDirection.Ascending) =>
                sortBuilder.Ascending(taskList => taskList.CreatedAt),
            (TaskListSortField.CreatedAt, QuerySortDirection.Descending) =>
                sortBuilder.Descending(taskList => taskList.CreatedAt),
            (TaskListSortField.UpdatedAt, QuerySortDirection.Ascending) =>
                sortBuilder.Ascending(taskList => taskList.UpdatedAt),
            _ =>
                sortBuilder.Descending(taskList => taskList.CreatedAt)
        };
    }
}
