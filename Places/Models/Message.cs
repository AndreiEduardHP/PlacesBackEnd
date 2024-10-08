﻿using System;

namespace Places.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int SenderId { get; set; } 
        public int ChatId { get; set; } 
        public DateTime Timestamp { get; set; }

        public bool IsRead { get; set; }
        public Chat Chat { get; set; }
    }
}
