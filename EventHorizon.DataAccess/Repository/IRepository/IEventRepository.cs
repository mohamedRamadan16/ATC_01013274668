using EventHorizon.Models.Models;

namespace EventHorizon.DataAccess.Repository.IRepository;

public interface IEventRepository : IRepository<Event>
{
    Task<Event> UpdateAsync(Event _event);
}
