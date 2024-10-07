using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using Places.Data;
using Places.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Message = Places.Models.Message;

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
            
            var groupName = GetGroupName(senderId, receiverId);
           

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

            TimeZoneInfo romaniaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
            DateTime romaniaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, romaniaTimeZone);
            // Creăm și salvăm noul mesaj
            var messageEntity = new Message
            {
                SenderId = senderId,
                ChatId = chat.Id,
                Text = message,
               
            Timestamp = romaniaTime
            };

            _context.Messages.Add(messageEntity);
            await _context.SaveChangesAsync();

            await Clients.Group(groupName).SendAsync("ReceiveMessage",messageEntity);
            
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId1String = httpContext.Request.Headers["userId1"];
            var userId2String = httpContext.Request.Headers["userId2"];

            // Convert StringValues to int
            if (int.TryParse(userId1String, out int userId1) && int.TryParse(userId2String, out int userId2))
            {
                var groupName = GetGroupName(userId1, userId2);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                Console.WriteLine($"User {userId1} connected to group {groupName}");
            }
            else
            {
                Console.WriteLine("Invalid user IDs provided.");
            }

            await base.OnConnectedAsync();
        }

        private string GetGroupName(int user1, int user2)
        {
            var ids = new[] { user1, user2 };
            Array.Sort(ids);

            return $"chat_{ids[0]}_{ids[1]}";
        }
    }
}
