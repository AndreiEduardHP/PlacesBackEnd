using Places.Dto;

public class ChatProfileDto
{
    public int ChatId { get; set; }
    public UserProfileDto CurrentUser { get; set; }
    public UserProfileDto SecondUser { get; set; }

    public List<MessageDto> Messages { get; set; }
}
