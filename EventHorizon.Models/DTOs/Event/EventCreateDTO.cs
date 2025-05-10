
namespace EventHorizon.Models.DTOs.Event;

public class EventCreateDTO
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string venue { get; set; } = null!;
    public double price { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime EventDate { get; set; }
    public int? CategoryId { get; set; }
}
