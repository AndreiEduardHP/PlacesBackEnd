using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Places.Models
{
    public class Friend
    {
      
        public int Id { get; set; }

        
        public int UserId { get; set; }
        public UserProfile User { get; set; }

        
        public int FriendId { get; set; }
        public UserProfile FriendUser { get; set; }
    }
}
