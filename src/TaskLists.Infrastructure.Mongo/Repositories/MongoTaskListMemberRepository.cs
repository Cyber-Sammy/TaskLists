using MongoDB.Driver;
using MongoDB.Bson;
using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Domain.Entities;
using TaskLists.Infrastructure.Mongo.Constants;
using TaskLists.Infrastructure.Mongo.Documents;
using TaskLists.Infrastructure.Mongo.Mapping;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Infrastructure.Mongo.Repositories;

internal sealed class MongoTaskListMemberRepository : ITaskListMemberRepository
{
    private readonly IMongoCollection<TaskListMemberDocument> _taskListMembers;

    public MongoTaskListMemberRepository(IMongoDatabase database)
    {
        _taskListMembers = database.GetCollection<TaskListMemberDocument>(MongoCollectionConstants.TaskListMembers);
    }

    public async Task<Result<Guid>> AddAsync(TaskListMember member, CancellationToken cancellationToken)
    {
        try
        {
            var document = member.ToDocument();

            await _taskListMembers.InsertOneAsync(document, cancellationToken: cancellationToken);

            return Result<Guid>.Success(document.Id);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return Result<Guid>.Conflict(MongoTaskListMemberConstants.MemberAlreadyExists);
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

    public async Task<Result<Guid>> DeleteAsync(
        Guid taskListId,
        Guid memberUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleteResult = await _taskListMembers.DeleteOneAsync(
                member => member.TaskListId == taskListId && member.MemberUserId == memberUserId,
                cancellationToken);

            if (!deleteResult.IsAcknowledged)
            {
                return Result<Guid>.Failure(
                    Result.ResultStatus.Error,
                    MongoRepositoryConstants.OperationWasNotAcknowledged);
            }

            if (deleteResult.DeletedCount == 0)
            {
                return Result<Guid>.NotFound(MongoTaskListMemberConstants.MemberWasNotFound);
            }

            return Result<Guid>.Success(memberUserId);
        }
        catch (MongoException ex)
        {
            return Result<Guid>.Error(ex.Message);
        }
    }

    public async Task<Result<bool>> ExistsAsync(
        Guid taskListId,
        Guid memberUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _taskListMembers
                .Find(member => member.TaskListId == taskListId && member.MemberUserId == memberUserId)
                .AnyAsync(cancellationToken);

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

    public async Task<Result<IReadOnlyCollection<TaskListMember>>> GetByTaskListIdAsync(
        Guid taskListId,
        CancellationToken cancellationToken)
    {
        try
        {
            var documents = await _taskListMembers
                .Find(member => member.TaskListId == taskListId)
                .ToListAsync(cancellationToken);

            var members = documents
                .Select(document => document.ToDomain())
                .ToList()
                .AsReadOnly();

            return Result<IReadOnlyCollection<TaskListMember>>.Success(members);
        }
        catch (MongoException ex)
        {
            return Result<IReadOnlyCollection<TaskListMember>>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<IReadOnlyCollection<TaskListMember>>.Error(ex.Message);
        }
    }

    public async Task<Result<long>> DeleteByTaskListIdAsync(
        Guid taskListId,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleteResult = await _taskListMembers.DeleteManyAsync(
                member => member.TaskListId == taskListId,
                cancellationToken);

            if (!deleteResult.IsAcknowledged)
            {
                return Result<long>.Failure(
                    Result.ResultStatus.Error,
                    MongoRepositoryConstants.OperationWasNotAcknowledged);
            }

            return Result<long>.Success(deleteResult.DeletedCount);
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

    public async Task<Result<TaskListMember?>> GetByTaskListAndUserAsync(
        Guid taskListId,
        Guid memberUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            var document = await _taskListMembers
                .Find(member => member.TaskListId == taskListId && member.MemberUserId == memberUserId)
                .FirstOrDefaultAsync(cancellationToken);

            return Result<TaskListMember?>.Success(document?.ToDomain());
        }
        catch (MongoException ex)
        {
            return Result<TaskListMember?>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<TaskListMember?>.Error(ex.Message);
        }
    }
}
