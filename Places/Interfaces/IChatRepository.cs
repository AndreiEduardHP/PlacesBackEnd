using Places.Models;
using Places.Dto;

namespace Places.Interfaces
{
    public interface IChatRepository
    {
        Task<IEnumerable<ChatProfileDto>> GetAllChatsAsync(int userId,int numberOfMessages);
        Task<Chat> GetChatByIdAsync(int chatId);

        Task<IEnumerable<MessageDto>> GetMessagesByChatId(int chatId, int numberOfMessages);
        Task<Chat> CreateChatAsync(Chat chat);
        Task UpdateChatAsync(Chat chat);
        Task DeleteChatAsync(int chatId);
        Task<int> GetChatRoom(int user1, int user2);
        ChatProfileDto GetMyChatRooms(int user1Id, int user2Id);
        Task MarkMessagesAsReadAsync(int chatId, int userId);
        Task<int> GetNumberOfMessages(int userId);
    }
}
