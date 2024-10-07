using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Places.Data;
using Places.Dto;
using Places.Interfaces;
using Places.Models;
using Places.Repository;

namespace Places.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventRepository _eventRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;
        private readonly PlacesContext _context;

        public EventController(IEventRepository eventRepository, ILocationRepository locationRepository, IMapper mapper, PlacesContext context)
        {
            _eventRepository = eventRepository;
            _locationRepository = locationRepository;
            _mapper = mapper;
            _context = context;
        }

        [HttpPut("updateEvent/{eventId}")]
        public async Task<IActionResult> UpdateEvent(int eventId, UpdateEventDto updateEventDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventEntity =  _eventRepository.GetEvent(eventId);
            if (eventEntity == null)
            {
                return NotFound();
            }

            eventEntity.EventName = updateEventDto.EventName ?? eventEntity.EventName;
            eventEntity.EventImage = updateEventDto.EventImage ?? eventEntity.EventImage;
            eventEntity.EventDescription = updateEventDto.EventDescription ?? eventEntity.EventDescription;
            eventEntity.OtherRelevantInformation = updateEventDto.OtherRelevantInformation ?? eventEntity.OtherRelevantInformation;
            eventEntity.MaxParticipants = updateEventDto.MaxParticipants;

            _eventRepository.UpdateEvent(eventEntity);

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCoordinates(int id, [FromBody] UpdateCoordinatesDto updateCoordinatesDto)
        {
            var eventToUpdate = await _context.Events.Include(e => e.EventLocation).FirstOrDefaultAsync(e => e.Id == id);
            if (eventToUpdate == null)
            {
                return NotFound();
            }

            eventToUpdate.EventLocation.latitude = updateCoordinatesDto.Latitude;
            eventToUpdate.EventLocation.longitude = updateCoordinatesDto.Longitude;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Coordinates updated successfully" });
            }
            catch (Exception ex)
            {
             
                return StatusCode(500, new { message = "Failed to update coordinates" });
            }
        }
    

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Event>))]
        public async Task<IActionResult> GetEventsAsync()
        {
            var events = _eventRepository.GetEvents();
            var eventDtos = _mapper.Map<List<EventDto>>(events);

            foreach (var evt in eventDtos)
            {
                var location = _locationRepository.GetLocation(evt.EventLocationId);
                evt.LocationLatitude = location.latitude;
                evt.LocationLongitude = location.longitude;
                evt.EventParticipantsCount = await _eventRepository.GetCurrentParticipantCount(evt.Id);
            }

            return Ok(eventDtos);
        }

        [HttpGet("search")]
        public IActionResult SearchEvents(string query)
        {
            var events = _context.Events
                .Where(e => e.EventName.Contains(query) || e.EventDescription.Contains(query))
                .ToList();
            var eventDtos = _mapper.Map<List<EventDto>>(events);
            foreach (var evt in eventDtos)
            {
                var location = _locationRepository.GetLocation(evt.EventLocationId);
                evt.LocationLatitude = location.latitude;
                evt.LocationLongitude = location.longitude;
                
            }

            return Ok(eventDtos);
        }

        [HttpGet("{eventId}")]
        [ProducesResponseType(200, Type = typeof(Event))]
        public IActionResult GetEvent(int eventId)
        {
            if(!_eventRepository.EventExists(eventId))
                return NotFound();
            
            var eventById = _mapper.Map<EventDto>(_eventRepository.GetEvent(eventId));

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(eventById);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateEvent([FromBody] EventDto createdEvent)
        {
          


            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Location location = new Location();
            location.longitude = createdEvent.LocationLongitude;
            location.latitude = createdEvent.LocationLatitude;

            _locationRepository.CreateLocation(location);
            int locationId = location.Id;

            var eventMap = _mapper.Map<Event>(createdEvent);


            //byte[] imageData;
          
            //imageData = Convert.FromBase64String(createdEvent.EventImage);
            
          



            eventMap.EventLocationId = locationId;
            eventMap.EventLocation = _locationRepository.GetLocation(locationId);
          //  eventMap.EventImage = Convert.FromBase64String(eventMap.EventImage);
            if (!_eventRepository.CreateEvent(eventMap))
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }

            return Ok(new { EventId = eventMap.Id, Message = "Successfully created" });
        }



        [HttpPut("deleteEvent/{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventToUpdate =  _eventRepository.GetEvent(id);
            if (eventToUpdate == null)
            {
                return NotFound();
            }

            eventToUpdate.IsDeleted = true;
          

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Event deleted successfully" });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = "Failed to delete event" });
            }
        }
        [HttpPost("{eventId}/AddImages")]
        public IActionResult AddImagesToEvent(int eventId, [FromBody] List<string> imageUrls)
        {
            var eventEntity = _context.Events.Include(e => e.EventAlbumImages).FirstOrDefault(e => e.Id == eventId);

            if (eventEntity == null)
            {
                return NotFound();
            }

            foreach (var imageUrl in imageUrls)
            {
                eventEntity.EventAlbumImages.Add(new EventAlbumImage
                {
                    ImageUrl = imageUrl,
                    EventId = eventId
                });
            }

            _context.SaveChanges();

            return Ok();
        }

    }


}
