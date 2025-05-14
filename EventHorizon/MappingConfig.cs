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
            CreateMap<Category, Models.DTOs.Category.CategoryDTO>().ReverseMap();
            CreateMap<Category, CategoryUpdateDTO>().ReverseMap();

            // Event
            CreateMap<Event, EventDTO>()      
                .ReverseMap();
            CreateMap<EventCreateDTO, Event>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) // We set it manually
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<Event, EventUpdateDTO>().ReverseMap();
                
        }
    }
}
