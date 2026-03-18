using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MovieTicketBooking.Api.Repo;

public class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> _collection;
    protected readonly ILogger _logger;

    protected BaseRepository(IMongoCollection<T> collection, ILogger logger)
    {
        _collection = collection;
        _logger = logger;
    }

    public virtual async Task<T?> GetByIdAsync(string id, string partitionKey)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(string? partitionKey = null)
    {
        // MongoDB doesn't need a partition key; we keep the parameter for compatibility.
        return await _collection.AsQueryable()
            .Where(x => x.IsActive)
            .ToListAsync();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        entity.CreatedAt = entity.UpdatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
        await _collection.ReplaceOneAsync(filter, entity);
        return entity;
    }

    public virtual async Task DeleteAsync(string id, string partitionKey)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        await _collection.DeleteOneAsync(filter);
    }

    public virtual async Task<IEnumerable<T>> QueryAsync(
        Func<IQueryable<T>, IQueryable<T>> queryFunc,
        string? partitionKey = null)
    {
        // MongoDB doesn't need a partition key; we keep the parameter for compatibility.
        var baseQuery = _collection.AsQueryable();
        var query = queryFunc(baseQuery);
        return await query.ToListAsync();
    }
}
