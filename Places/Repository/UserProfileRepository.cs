using Microsoft.EntityFrameworkCore;
using Places.Data;
using Places.Dto;
using Places.Interfaces;
using Places.Models;

namespace Places.Repository
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly PlacesContext _context;
        public UserProfileRepository(PlacesContext context)
        {
            _context = context;
        }

        public UserProfile GetUserProfile(int id)
        {
            return _context.UserProfile.Where(up => up.Id == id).FirstOrDefault();
        }

        public UserProfile GetUserProfileByPhone(string phoneNumber)
        {
            return _context.UserProfile.Where(up => up.PhoneNumber == phoneNumber).FirstOrDefault();
        }

        public ICollection<UserProfile> GetUserProfiles()
        {
            return _context.UserProfile.OrderBy(up => up.Id).ToList();
        }

        public bool UserProfileExists(int id)
        {
            return _context.UserProfile.Any(up => up.Id == id);
        }

        public bool CreateUserProfile(UserProfile userProfile)
        {
            
            _context.UserProfile.Add(userProfile);

            return Save();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool UpdateUserProfile(UserProfile userProfile)
        {
            _context.UserProfile.Update(userProfile);
            
            return Save();
        }

        public bool DeleteUserProfile(UserProfile userProfile)
        {
            _context.UserProfile.Remove(userProfile);
            return Save();
        }

        public bool CheckPhoneNumberExists(string phoneNumber)
        {
            return _context.UserProfile.Any(up => up.PhoneNumber == phoneNumber);
        }

        public async Task SendFriendRequest(int senderId, int receiverId)
        {

            var existingRequest = await _context.FriendsRequest
       .FirstOrDefaultAsync(r => r.SenderId == senderId && r.ReceiverId == receiverId && r.Status == "Pending");

            if (existingRequest != null)
            {

                return ;
            }

            var friendRequest = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                RequestDate = DateTime.Now,
                Status = "Pending"
            };

            _context.FriendsRequest.Add(friendRequest);
            await _context.SaveChangesAsync();
        }


        public async Task<List<FriendRequestDto>> GetPendingFriendRequests(int userId)
        {
            var pendingRequests = await _context.FriendsRequest
                .Where(fr => fr.ReceiverId == userId && fr.Status == "Pending")
                .Select(fr => new FriendRequestDto
                {
                    RequestId = fr.Id,
                    SenderName = fr.Sender.FirstName + " " + fr.Sender.LastName, // Concatenate first and last name
                    RequestDate = fr.RequestDate,
                    SenderPicture = fr.Sender.ProfilePicture
                    // Add any other properties you need from the sender's UserProfile
                })
                .ToListAsync();

            return pendingRequests;
        }

        public async Task AcceptFriendRequest(int requestId)
        {

            try
            {
                var request = await _context.FriendsRequest.FindAsync(requestId);
                if (request == null) throw new Exception("Friend request not found.");

                request.Status = "Accepted";
                await _context.SaveChangesAsync();

                var friend = new Friend
                {
                    UserId = request.SenderId, 
                    FriendId = request.ReceiverId 
                };

                _context.Friends.Add(friend);

                var friendReverse = new Friend
                {
                    UserId = request.ReceiverId,
                    FriendId = request.SenderId
                };

                _context.Friends.Add(friend);

                var chat = new Chat
                {
                    User1Id = friend.UserId,
                    User2Id = friend.FriendId
                };

                _context.Chats.Add(chat);

                await _context.SaveChangesAsync();

               
               
            }
            catch (Exception e)
            {
                
                throw e;
            }
           
        }

        public async Task DeclineFriendRequest(int requestId)
        {
            var request = await _context.FriendsRequest.FindAsync(requestId);
            if (request == null) throw new Exception("Friend request not found.");

            request.Status = "Declined";
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetFriendCount(int userId)
        {
            // Assuming bidirectional friendships, count where the user is either the user or the friend
            var count = await _context.Friends
                .CountAsync(f => f.UserId == userId );

            return count;
        }

        public async Task<bool> AreFriends(int userId, int friendId)
        {
            return await _context.Friends.AnyAsync(f => f.UserId == userId && f.FriendId == friendId);
        }

        public Task<string> GetFriendRequestStatus(int currentUserId, int otherUserId)
        {
            var status =  _context.FriendsRequest
               .Where(fr => (fr.SenderId == currentUserId && fr.ReceiverId == otherUserId) ||
                            (fr.SenderId == otherUserId && fr.ReceiverId == currentUserId))
               .Select(fr => fr.Status)
               .FirstOrDefaultAsync();

            return status;
        }

        public async Task<bool> UpdateUserPreferences(int userId, UserProfileDto preferences)
        {
            var userProfile = await _context.UserProfile.FirstOrDefaultAsync(up => up.Id == userId);
            if (userProfile == null) return false;

            if (preferences.LanguagePreference != null)
            {
                userProfile.LanguagePreference = preferences.LanguagePreference;
            }
            if (preferences.ThemeColor != null)
            {
                userProfile.ThemeColor = preferences.ThemeColor;
            }

            return Save(); // You may need to adjust this if your Save method is async
        }


    }
}
