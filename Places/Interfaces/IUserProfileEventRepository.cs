using Places.Dto;
using Places.Models;

namespace Places.Interfaces
{
    public interface IUserProfileEventRepository
    {
        bool JoinEvent (UserProfileEvent userProfileEvent);
        bool UnJoinEvent (int eventId, int userId);

        string GetQRCodeForUserEvent(int userId, int eventId);

        Task<(IEnumerable<UserProfileWithFriendStatusDto>, int)> GetUserProfilesByEventId(int eventId, int userId);
        bool CheckIfUserJoined(int eventId, int userId);
        bool ScanQr(int eventId, int userId);

        Task<List<Event>> GetJoinedEventsByUserId(int userId);
        Task<List<Event>> GetMyEventsByUserId(int userId);
    }
}
