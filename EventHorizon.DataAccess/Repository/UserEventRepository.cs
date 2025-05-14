using EventHorizon.DataAccess.Data;
using EventHorizon.DataAccess.Repository.IRepository;
using EventHorizon.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventHorizon.DataAccess.Repository
{
    public class UserEventRepository : IUserEventRepository
    {
        private readonly ApplicationDbContext _db;
        public UserEventRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UserEvent> BookEventAsync(string userId, Guid eventId)
        {
            var existing = await _db.UserEvents.FindAsync(userId, eventId);
            if (existing != null) return existing;
            var userEvent = new UserEvent { UserId = userId, EventId = eventId, BookingDate = DateTime.UtcNow };
            _db.UserEvents.Add(userEvent);
            await _db.SaveChangesAsync();
            return userEvent;
        }

        public async Task<bool> IsEventBookedAsync(string userId, Guid eventId)
        {
            return await _db.UserEvents.AnyAsync(ue => ue.UserId == userId && ue.EventId == eventId);
        }

        public async Task<IEnumerable<UserEvent>> GetUserBookingsAsync(string userId)
        {
            return await _db.UserEvents.Include(ue => ue.Event).Where(ue => ue.UserId == userId).ToListAsync();
        }

        public async Task<bool> CancelBookingAsync(string userId, Guid eventId)
        {
            var userEvent = await _db.UserEvents.FindAsync(userId, eventId);
            if (userEvent == null) return false;
            _db.UserEvents.Remove(userEvent);
            await _db.SaveChangesAsync();
            return true;
        }
    }
} 