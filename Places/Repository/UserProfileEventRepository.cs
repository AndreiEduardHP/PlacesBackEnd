using Microsoft.EntityFrameworkCore;
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

        public (IEnumerable<UserProfile>, int) GetUserProfilesByEventId(int eventId)
        {
            // Fetch the user profiles for users who have joined the specific event.
            var userProfiles = _context.UserProfileEvents
                .Where(upe => upe.EventId == eventId && !upe.HideUserInParticipantsList) // Check HideUserInParticipantsList here
                .Select(upe => upe.UserProfile)
                .ToList();

            // Count the number of users
            int totalCount = _context.UserProfileEvents
       .Where(upe => upe.EventId == eventId)
       .Count();

            // Return both the list of user profiles and the count
            return (userProfiles, totalCount);
        }
    }
}
