namespace Places.Models
{
    public class UserProfileEvent
    {
        public int Id { get; set; }
        public DateTime JoinedTime { get; set; }
        public int UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }

        public bool HideUserInParticipantsList { get; set; }
        public bool? UserChecked { get; set; } 
        public string QRCode { get; set; }

    }
}
