using System.ComponentModel.DataAnnotations.Schema;

namespace Places.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string Interest { get; set; }
        public string Country { get; set; }

        public string? NotificationToken { get; set; }  

        public DateTime DateAccountCreation { get; set; }

        public string? LanguagePreference { get; set; }
        public string? ThemeColor { get; set; }

        public int? Credit { get; set; }

        [Column(TypeName = "VARBINARY(MAX)")]
        public byte[]? ProfilePicture { get; set; }

        public int CurrentLocationId { get; set; }

        [ForeignKey("CurrentLocationId")]
        public Location UserLocation { get; set; }

        public ICollection<Friend> Friends { get; set; }

        // Navigation property for FriendRequest relationship (sent requests)
        public ICollection<FriendRequest> SentFriendRequests { get; set; }

        // Navigation property for FriendRequest relationship (received requests)
        public ICollection<FriendRequest> ReceivedFriendRequests { get; set; }

    }
}
