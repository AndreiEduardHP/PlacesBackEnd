namespace Places.Dto
{
    public class UpdateEventDto
    {
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public int MaxParticipants { get; set; }
        public string OtherRelevantInformation {  get; set; }

        public string EventImage { get; set; }
    }
}
