using Microsoft.AspNetCore.Mvc;
using Places.Dto;
using Places.Interfaces;
using System.Threading.Tasks;

namespace Places.Controllers // Note the corrected namespace
{

    [Route("api/[controller]")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly FriendService _friendService;
        private readonly IUserProfileRepository _userProfileRepository;

        public FriendController(FriendService friendService, IUserProfileRepository userProfileRepository)
        {
            _friendService = friendService;
            _userProfileRepository = userProfileRepository;
        }

        [HttpPost("sendFriendRequest")]
        public async Task<IActionResult> SendFriendRequest([FromBody] FriendRequestDto request)
        {
            try
            {
                await _friendService.SendFriendRequest(request.SenderId, request.ReceiverId, (float)request.Latitude, (float)request.Longitude);
                return Ok("Friend request sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("pendingFriendRequests/{userId}")]
        public async Task<IActionResult> GetPendingFriendRequests(int userId)
        {
            try
            {
                var requests = await _friendService.GetPendingFriendRequests(userId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("acceptedFriendRequests/{userId}")]
        public async Task<IActionResult> GetAcceptedFriendRequests(int userId)
        {
            try
            {
                var requests = await _friendService.GetAcceptedFriendRequests(userId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("acceptFriendRequest/{requestId}")]
        public async Task<IActionResult> AcceptFriendRequest(int requestId)
        {
            try
            {
                await _friendService.AcceptFriendRequest(requestId);
                return Ok("Friend request accepted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("declineFriendRequest/{requestId}")]
        public async Task<IActionResult> DeclineFriendRequest(int requestId)
        {
            try
            {
                await _friendService.DeclineFriendRequest(requestId);
                return Ok("Friend request declined.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("count/{userId}")]
        public async Task<IActionResult> GetFriendCount(int userId)
        {
            try
            {
                var count = await _friendService.GetFriendCount(userId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("{userId1}/{userId2}")]
        public async Task<IActionResult> DeleteFriend(int userId1, int userId2)
        {
            try
            {
                await _userProfileRepository.DeleteFriend(userId1, userId2);
                return NoContent(); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }



    }
}
