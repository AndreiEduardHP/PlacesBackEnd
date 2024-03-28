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
    public class UserProfileController : ControllerBase
    {

        private readonly IUserProfileRepository _userProfileRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;
        public UserProfileController(IUserProfileRepository userProfileRepository , ILocationRepository locationRepository, IMapper mapper)
        {
            _userProfileRepository = userProfileRepository;
            _locationRepository = locationRepository;
            _mapper = mapper;
        }

        [HttpPost("UpdateCredit/{userProfileId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCredit(int userProfileId, [FromBody] CreditUpdateDto creditUpdate)
        {
            if (!_userProfileRepository.UserProfileExists(userProfileId))
                return NotFound("User profile not found.");

            if (creditUpdate == null || creditUpdate.Amount <= 0)
            {
                return BadRequest("Invalid credit amount.");
            }

            var userProfile =  _userProfileRepository.GetUserProfile(userProfileId);

            // Update the credits
            userProfile.Credit += creditUpdate.Amount;

            // Add any necessary business logic here. For example, you might want to ensure that credits don't go negative.

            bool isUpdated = _userProfileRepository.UpdateUserProfile(userProfile);
            if (!isUpdated)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong while updating the credits.");
            }

            // Optionally return the updated user profile or just an OK status
            var updatedUserProfileDto = _mapper.Map<UserProfileDto>(userProfile);
            return Ok(updatedUserProfileDto);
        }

        [HttpPost("DeductCredit/{userProfileId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeductCredit(int userProfileId)
        {
            if (!_userProfileRepository.UserProfileExists(userProfileId))
                return NotFound();

            var userProfile = _userProfileRepository.GetUserProfile(userProfileId);

            // Check if the user has enough credits to deduct
            if (userProfile.Credit < 1)
            {
                ModelState.AddModelError("", "Insufficient credits.");
                return BadRequest(ModelState);
            }

            // Deduct 1 credit from the user's profile
            userProfile.Credit -= 1;

            if (!_userProfileRepository.UpdateUserProfile(userProfile))
            {
                ModelState.AddModelError("", "Something went wrong deducting credit.");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }


        [HttpGet("{currentUserId}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<UserProfile>))]
        public async Task<IActionResult> GetUserProfiles(int currentUserId)
        {
            try
            {
                var userProfiles = _userProfileRepository.GetUserProfiles().Where(up => up.Id != currentUserId);

                
                var userProfileDtos = new List<UserProfileDto>();
                foreach (var userProfile in userProfiles)
                {
                  
                    bool areFriends = _userProfileRepository.AreFriends(currentUserId, userProfile.Id).Result;
                    string friendRequestStatus = await _userProfileRepository.GetFriendRequestStatus(currentUserId, userProfile.Id);


                  
                    var userProfileDto = new UserProfileDto
                    {
                        Id = userProfile.Id,
                        FirstName = userProfile.FirstName,
                        LastName = userProfile.LastName,
                        Username = userProfile.Username,
                        PhoneNumber = userProfile.PhoneNumber,
                        Email = userProfile.Email,
                        NotificationToken=userProfile.NotificationToken,
                        City = userProfile.City,
                        Interest = userProfile.Interest,
                        ProfilePicture = userProfile.ProfilePicture,
                        CurrentLocationId = userProfile.CurrentLocationId,
                        AreFriends = areFriends,
                        FriendRequestStatus = friendRequestStatus
                    };

                    userProfileDtos.Add(userProfileDto);
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                return Ok(userProfileDtos);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an appropriate response
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("details/{userProfileId}")]
        [ProducesResponseType(200, Type = typeof(UserProfile))]
        [ProducesResponseType(400)]
        public IActionResult GetUserProfile(int userProfileId)
        {
            if (!_userProfileRepository.UserProfileExists(userProfileId))
                return NotFound();

            var userProfile = _mapper.Map<UserProfileDto>(_userProfileRepository.GetUserProfile(userProfileId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(userProfile);
        }


        [HttpGet("CheckIfPhoneNumberExists")]
      
        public IActionResult CheckIfPhoneNumberExists(string phoneNumber)
        {
            if (!_userProfileRepository.CheckPhoneNumberExists(phoneNumber))
                return NoContent();

       

            return Ok();
        }


        [HttpGet("GetUserProfileByPhone/{phoneNumber}")]
        [ProducesResponseType(200, Type = typeof(UserProfile))]
        [ProducesResponseType(400)]
        public IActionResult GetUserProfileByPhone(string phoneNumber)
        {
           
            var userProfile = _mapper.Map<UserProfileDto>(_userProfileRepository.GetUserProfileByPhone(phoneNumber));

            if(userProfile == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(userProfile);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateUserProfile([FromQuery] int locationId, [FromBody] UserProfileDto userProfileCreate)
        {
            if (userProfileCreate == null)
                return BadRequest(ModelState);

            var userProfileExist = _userProfileRepository.GetUserProfiles().Where(up => up.PhoneNumber == userProfileCreate.PhoneNumber || up.Email == userProfileCreate.Email).FirstOrDefault();

            if (userProfileExist != null)
            {
                ModelState.AddModelError("", "User Profile already exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userProfileMap = _mapper.Map<UserProfile>(userProfileCreate);
            userProfileMap.CurrentLocationId = locationId;
            userProfileMap.UserLocation = _locationRepository.GetLocation(locationId);
            userProfileMap.DateAccountCreation = DateTime.Now;
            userProfileMap.Credit = 0;

            if (!_userProfileRepository.CreateUserProfile( userProfileMap))
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }

            return Ok("Successfully created");
        }

        [HttpPut("{userProfileId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateUserProfile(int userProfileId, [FromBody] UserProfileDto updatedUserProfile)
        {
            if (updatedUserProfile == null)
                return BadRequest(ModelState);

            if (userProfileId != updatedUserProfile.Id)
            {
                return BadRequest(ModelState);
            }

            if (!_userProfileRepository.UserProfileExists(userProfileId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userProfileToBeUpdated = _userProfileRepository.GetUserProfile(userProfileId);

            // Update FirstName if provided
            if (!string.IsNullOrWhiteSpace(updatedUserProfile.FirstName))
            {
                userProfileToBeUpdated.FirstName = updatedUserProfile.FirstName;
            }

            // Update LastName if provided
            if (!string.IsNullOrWhiteSpace(updatedUserProfile.LastName))
            {
                userProfileToBeUpdated.LastName = updatedUserProfile.LastName;
            }

            // Update Username if provided
            if (!string.IsNullOrWhiteSpace(updatedUserProfile.Username))
            {
                userProfileToBeUpdated.Username = updatedUserProfile.Username;
            }

            // Update PhoneNumber if provided
            if (!string.IsNullOrWhiteSpace(updatedUserProfile.PhoneNumber))
            {
                userProfileToBeUpdated.PhoneNumber = updatedUserProfile.PhoneNumber;
            }

            // Update Email if provided
            if (!string.IsNullOrWhiteSpace(updatedUserProfile.Email))
            {
                userProfileToBeUpdated.Email = updatedUserProfile.Email;
            }

            // Update City if provided
            if (!string.IsNullOrWhiteSpace(updatedUserProfile.City))
            {
                userProfileToBeUpdated.City = updatedUserProfile.City;
            }

            // Update Interest if provided
            if (!string.IsNullOrWhiteSpace(updatedUserProfile.Interest))
            {
                userProfileToBeUpdated.Interest = updatedUserProfile.Interest;
            }

            // Update ProfilePicture if provided
            if (updatedUserProfile.ProfilePicture != null)
            {
                userProfileToBeUpdated.ProfilePicture = updatedUserProfile.ProfilePicture;
            }

            // Update CurrentLocationId if provided
            if (updatedUserProfile.CurrentLocationId.HasValue)
            {
                userProfileToBeUpdated.CurrentLocationId = updatedUserProfile.CurrentLocationId.Value;
            }
         

            if (!_userProfileRepository.UpdateUserProfile(userProfileToBeUpdated))
            {
                ModelState.AddModelError("", "Something went wrong updating the user profile");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }


        [HttpDelete("{userProfileId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteUserProfile(int userProfileId)
        {
            if (!_userProfileRepository.UserProfileExists(userProfileId))
            {
                return NotFound();
            }

      

         
          
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            return NoContent();
        }

        [HttpPost("UpdateUserImage/{userProfileId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateUserImage(int userProfileId, IFormFile imageFile)
        {
            if (!_userProfileRepository.UserProfileExists(userProfileId))
                return NotFound();

            if (imageFile == null || imageFile.Length <= 0)
            {
                ModelState.AddModelError("Image", "Please provide a valid image file.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Read the image data from the uploaded file into a byte array
            byte[] imageData;
            using (var stream = new MemoryStream())
            {
                imageFile.CopyTo(stream);
                imageData = stream.ToArray();
            }

            // Update the user's profile with the image data
            var userProfile = _userProfileRepository.GetUserProfile(userProfileId);
            userProfile.ProfilePicture = imageData; // Assuming you have a property for profile image in your UserProfile model

            if (!_userProfileRepository.UpdateUserProfile(userProfile))
            {
                ModelState.AddModelError("", "Something went wrong updating user image.");
                return StatusCode(500, ModelState);
            }

            return Ok();
        }


        [HttpPost("UpdateUserNotificationToken/{userProfileId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateUserNotificationToken(int userProfileId,string notificationToken)
        {
            if (!_userProfileRepository.UserProfileExists(userProfileId))
                return NotFound();

          

           


         
            var userProfile = _userProfileRepository.GetUserProfile(userProfileId);
            userProfile.NotificationToken = notificationToken; // Assuming you have a property for profile image in your UserProfile model

            if (!_userProfileRepository.UpdateUserProfile(userProfile))
            {
                ModelState.AddModelError("", "Something went wrong updating user image.");
                return StatusCode(500, ModelState);
            }

            return Ok();
        }
    }
}
