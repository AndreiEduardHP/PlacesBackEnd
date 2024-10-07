using Places.Models;

namespace Places.Dto
{
    public class UserProfileEventDto
    {
        public UserProfileEventDto()
        {
            JoinedTime = DateTime.Now; 
           
        }
        public DateTime JoinedTime { get; set; }
        public int UserProfileId { get; set; }
        public int EventId { get; set; }

        public bool HideUserInParticipantsList { get; set; }
        public bool UserChecked { get; set; }

        public string? QRCode { get; set; }
    }
   
}
