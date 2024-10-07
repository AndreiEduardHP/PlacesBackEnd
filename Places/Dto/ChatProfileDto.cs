using Places.Dto;

public class ChatProfileDto
{
    public int ChatId { get; set; }
    public bool AreFriends { get; set; }
    public string FriendRequestStatus { get; set; }
    public int UnreadMessagesCount { get; set; }
    public string LastMessage { get; set; }
    public UserProfileDto CurrentUser { get; set; }
    public UserProfileDto SecondUser { get; set; }

    public List<MessageDto> Messages { get; set; }
}
