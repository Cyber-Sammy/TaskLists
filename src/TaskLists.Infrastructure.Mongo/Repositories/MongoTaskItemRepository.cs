using MongoDB.Driver;
using MongoDB.Bson;
using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Application.Models;
using TaskLists.Domain.Entities;
using TaskLists.Infrastructure.Mongo.Constants;
using TaskLists.Infrastructure.Mongo.Documents;
using TaskLists.Infrastructure.Mongo.Mapping;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Infrastructure.Mongo.Repositories;

internal sealed class MongoTaskItemRepository : ITaskItemRepository
{
    private readonly IMongoCollection<TaskItemDocument> _taskItems;
    private readonly IMongoCollection<TaskListDocument> _taskLists;
    private readonly IMongoCollection<TaskListMemberDocument> _taskListMembers;

    public MongoTaskItemRepository(IMongoDatabase database)
    {
        _taskItems = database.GetCollection<TaskItemDocument>(MongoCollectionConstants.TaskItems);
        _taskLists = database.GetCollection<TaskListDocument>(MongoCollectionConstants.TaskLists);
        _taskListMembers = database.GetCollection<TaskListMemberDocument>(MongoCollectionConstants.TaskListMembers);
    }

    public async Task<Result<Guid>> AddAsync(TaskItem taskItem, CancellationToken cancellationToken)
    {
        try
        {
            var document = taskItem.ToDocument();

            await _taskItems.InsertOneAsync(document, cancellationToken: cancellationToken);

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
            var deleteMongoResult = await _taskItems.DeleteOneAsync(storedTaskItem => storedTaskItem.Id == id, cancellationToken);

            if (!deleteMongoResult.IsAcknowledged)
            {
                return Result<Guid>.Failure(
                    Result.ResultStatus.Error,
                    MongoRepositoryConstants.OperationWasNotAcknowledged);
            }

            if (deleteMongoResult.DeletedCount == 0)
            {
                return Result<Guid>.NotFound(MongoTaskItemConstants.TaskItemWasNotFound);
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

    public async Task<Result<long>> DeleteByTaskListIdAsync(Guid taskListId, CancellationToken cancellationToken)
    {
        try
        {
            var deleteMongoResult = await _taskItems.DeleteManyAsync(storedTaskItem => storedTaskItem.TaskListId == taskListId, cancellationToken);

            if (!deleteMongoResult.IsAcknowledged)
            {
                return Result<long>.Failure(
                    Result.ResultStatus.Error,
                    MongoRepositoryConstants.OperationWasNotAcknowledged);
            }

            return Result<long>.Success(deleteMongoResult.DeletedCount);
        }
        catch (MongoException ex)
        {
            return Result<long>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<long>.Error(ex.Message);
        }
    }

    public async Task<Result<PagedResponse<TaskItem>>> GetByTaskListIdAsync(Guid taskListId, int page, int pageSize, CancellationToken cancellationToken)
    {
        try
        {
            var totalCount = await _taskItems.CountDocumentsAsync(storedTaskItem => storedTaskItem.TaskListId == taskListId, cancellationToken: cancellationToken);
            var pagedTaskItems = await _taskItems
                .Find(storedTaskItem => storedTaskItem.TaskListId == taskListId)
                .SortByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);
            var pagedResponse = new PagedResponse<TaskItem>(
                pagedTaskItems.Select(pti => pti.ToDomain()).ToList().AsReadOnly(),
                page,
                pageSize,
                totalCount);

            return Result<PagedResponse<TaskItem>>.Success(pagedResponse);
        }
        catch (MongoException ex)
        {
            return Result<PagedResponse<TaskItem>>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<PagedResponse<TaskItem>>.Error(ex.Message);
        }
    }

    public async Task<Result<TaskItem?>> GetByIdVisibleToUserAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var taskItemDocument = await _taskItems
                .Find(taskItem => taskItem.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (taskItemDocument is null)
            {
                return Result<TaskItem?>.Success(null);
            }

            var canAccessTaskList = await CanAccessTaskListAsync(
                taskItemDocument.TaskListId,
                userId,
                cancellationToken);

            return canAccessTaskList
                ? Result<TaskItem?>.Success(taskItemDocument.ToDomain())
                : Result<TaskItem?>.Success(null);
        }
        catch (MongoException ex)
        {
            return Result<TaskItem?>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<TaskItem?>.Error(ex.Message);
        }
    }

    public async Task<Result<Guid>> UpdateAsync(TaskItem taskItem, CancellationToken cancellationToken)
    {
        try
        {
            var document = taskItem.ToDocument();

            var updateResult = await _taskItems.ReplaceOneAsync(
                storedTaskItem => storedTaskItem.Id == taskItem.Id,
                document,
                cancellationToken: cancellationToken);

            if (updateResult.MatchedCount == 0)
            {
                return Result<Guid>.NotFound(MongoTaskItemConstants.TaskItemWasNotFound);
            }

            return Result<Guid>.Success(taskItem.Id);
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

    private async Task<bool> CanAccessTaskListAsync(
        Guid taskListId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var isOwner = await _taskLists
            .Find(taskList => taskList.Id == taskListId && taskList.OwnerUserId == userId)
            .AnyAsync(cancellationToken);

        if (isOwner)
        {
            return true;
        }

        return await _taskListMembers
            .Find(member => member.TaskListId == taskListId && member.MemberUserId == userId)
            .AnyAsync(cancellationToken);
    }
}
