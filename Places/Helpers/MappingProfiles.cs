using AutoMapper;
using Places.Dto;
using Places.Models;

namespace Places.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<UserProfile, UserProfileDto>();
            CreateMap<UserProfileDto, UserProfile>();
            CreateMap<Location, LocationDto>();
            CreateMap<LocationDto, Location>();
            CreateMap<Event, EventDto>();
            CreateMap<EventDto, Event>();
            CreateMap<UserProfileEvent, UserProfileEventDto>();
            CreateMap<UserProfileEventDto, UserProfileEvent>();

        }
        
    }
}
