using Microsoft.EntityFrameworkCore;
using Places.Data;
using Places.Dto;
using Places.Interfaces;
using Places.Models;

namespace Places.Repository
{
    public class ChatsRepository : IChatRepository
    {

        private readonly PlacesContext _context;

        public ChatsRepository(PlacesContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            return await _context.UserProfile
                .Where(u => u.Id == userId)
                .Select(u => new UserProfileDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Username = u.Username,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    City = u.City,
                    Interest = u.Interest,
                    NotificationToken=u.NotificationToken,
                    ProfilePicture = u.ProfilePicture
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ChatProfileDto>> GetAllChatsAsync(int userId)
        {
            // Retrieve all chats where the user is involved
            var userChats = await _context.Chats
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .ToListAsync();

            
            var chatProfiles = new List<ChatProfileDto>();

            foreach (var chat in userChats)
            {
                int otherUserId = chat.User1Id == userId ? chat.User2Id : chat.User1Id;

               
                var userProfile = await GetUserProfileAsync(userId);
                var otherUserProfile = await GetUserProfileAsync(otherUserId);
                var messages = await _context.Messages
                       .Where(m => m.ChatId == chat.Id)
                       .Select(m => new MessageDto
                       {
                           Id = m.Id,
                           Text = m.Text,
                           SenderId = m.SenderId,
                           ChatId = m.ChatId,
                           Timestamp = m.Timestamp
                       })
                       .ToListAsync();

               
                var chatProfile = new ChatProfileDto
                {
                    ChatId = chat.Id,
                    CurrentUser = userProfile,
                    SecondUser = otherUserProfile,
                    Messages = messages
                };

           
                chatProfiles.Add(chatProfile);
            }

            return chatProfiles;
        }

        public async Task<Chat> GetChatByIdAsync(int chatId)
        {
            return await _context.Chats.FirstOrDefaultAsync(c => c.Id == chatId);
        }

        public async Task<Chat> CreateChatAsync(Chat chat)
        {
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
            return chat;
        }

        public async Task UpdateChatAsync(Chat chat)
        {
            _context.Entry(chat).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteChatAsync(int chatId)
        {
            var chat = await _context.Chats.FindAsync(chatId);
            if (chat != null)
            {
                _context.Chats.Remove(chat);
                await _context.SaveChangesAsync();
            }
        }

        public ChatProfileDto GetMyChatRooms(int user1Id, int user2Id)
        {
            throw new NotImplementedException();
        }
    }
}
