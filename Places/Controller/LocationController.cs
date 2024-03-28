using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Places.Dto;
using Places.Interfaces;
using Places.Models;
using Places.Repository;

namespace Places.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;
        public LocationController(ILocationRepository locationRepository, IMapper mapper)
        {
            _locationRepository = locationRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Location>))]
        public IActionResult GetLocations()
        {
            var locations = _mapper.Map<List<LocationDto>>(_locationRepository.GetLocations());

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(locations);
        }

        [HttpGet("{locationId}")]
        [ProducesResponseType(200, Type = typeof(Location))]
        [ProducesResponseType(400)]
        public IActionResult GetLocation(int locationId)
        {
            if (!_locationRepository.LocationExists(locationId))
                return NotFound();

            var location = _mapper.Map<LocationDto>(_locationRepository.GetLocation(locationId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(location);
        }

        [HttpGet("GetLocationByCoords")]
        [ProducesResponseType(200, Type = typeof(Location))]
        [ProducesResponseType(400)]
        public IActionResult GetLocationByCoords([FromQuery] double latitude, double longitude)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (!_locationRepository.LocationExistsByCoord(latitude, longitude))
            {
                return NoContent();
                
            }

            var location = _mapper.Map<LocationDto>(_locationRepository.GetLocationByCoords(latitude, longitude));

            return Ok(location);
        }

        [HttpGet("GetLocationOfAUser/userProfiles/{userProfileId}")]
        [ProducesResponseType(200, Type = typeof(Location))]
        [ProducesResponseType(400)]
        public IActionResult GetLocationOfAUser(int userProfileId)
        {
            var location = _mapper.Map<LocationDto>(_locationRepository.GetLocationByUserProfile(userProfileId));

            if(!ModelState.IsValid)
            {
                return BadRequest();
            }
            return Ok(location);
        }

        [HttpGet("GetProfilesByLocation/{locationId}/userProfiles")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Location>))]
        [ProducesResponseType(400)]
        public IActionResult GetProfilesByLocation(int locationId)
        {
            var userProfiles = _mapper.Map<List<UserProfile>>(_locationRepository.GetProfilesByLocation(locationId));

            if (!ModelState.IsValid)
                return NotFound();

            return Ok(userProfiles);
        }

        [HttpGet("GetOtherUserProfiles/{userProfileId}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Location>))]
        [ProducesResponseType(400)]
        public IActionResult GetOtherUserProfiles(int userProfileId)
        {
            var otherUserProfiles = _mapper.Map<List<UserProfile>>(_locationRepository.GetOtherUserProfiles(userProfileId));

            if (!ModelState.IsValid)
                return NotFound();

            return Ok(otherUserProfiles);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateLocation([FromBody] LocationDto locationCreate)
        {
            if(locationCreate == null)
            {
                return BadRequest(ModelState);
            }
            var location = _locationRepository.GetLocations()
                .Where(l => l.Id == locationCreate.Id).FirstOrDefault();

            if(location != null)
            {
                ModelState.AddModelError("", "Location already saved.");
                return StatusCode(422, ModelState);
            }
           
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var locationMap = _mapper.Map<Location>(locationCreate);
            if (!_locationRepository.CreateLocation(locationMap))
            {
                ModelState.AddModelError("", "Something went wrong");
                return StatusCode(500, ModelState);
            }

            //if (!_locationRepository.LocationExistsByCoord(locationCreate.latitude, locationCreate.longitude))
            //{
               
            //    return Ok(locationMap);
            //}
            return Ok("Successfully created");
        }

        [HttpPut("{locationId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateLocation(int locationId, [FromBody] LocationDto updatedLocation)
        {
            if (updatedLocation == null)
                return BadRequest(ModelState);

            if (locationId != updatedLocation.Id)
                return BadRequest(ModelState);

            if (!_locationRepository.LocationExists(locationId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest();

            var locationMap = _mapper.Map<Location>(updatedLocation);

            if (!_locationRepository.UpdateLocation(locationMap))
            {
                ModelState.AddModelError("", "Something went wrong updating category");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{locationId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteLocation(int locationId)
        {
            if (!_locationRepository.LocationExists(locationId))
            {
                return NotFound();
            }

            var locationToDelete = _locationRepository.GetLocation(locationId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_locationRepository.DeleteLocation(locationToDelete))
            {
                ModelState.AddModelError("", "Something went wrong deleting category");
            }

            return NoContent();
        }

    }


}
