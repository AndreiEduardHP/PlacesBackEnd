using Places.Models;

namespace Places.Interfaces
{
    public interface IUserProfileEventRepository
    {
        bool JoinEvent (UserProfileEvent userProfileEvent);
        bool UnJoinEvent (int eventId, int userId);

        string GetQRCodeForUserEvent(int userId, int eventId);

        (IEnumerable<UserProfile>, int) GetUserProfilesByEventId(int eventId);
        bool CheckIfUserJoined(int eventId, int userId);

        Task<List<Event>> GetJoinedEventsByUserId(int userId);
    }
}
