using EventHorizon.DataAccess.Data;
using EventHorizon.DataAccess.Repository.IRepository;
using EventHorizon.Models.Models;

namespace EventHorizon.DataAccess.Repository;

public class EventRepository : Repository<Event>, IEventRepository
{
    private readonly ApplicationDbContext _db;
    public EventRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }
    public async Task<Event> UpdateAsync(Event _event)
    {
        _event.UpdatedAt = DateTime.Now;
        _db.Events.Update(_event);
        await _db.SaveChangesAsync();
        return _event;
    }
}
