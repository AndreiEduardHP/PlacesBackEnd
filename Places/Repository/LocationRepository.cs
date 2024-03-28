using Places.Data;
using Places.Interfaces;
using Places.Models;

namespace Places.Repository
{
    public class LocationRepository : ILocationRepository
    {
        private readonly PlacesContext _context;
        public LocationRepository(PlacesContext context)
        {
            _context = context;
        }
        public Location GetLocation(int id)
        {
            return _context.Location.Where(l => l.Id == id).FirstOrDefault();
        }

        public Location GetLocationByCoords(double latitude, double longitude)
        {
            return _context.Location.Where(l => l.latitude == latitude && l.longitude == longitude).FirstOrDefault();
        }

        public ICollection<Location> GetLocations()
        {
            return _context.Location.OrderBy(l=>l.Id).ToList();
        }

        public ICollection<UserProfile> GetProfilesByLocation(int locationId)
        {
            return _context.UserProfile.Where(l => l.UserLocation.Id == locationId).ToList();
        }

        public bool LocationExists(int id)
        {
            return _context.Location.Any(l => l.Id == id);
        }
        public bool CreateLocation(Location location)
        {
            //Change Tracker
            _context.Location.Add(location);
            return Save();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool UpdateLocation(Location location)
        {
            _context.Location.Update(location);
            return Save();
        }

        public bool DeleteLocation(Location location)
        {
            _context.Location.Remove(location);
            return Save();
        }

        public bool LocationExistsByCoord(double latitude, double longitude)
        {
            return _context.Location.Any(l => l.latitude == latitude && l.longitude == longitude);
        }

        public Location GetLocationByUserProfile(int userProfileId)
        {
            return _context.UserProfile.Where(up => up.Id == userProfileId).Select(l => l.UserLocation).FirstOrDefault();
        }

        public ICollection<UserProfile> GetOtherUserProfiles(int userProfileId)
        {
            return _context.UserProfile.Where(up => up.Id != userProfileId).ToList();
        }

        public ICollection<Location> GetOtherUsersLocations(int userProfileId)
        {
            return _context.UserProfile.Where(up => up.Id != userProfileId).Select(l => l.UserLocation).ToList();
        }
    }
}
