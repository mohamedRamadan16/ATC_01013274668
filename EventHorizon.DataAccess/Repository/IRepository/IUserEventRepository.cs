using EventHorizon.Models.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventHorizon.DataAccess.Repository.IRepository
{
    public interface IUserEventRepository
    {
        Task<UserEvent> BookEventAsync(string userId, Guid eventId);
        Task<bool> IsEventBookedAsync(string userId, Guid eventId);
        Task<IEnumerable<UserEvent>> GetUserBookingsAsync(string userId);
        Task<bool> CancelBookingAsync(string userId, Guid eventId);
    }
} 