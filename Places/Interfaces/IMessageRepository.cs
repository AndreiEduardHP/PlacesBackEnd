namespace Places.Interfaces;
using Places.Models;

    public interface IMessageRepository
    {
    Task<IEnumerable<Message>> GetAllMessagesAsync();
    Task<IEnumerable<Message>> GetMessagesByChatIdAsync(int chatId);
    Task<Message> GetMessageByIdAsync(int messageId);
    Task<Message> CreateMessageAsync(Message message);
    Task UpdateMessageAsync(Message message);
    Task DeleteMessageAsync(int messageId);

}
