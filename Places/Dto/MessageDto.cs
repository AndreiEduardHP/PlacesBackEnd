namespace Places.Dto
{
    public class MessageDto
    {

        public int Id { get; set; }
        public string Text { get; set; }
        public int SenderId { get; set; }
        public int ChatId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
