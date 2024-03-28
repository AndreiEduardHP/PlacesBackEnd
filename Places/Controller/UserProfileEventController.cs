using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using Places.Dto;
using Places.Interfaces;
using Places.Models;
using QRCoder;
using System.Linq;

namespace Places.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileEventController : ControllerBase
    {

        private readonly IUserProfileEventRepository _userProfileEventRepository;
      
        private readonly IMapper _mapper;
        public UserProfileEventController(IUserProfileEventRepository userProfileEventRepository,  IMapper mapper)
        {
            _userProfileEventRepository = userProfileEventRepository;
          
            _mapper = mapper;
        }


        [HttpPost("joinevent")]
        public IActionResult JoinEvent(UserProfileEventDto userProfileEventDto)
        {
            try
            {
                string qrCodeContent = $"EventId:{userProfileEventDto.EventId},UserId:{userProfileEventDto.UserProfileId}";

                // Generate QR code
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeContent, QRCodeGenerator.ECCLevel.Q))
                    {
                        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                        {
                            byte[] qrCodeBytes = qrCode.GetGraphic(20);
                            userProfileEventDto.QRCode = Convert.ToBase64String(qrCodeBytes); // Save QR code as Base64 string
                        }
                    }
                }


                var userProfileEvent = _mapper.Map<UserProfileEvent>(userProfileEventDto);

                bool joined = _userProfileEventRepository.JoinEvent(userProfileEvent);

                if (joined)
                {
                    return Ok("Joined event successfully");
                }
                else
                {
                    return BadRequest("Failed to join event");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("GetQRCode/{eventId}/{userId}")]
        public IActionResult GetQRCodeForEvent(int eventId, int userId)
        {
            var qrCode = _userProfileEventRepository.GetQRCodeForUserEvent(userId, eventId);

            if (string.IsNullOrEmpty(qrCode))
            {
                return NotFound("QR Code not found.");
            }

            return Ok(new { QRCode = qrCode });
        }

        [HttpGet("JoinedEvents/{userId}")]
        public async Task<ActionResult<List<Event>>> GetEventsByUserId(int userId)
        {
            var events = await _userProfileEventRepository.GetJoinedEventsByUserId(userId);

            if (events == null || events.Count == 0)
            {
                return NotFound();
            }

            return Ok(events);
        }



        [HttpGet("checkIfUserJoined")]
        public ActionResult<bool> CheckIfUserJoined(int eventId, int userId)
        {
            try
            {
          
                var userJoined = _userProfileEventRepository.CheckIfUserJoined(eventId, userId);
                return Ok(userJoined);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while checking if the user joined the event.");
            }
        }

        // POST api/userprofileevent/unjoinevent
        [HttpPost("unjoinevent")]
        public IActionResult UnJoinEvent(int eventId, int userId)
        {
            try
            {
                bool unjoined = _userProfileEventRepository.UnJoinEvent(eventId, userId);

                if (unjoined)
                {
                    return Ok("Unjoined event successfully");
                }
                else
                {
                    // Could not find the entity to delete
                    return NotFound("No matching event found to unjoin");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("event/{eventId}/userprofiles")]
        public IActionResult GetUserProfilesByEventId(int eventId)
        {
            try
            {
             
                var (userProfiles, totalCount) = _userProfileEventRepository.GetUserProfilesByEventId(eventId);

             
                var userProfileDtos = _mapper.Map<IEnumerable<UserProfileDto>>(userProfiles ?? Enumerable.Empty<UserProfile>());

              
                var response = new
                {
                    CountParticipants = totalCount,
                    UserProfiles = userProfileDtos
                };

             
                return Ok(response);
            }
            catch (Exception e)
            {
           
                Console.WriteLine(e); 
                return StatusCode(500, "Internal server error");
            }
        }
    }

}
