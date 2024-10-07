using AutoMapper;
using Places.Dto;
using Places.Models;

namespace Places.MapperConfig
{
    public class EventAlbumImagesMapper : Profile
    {
        public EventAlbumImagesMapper() {
            CreateMap<EventAlbumImage, EventAlbumImageDto>()
                    .ForMember(d => d.ImageUrl, o => o.MapFrom(x => x.ImageUrl));
            CreateMap<EventAlbumImageDto, EventAlbumImage>()
                  .ForMember(d => d.ImageUrl, o => o.MapFrom(x => x.ImageUrl));

        }
    }
}
