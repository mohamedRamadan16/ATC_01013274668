using EventHorizon.Models.Models;

namespace EventHorizon.DataAccess.Repository.IRepository;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category> UpdateAsync(Category category);
}
