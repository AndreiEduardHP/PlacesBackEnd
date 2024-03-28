using Microsoft.AspNetCore.Mvc;
using Places.Interfaces;
using Places.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Places.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;

        public MessagesController(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages()
        {
            return Ok(await _messageRepository.GetAllMessagesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetMessage(int id)
        {
            var message = await _messageRepository.GetMessageByIdAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            return Ok(message);
        }

        [HttpGet("chat/{chatId}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessagesForChat(int chatId)
        {
            var messages = await _messageRepository.GetMessagesByChatIdAsync(chatId);
            if (messages == null)
            {
                return NotFound();
            }
            return Ok(messages);
        }

        [HttpPost]
        public async Task<ActionResult<Message>> CreateMessage(Message message)
        {
            var newMessage = await _messageRepository.CreateMessageAsync(message);
            return CreatedAtAction(nameof(GetMessage), new { id = newMessage.Id }, newMessage);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMessage(int id, Message message)
        {
            if (id != message.Id)
            {
                return BadRequest();
            }
            await _messageRepository.UpdateMessageAsync(message);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            await _messageRepository.DeleteMessageAsync(id);
            return NoContent();
        }
    }
}
