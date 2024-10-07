using Places.Dto;
using Places.Models;

namespace Places.Interfaces
{
    public interface IUserProfileRepository
    {
        ICollection<UserProfile> GetUserProfiles();
        UserProfile GetUserProfile(int id);
        UserProfile GetUserProfileByPhone(string phoneNumber);
        //ICollection<UserProfile> GetConnectionsOfAUser(int userProfileId);
        bool UserProfileExists(int id);
        bool CreateUserProfile(UserProfile userProfile);
        Task<bool> AreFriends(int userId, int friendId);
        bool UpdateUserProfile(UserProfile userProfile);
        bool DeleteUserProfile(UserProfile userProfile);
        bool Save();
        bool CheckPhoneNumberExists(string phoneNumber);
        bool CheckEmailNumberExists(string email);

        Task SendFriendRequest(int senderId, int receiverId, float latitude, float longitude);

        Task<UserDataAwardsDto> CheckUserDataAwards(int userId);

        Task<List<FriendRequestDto>> GetPendingFriendRequests(int userId ); 
             Task<List<object>> GetAcceptedFriendRequests(int userId);
        Task<bool> UpdateUserPreferences(int userId, UserProfileDto preferences);

        Task AcceptFriendRequest(int requestId);
        Task DeclineFriendRequest(int requestId);
        Task DeleteFriend(int userId1, int userId2);
        Task<string> GetFriendRequestStatus(int currentUserId, int otherUserId); // Add this method
        Task<int> GetFriendCount(int userId);
    }
}
