using MongoDB.Driver;
using MongoDB.Bson;
using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Domain.Entities;
using TaskLists.Infrastructure.Mongo.Constants;
using TaskLists.Infrastructure.Mongo.Documents;
using TaskLists.Infrastructure.Mongo.Mapping;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Infrastructure.Mongo.Repositories;

internal sealed class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<UserDocument> _users;

    public MongoUserRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<UserDocument>(MongoCollectionConstants.Users);
    }

    public async Task<Result<IReadOnlyCollection<User>>> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            var documents = await _users
                .Find(_ => true)
                .SortBy(user => user.CreatedAt)
                .ToListAsync(cancellationToken);

            var users = documents
                .Select(document => document.ToDomain())
                .ToArray();

            return Result<IReadOnlyCollection<User>>.Success(users);
        }
        catch (MongoException ex)
        {
            return Result<IReadOnlyCollection<User>>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<IReadOnlyCollection<User>>.Error(ex.Message);
        }
    }

    public async Task<Result<Guid>> AddAsync(User user, CancellationToken cancellationToken)
    {
        try
        {
            var document = user.ToDocument();

            await _users.InsertOneAsync(document, cancellationToken: cancellationToken);

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

    public async Task<Result<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _users
                .Find(user => user.Id == id)
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

    public async Task<Result<User?>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _users
                .Find(user => user.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            return Result<User?>.Success(document?.ToDomain());
        }
        catch (MongoException ex)
        {
            return Result<User?>.Error(ex.Message);
        }
        catch (BsonException ex)
        {
            return Result<User?>.Error(ex.Message);
        }
    }
}
