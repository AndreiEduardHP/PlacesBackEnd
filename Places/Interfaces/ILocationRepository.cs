using Places.Models;
using System.Collections;

namespace Places.Interfaces
{
    public interface ILocationRepository
    {
        ICollection<Location> GetLocations();
        Location GetLocation(int id);
        Location GetLocationByCoords(double latitude, double longitude);
        Location GetLocationByUserProfile(int userProfileId);
        ICollection<UserProfile> GetProfilesByLocation(int locationId);
        ICollection<UserProfile> GetOtherUserProfiles(int userProfileId);
        ICollection<Location> GetOtherUsersLocations(int userProfileId);
        bool LocationExists(int id);
        bool LocationExistsByCoord(double latitude, double longitude);
        bool CreateLocation(Location location);
        bool UpdateLocation(Location location);
        bool DeleteLocation(Location location);
        bool Save();
    }
}
