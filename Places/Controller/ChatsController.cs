using Microsoft.AspNetCore.Mvc;
using Places.Dto;
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
        public async Task<IActionResult> GetAllChats(int userId, int numberOfMessages)
        {
            var chatProfiles = await _chatRepository.GetAllChatsAsync(userId, numberOfMessages);
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
        [HttpGet("getChatMessages")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesByChatId(int chatId, int numberOfMessages)
        {
            var messages = await _chatRepository.GetMessagesByChatId(chatId, numberOfMessages);
            if (messages == null)
            {
                return NotFound();
            }
            return Ok(messages);
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

        [HttpGet("GetChatRoom")]
        public async Task<IActionResult> GetChatRoom(int user1, int user2)
        {
            var chatId = await _chatRepository.GetChatRoom(user1, user2);

            if (chatId != null)
            {
                return Ok(chatId);
            }
            else
            {
                return NotFound("Chat room not found.");
            }
        }

        [HttpPost("markAsRead")]
        public async Task<IActionResult> MarkMessagesAsReadAsync([FromQuery] int ChatId, [FromQuery] int UserId)
        {
            if (ChatId <= 0 || UserId <= 0)
            {
                return BadRequest("Invalid ChatId or UserId.");
            }

            try
            {
                await _chatRepository.MarkMessagesAsReadAsync(ChatId, UserId);
                return Ok(new { message = "Messages marked as read." });
            }
            catch (Exception ex)
            {
                // Loghează excepția ex și oferă un răspuns de eroare corespunzător
                return StatusCode(500, "An error occurred while marking messages as read.");
            }
        }


        [HttpGet("unreadMessagesCount")]
        public async Task<IActionResult> GetUnreadMessagesCount(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid userId.");
            }

            try
            {
                int unreadMessagesCount = await _chatRepository.GetNumberOfMessages(userId);
                return Ok(new { UnreadMessagesCount = unreadMessagesCount });
            }
            catch (Exception ex)
            {
                // Loghează excepția ex și oferă un răspuns de eroare corespunzător
                return StatusCode(500, "An error occurred while retrieving unread messages count.");
            }
        }
    }
}
