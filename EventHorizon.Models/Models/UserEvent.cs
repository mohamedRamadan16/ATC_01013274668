using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EventHorizon.Models.Models
{
    public class UserEvent
    {
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public Guid EventId { get; set; }
        [ForeignKey("EventId")]
        public Event Event { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    }
} 