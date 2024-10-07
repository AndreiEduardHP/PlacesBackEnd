using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Places.Models
{
    public class FriendRequest
    {
        
        public int Id { get; set; }

        public int ReceiverRequestId {  get; set; }
        public int SenderId { get; set; }
        public UserProfile Sender { get; set; }

        
        public int ReceiverId { get; set; }
        public UserProfile Receiver { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        // Status can be "Pending", "Accepted", "Declined"
        public string Status { get; set; } = "Pending";
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}
