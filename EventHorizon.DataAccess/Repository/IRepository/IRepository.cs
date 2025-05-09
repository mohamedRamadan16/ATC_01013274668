using System.Linq.Expressions;

namespace EventHorizon.DataAccess.Repository.IRepository;

public interface IRepository<T> where T : class
{
    Task<List<T>?> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, int pageSize = 0, int pageNumber = 1);
    Task<T?> GetAsync(Expression<Func<T, bool>> filter, bool tracked = true, string? includeProperties = null);
    Task CreateAsync(T villa);
    Task RemoveAsync(T villa);
    Task SaveAsync();
}
