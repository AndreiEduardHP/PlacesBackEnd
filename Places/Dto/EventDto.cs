﻿using static Places.Models.Event;

namespace Places.Dto
{
    public class EventDto
    {
        public int Id { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public string? EventImage { get; set; }

        public string? QRCode { get; set; }

        public int CreatedByUserId {  get; set; }
        public DateTime EventTime { get; set; }

        public int EventParticipantsCount { get; set; }

        public int EventLocationId { get; set; }
        public double LocationLatitude { get; set; }
        public double LocationLongitude { get; set; }
        public int MaxParticipants { get; set; }
       
    }
}
