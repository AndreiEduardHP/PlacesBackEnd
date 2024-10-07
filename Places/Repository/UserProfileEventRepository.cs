using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Places.Data;
using Places.Dto;
using Places.Interfaces;
using Places.Models;

namespace Places.Repository
{
    public class UserProfileEventRepository : IUserProfileEventRepository
    {
        private readonly PlacesContext _context;
        public UserProfileEventRepository(PlacesContext context)
        {
            _context = context;
        }
        bool IUserProfileEventRepository.JoinEvent(UserProfileEvent userProfileEvent)
        {
            try
            {
                _context.UserProfileEvents.Add(userProfileEvent);
                Console.WriteLine($"UserChecked before SaveChanges: {userProfileEvent.UserChecked}");
                _context.SaveChanges();

                return _context.UserProfileEvents.Any(
                    x => x.UserProfileId == userProfileEvent.UserProfileId &&
                    x.EventId == userProfileEvent.EventId);

            }
            catch(Exception e) {
                Console.WriteLine(e.Message);
                return false; 
            }

        }

        public string GetQRCodeForUserEvent(int userId, int eventId)
        {
            var userProfileEvent = _context.UserProfileEvents
                .FirstOrDefault(upe => upe.UserProfileId == userId && upe.EventId == eventId);

            return userProfileEvent?.QRCode; // Assuming QRCode is stored here
        }
        public async Task<List<Event>> GetJoinedEventsByUserId(int userId)
        {
            return await _context.UserProfileEvents
                                 .Where(upe => upe.UserProfileId == userId)
                                 .Include(upe => upe.Event.EventLocation)
                                 .Select(upe => upe.Event)
                                 
                                 .ToListAsync();
        }

        bool IUserProfileEventRepository.UnJoinEvent(int eventId, int userId)
        {
            try
            {
                // Find the event user wants to unjoin by both eventId and userId
                var userProfileEvent = _context.UserProfileEvents
                    .FirstOrDefault(upe => upe.EventId == eventId && upe.UserProfileId == userId);

                // If no matching event found, nothing to delete
                if (userProfileEvent == null)
                {
                    return false;
                }

                // Remove the found entity from the context
                _context.UserProfileEvents.Remove(userProfileEvent);

                // Save changes to the database
                int rowsAffected = _context.SaveChanges();

                // If at least one row was affected, the delete was successful
                return rowsAffected > 0;
            }
            catch (Exception e)
            {
                // Log the exception details
                Console.WriteLine(e.Message);
                return false;
            }
        }

        
        bool IUserProfileEventRepository.CheckIfUserJoined(int eventId, int userId)
        {

            var userJoined = _context.UserProfileEvents
           .Any(upe => upe.EventId == eventId && upe.UserProfileId == userId);

         
           
         

            return userJoined;

        }







        public async Task<(IEnumerable<UserProfileWithFriendStatusDto>, int)> GetUserProfilesByEventId(int eventId, int userId)
        {
            // Fetch the user profiles for users who have joined the specific event.
            var userProfiles = await _context.UserProfileEvents
                .Where(upe => upe.EventId == eventId && !upe.HideUserInParticipantsList)
                .Select(upe => upe.UserProfile)
                .ToListAsync();

            // Initialize a list to hold UserProfiles along with their friend status
            var userProfileWithStatuses = new List<UserProfileWithFriendStatusDto>();

            foreach (var userProfile in userProfiles)
            {
                // Get the friend request status for the current user and the other user profile
                var friendStatus = await GetFriendRequestStatus(userId, userProfile.Id);

                // Create a new UserProfileWithFriendStatus object and add it to the list
                userProfileWithStatuses.Add(new UserProfileWithFriendStatusDto
                {
                    UserProfile = userProfile,
                    FriendStatus = friendStatus
                });
            }

            // Count the number of users
            int totalCount = await _context.UserProfileEvents
                .Where(upe => upe.EventId == eventId)
                .CountAsync();

            // Return both the list of user profiles with statuses and the count
            return (userProfileWithStatuses, totalCount);
        }
        public async Task<string> GetFriendRequestStatus(int currentUserId, int otherUserId)
        {
            var status = await _context.FriendsRequest
                .Where(fr => (fr.SenderId == currentUserId && fr.ReceiverId == otherUserId) ||
                             (fr.SenderId == otherUserId && fr.ReceiverId == currentUserId))
                .Select(fr => fr.Status)
                .FirstOrDefaultAsync();

            return status;
        }

        public async Task<List<Event>> GetMyEventsByUserId(int userId)
        {
            return await _context.Events
                                 .Where(upe => upe.CreatedByUserId == userId && upe.IsDeleted != true)
                                 .Include(upe => upe.EventLocation)
                              

                                 .ToListAsync();
        }

        public bool ScanQr(int eventId, int userId)
        {

            var userJoined = _context.UserProfileEvents
          .Any(upe => upe.EventId == eventId && upe.UserProfileId == userId);





          
            var eventHasCheckFunctionality = _context.Events
                                            .Where(e => e.Id == eventId)
                                            .Select(e => e.CheckFunctionality)
                                            .FirstOrDefault();

            if (eventHasCheckFunctionality != null)
            {

                var userEvent = _context.UserProfileEvents
                    .FirstOrDefault(upe => upe.EventId == eventId && upe.UserProfileId == userId);

                if (userEvent != null)
                {
                    userEvent.UserChecked = true;
                    _context.SaveChanges();
                }
            }
            return userJoined;
        }
    }
}
