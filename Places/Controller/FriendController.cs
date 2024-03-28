using Microsoft.AspNetCore.Mvc;
using Places.Dto;
using System.Threading.Tasks;

namespace Places.Controllers // Note the corrected namespace
{

    [Route("api/[controller]")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly FriendService _friendService;

        public FriendController(FriendService friendService)
        {
            _friendService = friendService;
        }

        [HttpPost("sendFriendRequest")]
        public async Task<IActionResult> SendFriendRequest([FromBody] FriendRequestDto request)
        {
            try
            {
                await _friendService.SendFriendRequest(request.SenderId, request.ReceiverId);
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



    }
}
