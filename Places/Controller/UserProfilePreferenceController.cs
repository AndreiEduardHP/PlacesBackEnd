using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Places.Dto;
using Places.Interfaces;
using Places.Models;


namespace Places.Controller
{


    [Route("api/[controller]")]
    [ApiController]
    public class UserProfilePreferenceController : ControllerBase
    {

        private readonly IUserProfileRepository _userProfileRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;
        public UserProfilePreferenceController(IUserProfileRepository userProfileRepository, ILocationRepository locationRepository, IMapper mapper)
        {
            _userProfileRepository = userProfileRepository;
            _locationRepository = locationRepository;
            _mapper = mapper;
        }

        [HttpPut("{userId}/preferences")]
        public async Task<IActionResult> UpdateUserPreferences(int userId, [FromBody] UserProfileDto preferences)
        {
            if (!_userProfileRepository.UserProfileExists(userId))
            {
                return NotFound($"UserProfile with ID {userId} not found.");
            }

            // Await the asynchronous operation and then apply the negation
            var result = await _userProfileRepository.UpdateUserPreferences(userId, preferences);

            if (!result)
            {
                return BadRequest("Failed to update user preferences.");
            }

            return NoContent(); // 204 No Content is typically used for successful update operations
        }


    }
}
