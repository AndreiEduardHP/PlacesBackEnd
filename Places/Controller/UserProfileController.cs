using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Places.Dto;
using Places.Interfaces;
using Places.Models;
using System.Net.Mail;
using System.Net;
using System.Text;
using Azure.Core;


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
                var userProfiles = _userProfileRepository.GetUserProfiles().Where(up => up.Id != currentUserId && up.ProfileVisibility !="Private");

                
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
                        CurrentLatitude = userProfile.CurrentLatitude,
                        CurrentLongitude = userProfile.CurrentLongitude,
                        AreFriends = areFriends,
                        FriendRequestStatus = friendRequestStatus,
                        Description = userProfile.Description
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

        [HttpPost("shares/{userProfileId}")]
        [ProducesResponseType(200, Type = typeof(UserProfile))]
        [ProducesResponseType(400)]
        public IActionResult SharesCount(int userProfileId)
        {
            if (!_userProfileRepository.UserProfileExists(userProfileId))
                return NotFound();

            var userProfile = _userProfileRepository.GetUserProfile(userProfileId);
            userProfile.Shares += 1;
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!_userProfileRepository.UpdateUserProfile(userProfile))
            {
                ModelState.AddModelError("", "Something went wrong updating user image.");
                return StatusCode(500, ModelState);
            }
            return Ok(userProfile);
        }


        [HttpGet("CheckIfPhoneNumberExists")]
      
        public IActionResult CheckIfPhoneNumberExists(string phoneNumber)
        {
            if (!_userProfileRepository.CheckPhoneNumberExists(phoneNumber))
                return NoContent();

       

            return Ok();
        }
        [HttpGet("CheckIfEmailExists")]

        public IActionResult CheckIfEmailExists(string email)
        {
            if (!_userProfileRepository.CheckEmailNumberExists(email))
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

            var userProfileExist = _userProfileRepository.GetUserProfiles().Where(up => up.PhoneNumber == userProfileCreate.PhoneNumber || (up.Email == userProfileCreate.Email && up.EmailVerified == true)).FirstOrDefault();

            if (userProfileExist != null)
            {
                ModelState.AddModelError("", "User Profile already exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userProfileMap = _mapper.Map<UserProfile>(userProfileCreate);
            userProfileMap.CurrentLatitude = 1;
            userProfileMap.CurrentLongitude = 1;
            userProfileMap.UserLocation = _locationRepository.GetLocation(locationId);
            userProfileMap.DateAccountCreation = DateTime.Now;
            userProfileMap.Credit = 0;
            userProfileMap.ProfileVisibility = "Visible to everyone";
            userProfileMap.EmailVerified = false;
            userProfileMap.Description ="-";

            var w = _userProfileRepository.CreateUserProfile(userProfileMap);
            if (w == null)
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }
            int createdUserProfileId = userProfileMap.Id;
            return Ok(new { Message = "Successfully created", UserProfileId = createdUserProfileId });
        }

        [HttpPut("{userProfileId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateUserProfile(int userProfileId, [FromBody] UserProfileDto updatedUserProfile)
        {
            var userProfileToBeUpdated = _userProfileRepository.GetUserProfile(userProfileId);
            if(userProfileToBeUpdated.Email != updatedUserProfile.Email) {
                if (_userProfileRepository.CheckEmailNumberExists(updatedUserProfile.Email))
                {
                    return StatusCode(409);
                };
            }
           
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

           
            if (updatedUserProfile.CurrentLatitude != null)
            {
                userProfileToBeUpdated.CurrentLatitude = updatedUserProfile.CurrentLatitude;
            }
            if (updatedUserProfile.CurrentLongitude != null)
            {
                userProfileToBeUpdated.CurrentLongitude = updatedUserProfile.CurrentLongitude;
            }
            if (updatedUserProfile.Description != null)
            {
                userProfileToBeUpdated.Description = updatedUserProfile.Description;
            }
            if (userProfileToBeUpdated.Email != updatedUserProfile.Email) {
                userProfileToBeUpdated.EmailVerified = false;
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
        public IActionResult UpdateUserImage(int userProfileId, [FromForm] string imageFile)
        {
            if (!_userProfileRepository.UserProfileExists(userProfileId))
                return NotFound(new { message = "User not found." });

            if (string.IsNullOrEmpty(imageFile))
            {
                ModelState.AddModelError("Image", "Please provide a valid image URL.");
                return BadRequest(ModelState);
            }

            // Update the user's profile with the image URL
            var userProfile = _userProfileRepository.GetUserProfile(userProfileId);
            if (userProfile == null)
                return NotFound(new { message = "User profile not found." });

            userProfile.ProfilePicture = imageFile; // Assuming you have a property for profile image URL in your UserProfile model

            if (!_userProfileRepository.UpdateUserProfile(userProfile))
            {
                ModelState.AddModelError("", "Something went wrong updating the user image.");
                return StatusCode(500, ModelState);
            }

            return NoContent(); // Returning 204 No Content since the update was successful but no content needs to be returned
        }


        [HttpPost("UpdateUserNotificationToken/{userProfileId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateUserNotificationToken(int userProfileId,string notificationToken,double latitude, double longitude)
        {
            if (!_userProfileRepository.UserProfileExists(userProfileId))
                return NotFound();

          

           


         
            var userProfile = _userProfileRepository.GetUserProfile(userProfileId);
            userProfile.NotificationToken = notificationToken; 
            userProfile.CurrentLatitude = latitude;
            userProfile.CurrentLongitude = longitude;
            if (!_userProfileRepository.UpdateUserProfile(userProfile))
            {
                ModelState.AddModelError("", "Something went wrong updating user image.");
                return StatusCode(500, ModelState);
            }

            return Ok();
        }

        [HttpGet("CheckUserDataAwards/{userId}")]
        [ProducesResponseType(200, Type = typeof(UserDataAwardsDto))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CheckUserDataAwards(int userId)
        {
           

            var userDataAwards = await _userProfileRepository.CheckUserDataAwards(userId);
            if (userDataAwards == null)
                return NotFound();

            return Ok(userDataAwards);
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(int userId)
        {
            var userProfile = _userProfileRepository.GetUserProfile(userId);
            if (userProfile == null)
            {
                return BadRequest("Invalid id.");
            }

            userProfile.EmailVerified = true;


            if (!_userProfileRepository.UpdateUserProfile(userProfile))
            {
                ModelState.AddModelError("", "Something went wrong updating user image.");
                return StatusCode(500, ModelState);
            }

           

            return Ok("Email verified successfully.");
        }
        [HttpPost("send/email/{userId}")]
        
        public IActionResult SendEmail(int userId)
        {

            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com") // Înlocuiește cu serverul tău SMTP
                {
                    Port = 587, // Portul standard pentru SMTP
                    Credentials = new NetworkCredential("romaniahp@gmail.com", "nbml imuw jlgj dnio"), // Înlocuiește cu credențialele tale
                    EnableSsl = true,
                };
                var foundUser = _userProfileRepository.GetUserProfile(userId);

                   string link = $"https://placesbyteunit.azurewebsites.net/api/userprofile/verify-email?userId={userId}";
              //  string link = $"http://192.168.1.228:8080/api/userprofile/verify-email?userId={userId}";
                string base64 = "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNzA0IiBoZWlnaHQ9IjE4MyIgdmlld0JveD0iMCAwIDcwNCAxODMiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxwYXRoIGQ9Ik0yNTcuMzY5IDE4LjExOTZDMjYwLjIxNSA4LjM2NTAyIDI2NC45NCAzIDI3My4zMDcgM0MyODEuNjc1IDMgMjg1LjIwMyAxMi43NTQ2IDI4Ny4yNTQgMTguMTE5NkwzMTguNDM2IDk4LjQ0NjlDMzE5LjE2IDEwMC4zMTIgMzE4Ljc0IDEwMi40OTkgMzE3LjA3OCAxMDMuNjEzQzMwOS4wNDYgMTA4Ljk5NSAzMDAuNTI4IDEwNC4wMTcgMjk1LjcxMiA5OS4yOTk0QzI5NC4zMiA5Ny45MzU0IDI5My4zNTMgOTYuMjE1NiAyOTIuNjEzIDk0LjQxMjNMMjczLjMwNyA0Ny4zODM0TDIzMi4xMTkgMTQ1LjcyMkMyMzAuMDM5IDE1MC42ODggMjI3LjM0IDE1NS45NCAyMjIuMTk4IDE1Ny41NEMyMjAuMDYzIDE1OC4yMDQgMjE3LjYxMyAxNTguNCAyMTUuMDMxIDE1Ny42MUMyMTEuODM3IDE1Ni42MzMgMjEwLjAxIDE1NC4yMjcgMjA5LjAyMyAxNTEuNjE5QzIwNy4zMTEgMTQ3LjA5MiAyMDguOTcgMTQyLjE3NiAyMTAuNzc0IDEzNy42ODRDMjI2LjU5NCA5OC4yNzQgMjU0LjM2MiAyOC40MjUzIDI1Ny4zNjkgMTguMTE5NloiIGZpbGw9IndoaXRlIi8+CjxwYXRoIGQ9Ik0zMDYuMjkyIDEyNC45MzdDMzA2Ljk4MyAxMTkuMjUzIDMxMC43ODMgMTE4LjIzOSAzMTQuMTUgMTE3LjYxNkMzMjAuOTI0IDExNi4zNjQgMzI0LjMzMiAxMTguODA2IDMyNi44NCAxMjEuMjY5QzMyNy42NDkgMTIyLjA2NCAzMjguMjc4IDEyMy4wMjEgMzI4Ljc0NyAxMjQuMDU1TDMzMy41MyAxMzQuNTkyQzMzNi40NzYgMTQxLjA4MyAzMzkuNTQ3IDE0OS4xMTEgMzM1LjA1NSAxNTQuNjQ1QzMzMy44ODYgMTU2LjA4NSAzMzIuMjggMTU3LjE4NCAzMzAuMDg5IDE1Ny42MUMzMjIuOTg0IDE1OC45OTEgMzE4LjIwMSAxNTYuNTI3IDMxNS40ODEgMTQ5LjgxOUwzMDYuOTE0IDEyOC42OTFDMzA2LjQzIDEyNy40OTcgMzA2LjEzNiAxMjYuMjE1IDMwNi4yOTIgMTI0LjkzN1oiIGZpbGw9IndoaXRlIi8+CjxwYXRoIGQ9Ik0xMTAuMjE0IDEyQzExMC4yMjEgNyAxMTQuNDk0IDMgMTIyLjIxNCAzQzEzMS44MTQgMyAxMzMuNzE0IDggMTMzLjcxNCAxMlYxMzQuNUgxNzEuNjg0QzE3Ni4yMDcgMTM0LjUgMTgxLjEzIDEzNC43NDUgMTg0LjI0NSAxMzguMDI0QzE4Ni4xNjcgMTQwLjA0NiAxODcuNjE3IDE0Mi45NzQgMTg3LjIxNCAxNDdDMTg2LjY5MiAxNTIuMjIgMTg0LjEzNSAxNTUuMTY5IDE4MS43NzIgMTU2LjY4MUMxNzkuNzggMTU3Ljk1NiAxNzcuMzIgMTU4IDE3NC45NTQgMTU4SDEyMi4yMTRDMTEzLjcxNSAxNTggMTEwLjg4MSAxNTMuMTY3IDExMC4yMTQgMTQ5LjVDMTEwLjA0OCAxMDYuMTY3IDExMC4yMDcgMTguMDEzMyAxMTAuMjE0IDEyWiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTQ2LjMyODIgNjcuNDk3M0gxOS41MDAyTDE5LjUwMDIgOTJINDYuMzI4MkM1OC41MDAyIDkyIDcyLjM4NTggODMuNjY2NyA3OCA3OC41Qzg3LjUgNjcuNDk3MyA4OC45OTk4IDU1IDg5IDQ2Ljc3MTRDODkuMDAwMyAzNS40NTAxIDgyIDIzLjUxNDMgNzggMThDNzEuMTA4OCA4LjUgNTQuODg1NSA0LjE2NjY3IDQ2LjMyODIgM0wxMS41MDAyIDNMMTkuNTAwMiAyNy41TDQ2LjMyODIgMjcuNUM1Mi41ODAyIDI3LjUgNjMuMDAwMiAzMC4wNDUzIDYzIDQ2Ljc3MTRDNjIuOTk5OCA2My43NjU0IDUxLjE5MDggNjcuNjE4NSA0Ni4zMjgyIDY3LjQ5NzNaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNMC4yMjA4NjEgMTJDMC42MjA4NjEgNiA3LjE2NjY3IDMuNSAxMSAzQzIwLjYgMyAyMy44ODc0IDkgMjMuNzIwNyAxMlYxMzQuNVYxNDkuNUMyMy43MjA3IDE1MiAyMS44MjA5IDE1OCAxMi4yMjA5IDE1OEMzLjcyMTAyIDE1OCAwLjg4NzU4IDE1My4xNjcgMC4yMjA4NjEgMTQ5LjVDMC4wNTQxOTQgMTA2LjE2NyAtMC4xNzkxMzkgMTggMC4yMjA4NjEgMTJaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNMzUwIDcyLjE3NEMzNTAgODEuODQ3MiAzNTkuNSA4NC41MjY1IDM1OS41IDg0LjUyNjVDMzYyLjU2MSA4NC44ODM1IDM2NC45NTMgODQuNDUxMiAzNjYuODIzIDgzLjU0N0MzNzYuMjAxIDc5LjAxMjMgMzc0LjI1NSA2NC43ODEzIDM3OC44ODYgNTUuNDUxQzM4My4yIDQ2Ljc2MTIgMzg5Ljg3OCAzOS40NDA2IDQwMy4wMDIgMzIuOTMxNkM0MjQuNTAxIDIyLjI2OTkgNDQ0LjE2OSAzMS4xMTk5IDQ1My41MDMgMzguMzY2N0M0NTcuNTAzIDQxLjY2MDcgNDY2LjUwMSA0NC41MDQ0IDQ3MS4wMDMgMzguMzY2N0M0NzQuNTU5IDMzLjUxODUgNDcyLjU0NiAyNy40NTc0IDQ3MC41MDcgMjMuNTU1NUM0NjkuMDYgMjAuNzg2NSA0NjYuODQ2IDE4LjUxODIgNDY0LjMwMSAxNi43MDYxQzQ1MC41MTggNi44OTIzNSA0MjEuNjU3IC00Ljg3NTg2IDM5MS41MDMgMTAuMjAyN0MzNTAuMDAzIDMwLjk1NDkgMzUwIDYyLjUwMDggMzUwIDcyLjE3NFoiIGZpbGw9IndoaXRlIi8+CjxwYXRoIGQ9Ik0zNTUuMjkxIDExNi4yMDlDMzYxLjc4IDEzNi4zNSAzOTAuMDg3IDE1OC4zMjcgNDExLjkzNiAxNTguMzI4QzQyMy43NjkgMTU5LjY0NiA0NDYuNDk5IDE1Ny4xNiA0NjAuOTM2IDE0Ni45NjRDNDY2LjQ5MSAxNDIuMjcxIDQ2OC41ODMgMTQwLjIyMiA0NzAuNDMzIDEzNS45MTdDNDcwLjc4MyAxMzUuMTAyIDQ3MS4wMDEgMTM0LjIzMyA0NzAuOTk2IDEzMy4zNUM0NzAuOTYxIDEyNi42OTQgNDY2LjA5NSAxMTYuNzM3IDQ1Mi4wNzggMTI0Ljg0M0M0NTEuNjU2IDEyNS4wODcgNDUxLjI0NSAxMjUuMzgzIDQ1MC44NjQgMTI1LjY4N0M0MzkuMDUgMTM1LjExOSA0MjcuMDc0IDEzNS41MzQgNDEzLjk5MyAxMzQuNjc4QzM4OS45MzcgMTMxLjc1MiAzNzkuODUxIDExNS43NjQgMzc1LjQ3OCAxMDQuODU5QzM3My43IDEwMC4wMjMgMzY2LjY3MyA5NS41ODQxIDM1OS42NTcgOTguNjQ3NUMzNTMuMjI1IDEwMS40NTYgMzUzLjQ2MyAxMDguMTYxIDM1NC4xMDEgMTExLjY5NUMzNTQuMzc4IDExMy4yMjggMzU0LjgxMyAxMTQuNzI3IDM1NS4yOTEgMTE2LjIwOVoiIGZpbGw9IndoaXRlIi8+CjxjaXJjbGUgY3g9IjI3NCIgY3k9IjExOSIgcj0iMjAiIGZpbGw9InVybCgjcGFpbnQwX2xpbmVhcl82NzJfNTEpIi8+CjxwYXRoIGQ9Ik00OTYgMTQ4VjEzQzQ5NiA3LjQ3NzE1IDUwMC40NzcgMyA1MDYgM0g1NzMuOTMyQzU3Ni41OTYgMyA1ODEuOTI1IDUuMjIxNSA1ODEuOTI1IDE0LjEwNzVDNTgxLjkyNSAyMi45OTM1IDU3Ni41OTYgMjUuNTUxNiA1NzMuOTMyIDI1LjcxOTlINTE4LjQ4VjY2LjYxNTZINTYzLjQ0MUM1NjMuNDQxIDY2LjYxNTYgNTcwLjQzNSA2Ni42MTU3IDU3MC40MzUgNzkuMjM3OEM1NzAuNDM1IDkxLjM1NSA1NjMuNDQxIDkxLjM1NSA1NjMuNDQxIDkxLjM1NUg1MTguNDhWMTMzLjc2NUg1NzMuOTMyQzU3Ni4yNjMgMTM0LjI3IDU4MS4xMjYgMTM3LjMgNTgxLjkyNSAxNDUuMzc4QzU4Mi43MjQgMTUzLjQ1NiA1NzYuOTI5IDE1Ny4xNTkgNTczLjkzMiAxNThINTA2QzUwMC40NzcgMTU4IDQ5NiAxNTMuNTIzIDQ5NiAxNDhaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNjM4LjA4MSA4NS42ODUxQzYyMC41IDc4IDYxMy4zNzMgNjguMjI1NCA2MDguNzk3IDU5Ljc5NjNMNjQ0LjkyNyA2My4wMzg1QzY1MC42NTkgNjUuMDYxMiA2NzcuMzk2IDc1LjM2MTIgNjg4Ljk2OSA4NS4wMjgxQzY5OS41MjcgOTMuODQ3NSA3MDAuODg2IDEwNC42NzMgNzAxLjA0NSAxMDguMDUxQzcwMS4wNzggMTA4Ljc2IDcwMS4wNjMgMTA5LjQ3MyA3MDEuMDUyIDExMC4xODNDNzAwLjcwOCAxMzIuODkzIDY5MC4yODQgMTQyLjQ0NCA2ODUuMTggMTQ2Ljc0MUM2NzMuNTA0IDE1Ni41NzEgNjU3LjU4MyAxNjEuNDA5IDYzOC41OTcgMTU3LjE0QzYyNi41OSAxNTQuNDQxIDYxOC45MDMgMTQ5Ljg3NyA2MTQuMjM4IDE0Ni4wMjVDNjA5LjQ5NyAxNDIuMTExIDYwNC41MjkgMTM3LjA0IDYwNC4zMjYgMTMwLjg5MUM2MDQuMjc2IDEyOS4zOTYgNjA0LjQ5MSAxMjcuNzE3IDYwNS4xMiAxMjUuODY3QzYwNS42MTggMTI0LjQwNSA2MDYuNjQ0IDEyMy4xNzEgNjA3LjkzOCAxMjIuMzI2QzYxMy4xMzUgMTE4LjkzMyA2MTUuODg0IDEyMC4wMzkgNjIwLjk1NiAxMjIuMTM4QzYyNy42NDEgMTI0LjkwNCA2MzQuNzAzIDEzMS4wNjkgNjM4LjYyNCAxMzIuNjU2QzY0My4wMzQgMTM0LjQ0IDY1Ni44NCAxMzguNDk3IDY2NS41MzUgMTMyLjYyOUM2NzQuMjMxIDEyNi43NiA2ODAuMDUzIDExNi4xMjUgNjc0LjA2NSAxMDQuMTM4QzY2OC4wNzYgOTIuMTUxMyA2NTEuMTE1IDkxLjM4MjIgNjM4LjA4MSA4NS42ODUxWiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTY2My40MzggNjkuNTIzN0M2NzkuMTkgNzMuMDgzNCA2OTAuNTIxIDgzLjc4MiA2OTUuMTA1IDkyLjA0NTRMNjMzLjk2OSA4Mi41MTI1QzYyMi45NjkgNzUuOTcxOSA2MjguMzMyIDgxLjUzMjIgNjE4LjQ2OSA3My40NTYyQzYwOC4zMjMgNjUuMTQ5IDYwNS45MjYgNTEuNzA5MyA2MDUuMzczIDQ2LjY1MjZDNjA1LjIyNiA0NS4zMTA2IDYwNS4yMzQgNDMuOTYxNyA2MDUuMzQ1IDQyLjYxNThDNjA2LjYyNyAyNy4wODY4IDYxMy4yMTUgMTkuNzQ2OSA2MTguMTgyIDE1LjY1MDZDNjI5Ljg0OCA2LjAzMDIxIDY0NS43NjEgLTAuNjUzODg0IDY2NC43NTEgMy41NDY3MUM2ODguNDg4IDguNzk3NDUgNjk1LjczMyAxOS4zODc2IDY5NS43MzMgMTkuMzg3NkM2OTUuNzMzIDE5LjM4NzYgNzAwLjQ5MSAyNC4yMzIzIDY5NS43MzMgMzMuNzgyNkM2OTIuNDQzIDM5LjI3MDkgNjgzLjU4OCAzOC40OTg1IDY4MC41ODggMzYuNDMxNUM2NzMuMjQ3IDMxLjM3MzcgNjY3LjM2IDI4LjQ4NjIgNjYzLjQzOCAyNi45MjgyQzY1OS4wMjYgMjUuMTc1NSA2NDguMDI5IDI1Ljg0MTMgNjM5LjM0IDMxLjU4MjdDNjMwLjY1MSAzNy4zMjQxIDYyOC44MzQgNDUuODA3OCA2MzAuNjA1IDUwLjQyOTFDNjMyLjM3NyA1NS4wNTA1IDY0Ni4xMDUgNjUuNjA2OCA2NjMuNDM4IDY5LjUyMzdaIiBmaWxsPSJ3aGl0ZSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJwYWludDBfbGluZWFyXzY3Ml81MSIgeDE9IjI3NCIgeTE9Ijk5IiB4Mj0iMjc0IiB5Mj0iMTM5IiBncmFkaWVudFVuaXRzPSJ1c2VyU3BhY2VPblVzZSI+CjxzdG9wIG9mZnNldD0iMC4zNjE2NjciIHN0b3AtY29sb3I9IiMzMTlERkMiLz4KPHN0b3Agb2Zmc2V0PSIwLjgzMTY2NyIgc3RvcC1jb2xvcj0iIzI2NkVDMyIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+Cjwvc3ZnPgo=";
                string body = $@"
<p>Please press <a href='{link}'>here</a> to confirm your email.</p>
<div>
    <img src='{base64}' alt='SVG Image' />
</div>";
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("romaniahp@gmail.com"),
                    Subject = "Verify Email",
                    Body = $"<p>Please press <a href=\"{link}\">here</a> to confirm your email.</p> <div><img width='60' height='60' src={base64}</div>",



                IsBodyHtml = true, // Important pentru a trimite HTML
                };
                mailMessage.To.Add(foundUser.Email);

                smtpClient.Send(mailMessage);

                return Ok();

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
