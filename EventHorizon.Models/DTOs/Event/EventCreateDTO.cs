
using Microsoft.AspNetCore.Http;

namespace EventHorizon.Models.DTOs.Event;

public class EventCreateDTO
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string venue { get; set; } = null!;
    public decimal price { get; set; }
    public DateTime EventDate { get; set; }
    public int? CategoryId { get; set; }

    // For Image Upload
    public IFormFile? Image { get; set; }
}
