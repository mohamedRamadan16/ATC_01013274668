using EventHorizon.DataAccess.Data;
using EventHorizon.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;

namespace EventHorizon.DataAccess.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _db;
    internal DbSet<T> dbSet;
    public Repository(ApplicationDbContext db)
    {
        _db = db;
        this.dbSet = _db.Set<T>();
    }
    public async Task CreateAsync(T obj)
    {
        await dbSet.AddAsync(obj);
        await SaveAsync();
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> filter, bool tracked = true, string? includeProperties = null)
    {
        IQueryable<T> query = dbSet;
        if (!tracked)
            query = query.AsNoTracking();
        if (filter != null)
            query = query.Where(filter);
        if (includeProperties != null)
        {
            foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(property);
        }
        return await query.FirstOrDefaultAsync();
    }

    public async Task<List<T>?> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, int pageSize = 0, int pageNumber = 1)
    {
        IQueryable<T> query = dbSet;
        if (filter != null)
            query = query.Where(filter);

        // Pagination
        if (pageSize > 0)
        {
            if (pageSize > 15)
                pageSize = 15;
            query = query.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
        }

        if (includeProperties != null)
        {
            foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(property);
        }

        return await query.ToListAsync();
    }

    public async Task RemoveAsync(T obj)
    {
        dbSet.Remove(obj);
        await SaveAsync();
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }
}
