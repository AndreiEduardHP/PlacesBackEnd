using Places.Models;

namespace Places.Dto
{
    public class UserProfileWithFriendStatusDto
    {
        public UserProfile UserProfile { get; set; }
        public string FriendStatus { get; set; }
    }
}
