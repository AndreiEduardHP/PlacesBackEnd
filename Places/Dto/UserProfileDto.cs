namespace Places.Dto
{
    public class UserProfileDto
    {
        public int? Id { get; set; }
        public string? FirstName { get; set; }
        public DateTime DateAccountCreation { get; set; }
        public string? LanguagePreference { get; set; }
        public string? ThemeColor { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        public string? NotificationToken { get; set; }

        public string? Country { get; set; }

        public int? Credit {  get; set; }
        public string? City { get; set; }
        public string? Interest { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public int? CurrentLocationId { get; set; }
        public string? FriendRequestStatus { get; set; }
        public bool? AreFriends { get; set; }
    }
}
