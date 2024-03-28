using Places.Models;
using Places.Dto;

namespace Places.Interfaces
{
    public interface IChatRepository
    {
        Task<IEnumerable<ChatProfileDto>> GetAllChatsAsync(int userId);
        Task<Chat> GetChatByIdAsync(int chatId);
        Task<Chat> CreateChatAsync(Chat chat);
        Task UpdateChatAsync(Chat chat);
        Task DeleteChatAsync(int chatId);
        ChatProfileDto GetMyChatRooms(int user1Id, int user2Id);
    }
}
