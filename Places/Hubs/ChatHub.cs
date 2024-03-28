using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Places.Data;
using Places.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRChat.Hubs
{
    public class ChatHub : Hub
    {
        private readonly PlacesContext _context;

        public ChatHub(PlacesContext context)
        {
            _context = context;
        }

        public async Task SendMessage(int senderId, int receiverId, string message)
        {
            
            var groupName = GetGroupName(senderId.ToString(), receiverId.ToString());
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Verificăm dacă există un chat între sender și receiver
            var chat = await _context.Chats
                .FirstOrDefaultAsync(c => (c.User1Id == senderId && c.User2Id == receiverId) || (c.User1Id == receiverId && c.User2Id == senderId));

            if (chat == null)
            {
                // Dacă nu există, creăm un nou chat
                chat = new Chat
                {
                    User1Id = senderId,
                    User2Id = receiverId
                };
                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();
            }
        

            // Creăm și salvăm noul mesaj
            var messageEntity = new Message
            {
                SenderId = senderId,
                ChatId = chat.Id,
                Text = message,
             
            Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(messageEntity);
            await _context.SaveChangesAsync();

            await Clients.Group(groupName).SendAsync("ReceiveMessage",messageEntity);
        }

        private string GetGroupName(string user1, string user2)
        {
            var ids = new List<string> { user1, user2 };
            ids.Sort();
            return $"chat_{ids[0]}_{ids[1]}";
        }
    }
}
