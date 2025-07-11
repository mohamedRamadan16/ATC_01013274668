﻿
using Microsoft.AspNetCore.Http;

namespace EventHorizon.Models.DTOs.Event;

public class EventUpdateDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string venue { get; set; } = null!;
    public decimal price { get; set; }
    public IFormFile? Image { get; set; }
    public DateTime EventDate { get; set; }
    public int? CategoryId { get; set; }
}
