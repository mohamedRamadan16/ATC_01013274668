using AutoMapper;
using EventHorizon.Models.DTOs.Category;
using EventHorizon.Models.DTOs.Event;
using EventHorizon.Models.Models;

namespace EventHorizon
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            // Category
            CreateMap<Category, CategoryCreateDTO>();
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<Category, CategoryUpdateDTO>().ReverseMap();

            // Event
            CreateMap<Event, EventDTO>().ReverseMap();
            CreateMap<Event, EventCreateDTO>().ReverseMap();
            CreateMap<Event, EventUpdateDTO>().ReverseMap();
        }
    }
}
