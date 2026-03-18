using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(string id, string partitionKey);
    Task<IEnumerable<T>> GetAllAsync(string? partitionKey = null);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(string id, string partitionKey);
    Task<IEnumerable<T>> QueryAsync(Func<IQueryable<T>, IQueryable<T>> queryFunc, string? partitionKey = null);
}
