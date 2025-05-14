namespace EventHorizon.Models.DTOs.Event;

public class EventDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Venue { get; set; } = null!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime EventDate { get; set; }
}

