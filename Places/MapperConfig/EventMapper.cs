using AutoMapper;
using Places.Dto;
using Places.Models;

namespace Places.MapperConfig
{
    public class EventMapper : Profile
    {
        public EventMapper() {
            CreateMap<Event, EventDto>()
                   .ForMember(d => d.EventAlbumImages, o => o.MapFrom(x => x.EventAlbumImages));
            CreateMap<EventDto, Event>()
                  .ForMember(d => d.EventAlbumImages, o => o.MapFrom(x => x.EventAlbumImages));
        }
    }
}
