using Azure.Core;
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
        public bool CheckEmailNumberExists(string email)
        {
            return _context.UserProfile.Any(up => up.Email == email && up.EmailVerified == true);
        }

        public async Task SendFriendRequest(int senderId, int receiverId, float latitude, float longitude)
        {

            var existingRequest = await _context.FriendsRequest
       .FirstOrDefaultAsync(r => r.SenderId == senderId && r.ReceiverId == receiverId || r.SenderId == receiverId && r.ReceiverId == senderId);

            if (existingRequest != null)
            {
                existingRequest.ReceiverRequestId = receiverId;
                existingRequest.Status = "Pending";

                await _context.SaveChangesAsync();
            }
            else
            {
                var friendRequest = new FriendRequest
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Latitude = latitude,
                    Longitude = longitude,
                    RequestDate = DateTime.Now,
                    Status = "Pending"
                };

                _context.FriendsRequest.Add(friendRequest);
                await _context.SaveChangesAsync();
            } 
        }


        public async Task<List<FriendRequestDto>> GetPendingFriendRequests(int userId)
        {
            

            var pendingRequests = await _context.FriendsRequest
           .Where(fr => fr.ReceiverRequestId == userId && fr.Status == "Pending")

                .Select(fr => new FriendRequestDto
                {
                    
                    RequestId = fr.Id,
                    SenderName = fr.ReceiverId == userId
    ? fr.Sender.FirstName + " " + fr.Sender.LastName.Substring(0, 1)
    : fr.Receiver.FirstName + " " + fr.Receiver.LastName.Substring(0, 1),
            RequestDate = fr.RequestDate,
                    SenderPicture =  fr.ReceiverId == userId
    ? fr.Sender.ProfilePicture 
    : fr.Receiver.ProfilePicture,
                   
                })
                .ToListAsync();

            return pendingRequests;
        }
        public async Task<List<object>> GetAcceptedFriendRequests(int userId)
        {
            var acceptedRequests = await _context.FriendsRequest
                .Where(fr => (fr.ReceiverId == userId || fr.SenderId == userId) )
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .Select(fr => new 
                {
                    RequestId = fr.Id,
                    fr.ReceiverId,
                    OtherPersonId = fr.SenderId == userId ? fr.ReceiverId : fr.SenderId,
                    SenderName = fr.SenderId == userId
                        ? fr.Receiver.FirstName + " " + fr.Receiver.LastName.Substring(0, 1)
                        : fr.Sender.FirstName + " " + fr.Sender.LastName.Substring(0, 1),
                    fr.RequestDate,
                    SenderPicture = fr.SenderId == userId ? fr.Receiver.ProfilePicture : fr.Sender.ProfilePicture,
                     fr.Status,
                     fr.Latitude,
                     fr.Longitude,
                     Profile = fr.SenderId == userId
                        ? fr.Receiver 
                        : fr.Sender,
                })
                .ToListAsync();

            return acceptedRequests.Cast<object>().ToList();
        }

        public async Task AcceptFriendRequest(int requestId)
        {
            try
            {
                var request = await _context.FriendsRequest.FindAsync(requestId);
                if (request == null) throw new Exception("Friend request not found.");

                request.Status = "Accepted";
                request.ReceiverRequestId = 0;
                await _context.SaveChangesAsync();

                var friendRow = await _context.Friends
                .FirstOrDefaultAsync(f => (f.UserId == request.SenderId && f.FriendId == request.ReceiverId) ||
                                     (f.UserId == request.ReceiverId && f.FriendId == request.SenderId));
                if (friendRow != null)
                {
                    return;      
                }
                else
                {
                    var friend = new Friend
                    {
                        UserId = request.SenderId,
                        FriendId = request.ReceiverId
                    };
                    _context.Friends.Add(friend);
                    await _context.SaveChangesAsync();
                }

                var friendRequestChat = await _context.Chats
                    .FirstOrDefaultAsync(f => (f.User1Id == request.SenderId && f.User2Id == request.ReceiverId) ||
                                              (f.User1Id == request.ReceiverId && f.User2Id == request.SenderId));

                if (friendRequestChat != null)
                {
                    return;
                }
                else
                {
                    var chat = new Chat
                    {
                        User1Id = request.SenderId,
                        User2Id = request.ReceiverId
                    };
                    _context.Chats.Add(chat);
                    await _context.SaveChangesAsync();
                }

               
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
            request.ReceiverRequestId = 0;
            _context.FriendsRequest.Update(request);
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
            if (preferences.ProfileVisibility != null)
            {
                userProfile.ProfileVisibility = preferences.ProfileVisibility;
            }

            return Save(); // You may need to adjust this if your Save method is async
        }

        public async Task<UserDataAwardsDto> CheckUserDataAwards(int userId)
        {
            int createdEventsCount = await _context.Events
           .CountAsync(e => e.CreatedByUserId == userId);

        
            int participatedEventsCount = await _context.UserProfileEvents
                .CountAsync(ep => ep.UserProfileId == userId);

         
            var userDataAwards = new UserDataAwardsDto
            {
                CountCreatedEvents = createdEventsCount,
                CountJoinedEvents = participatedEventsCount
            };

            return userDataAwards;
        }

        public async Task DeleteFriend(int userId1, int userId2)
        {
            try
            {
                var friendRequest = _context.FriendsRequest.FirstOrDefault(f => (f.SenderId == userId1 && f.ReceiverId == userId2) || (f.SenderId == userId2 && f.ReceiverId == userId1));
                if (friendRequest != null)
                {
                    friendRequest.Status = "Deleted";
                    friendRequest.ReceiverRequestId = 0;
                    _context.FriendsRequest.Update(friendRequest);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
