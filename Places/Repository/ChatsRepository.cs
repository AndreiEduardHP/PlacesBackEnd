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
        private readonly IUserProfileRepository _userProfileRepository;

        public ChatsRepository(PlacesContext context, IUserProfileRepository userProfileRepository)
        {
            _context = context;
            _userProfileRepository = userProfileRepository;
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
                    Description = u.Description,
                    NotificationToken=u.NotificationToken,
                    ProfilePicture = u.ProfilePicture
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ChatProfileDto>> GetAllChatsAsync(int userId,int numberOfMessages)
        {
            // Retrieve all chats where the user is involved
            var userChats = await _context.Chats
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .ToListAsync();

            
            var chatProfiles = new List<ChatProfileDto>();

            foreach (var chat in userChats)
            {
                int otherUserId = chat.User1Id == userId ? chat.User2Id : chat.User1Id;
                bool areFriends = await _userProfileRepository.AreFriends(userId, otherUserId);
                string friendRequestStatus = await _userProfileRepository.GetFriendRequestStatus(userId, otherUserId);

              //  var userProfile = await GetUserProfileAsync(userId);
                var otherUserProfile = await GetUserProfileAsync(otherUserId);

             

                var unreadMessagesCount = _context.Messages
        .Where(m => m.ChatId == chat.Id  && m.SenderId != userId && !m.IsRead)
        .Count();

              
               


                var chatProfile = new ChatProfileDto
                {
                    ChatId = chat.Id,
                 //   CurrentUser = userProfile,
                    SecondUser = otherUserProfile,
                 //   Messages = messages,
                     AreFriends = areFriends,
                    UnreadMessagesCount = unreadMessagesCount,
                 
                    FriendRequestStatus = friendRequestStatus
                };

           
                chatProfiles.Add(chatProfile);
            }

            return chatProfiles;
        }


        public async Task<int> GetNumberOfMessages(int userId)
        {
            int unreadMessagesCount = await _context.Messages
       .Where(m =>
           (m.Chat.User1Id == userId || m.Chat.User2Id == userId) &&
           m.SenderId != userId &&
           !m.IsRead)
       .CountAsync();

            return unreadMessagesCount;
        }


        public async Task MarkMessagesAsReadAsync(int chatId, int userId)
        {
            var messages = _context.Messages
                .Where(m => m.ChatId == chatId && m.SenderId != userId && !m.IsRead)
                .ToList();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

           // var lastReadMessageId = messages.LastOrDefault()?.Id;
          

            _context.SaveChanges();
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

        public async Task<int> GetChatRoom(int user1, int user2)
        {
            var chatRoom = await _context.Chats
                                .FirstOrDefaultAsync(chat =>
                                    (chat.User1Id == user1 && chat.User2Id == user2) ||
                                    (chat.User1Id == user2 && chat.User2Id == user1));

            return chatRoom.Id;
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesByChatId(int chatId, int numberOfMessages)
        {
          var messages = await _context.Messages
                  .Where(m => m.ChatId == chatId)
                  //  .OrderByDescending(m => m.Timestamp)
           //    .Take(numberOfMessages)
             
              .Select(m => new MessageDto
                {
                     Id = m.Id,
                    Text = m.Text,
                    SenderId = m.SenderId,
                     ChatId = m.ChatId,
                     Timestamp = m.Timestamp
                  })
                   .ToListAsync();


            return messages;
        }


    }
}
