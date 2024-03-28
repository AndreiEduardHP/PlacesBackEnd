using Microsoft.AspNetCore.Mvc;
using Places.Interfaces;
using Places.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Places.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatsController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;

        public ChatsController(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllChats(int userId)
        {
            var chatProfiles = await _chatRepository.GetAllChatsAsync(userId);
            return Ok(chatProfiles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Chat>> GetChat(int id)
        {
            var chat = await _chatRepository.GetChatByIdAsync(id);
            if (chat == null)
            {
                return NotFound();
            }
            return Ok(chat);
        }


        [HttpPost]
        public async Task<ActionResult<Chat>> CreateChat(Chat chat)
        {
            var newChat = await _chatRepository.CreateChatAsync(chat);
            return CreatedAtAction(nameof(GetChat), new { id = newChat.Id }, newChat);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChat(int id, Chat chat)
        {
            if (id != chat.Id)
            {
                return BadRequest();
            }
            await _chatRepository.UpdateChatAsync(chat);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChat(int id)
        {
            await _chatRepository.DeleteChatAsync(id);
            return NoContent();
        }
    }
}
