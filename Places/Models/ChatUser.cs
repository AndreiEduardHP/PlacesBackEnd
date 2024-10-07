namespace Places.Models
{
    public class ChatUser
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public int? LastReadMessageId { get; set; } 
        public string LastMessage {  get; set; }
        public DateTime Timestamp { get; set; }
    }

}
