using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Places.Data;
using Places.Models;
using Places.Interfaces;

namespace Places.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbackController(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddFeedback([FromBody] Feedback feedback)
        {
            try
            {
                feedback.Timestamp = DateTime.Now; // Set current timestamp
                await _feedbackRepository.AddFeedbackAsync(feedback);
                return Ok("Feedback added successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
