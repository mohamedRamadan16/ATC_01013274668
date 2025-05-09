using EventHorizon.DataAccess.Data;
using EventHorizon.DataAccess.Repository.IRepository;
using EventHorizon.Models.Models;

namespace EventHorizon.DataAccess.Repository;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private readonly ApplicationDbContext _db;
    public CategoryRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
        return category;
    }
}
