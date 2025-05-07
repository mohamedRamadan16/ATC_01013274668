using System.ComponentModel.DataAnnotations.Schema;

namespace EventHorizon.Models.Models;

public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string venue { get; set; } = null!;
    public double price { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime EventDate { get; set; }
    public int? CategoryId { get; set; }
    public string OwnerId { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public ApplicationUser Owner { get; set; } = null!;

}
