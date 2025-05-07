using Microsoft.AspNetCore.Identity;

namespace EventHorizon.Models.Models;

public class ApplicationUser : IdentityUser
{
    public List<Event> Events { get; set; } = [];
}
