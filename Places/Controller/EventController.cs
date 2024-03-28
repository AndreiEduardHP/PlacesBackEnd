using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

        public EventController(IEventRepository eventRepository, ILocationRepository locationRepository, IMapper mapper)
        {
            _eventRepository = eventRepository;
            _locationRepository = locationRepository;
            _mapper = mapper;
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
            eventEntity.EventDescription = updateEventDto.EventDescription ?? eventEntity.EventDescription;
            eventEntity.MaxParticipants = updateEventDto.MaxParticipants;
            _eventRepository.UpdateEvent(eventEntity);

            return NoContent();
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

            return Ok("Successfully created");
        }
    }
}
