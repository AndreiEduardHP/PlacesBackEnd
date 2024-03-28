namespace Places.Dto
{
    public class FriendRequestDto
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }

        public int RequestId { get; set; }
    
        public string? SenderName { get; set; } // Full name of the sender
        public byte[]? SenderPicture { get; set; }
        public DateTime RequestDate { get; set; } // When the request was sent
        // You can add other properties as needed, like SenderUsername, SenderEmail, etc.
    }
}
