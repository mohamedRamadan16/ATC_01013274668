namespace EventHorizon.Models.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<Event> Events { get; set; } = null!;
}
