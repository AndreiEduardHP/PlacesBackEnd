using System.ComponentModel.DataAnnotations.Schema;

namespace Places.Models
{
    public class Event
    {
     
        public int Id { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }

        public int  CreatedByUserId {  get; set; }

        [Column(TypeName = "VARCHAR(MAX)")]
        public string? EventImage { get; set; }

        public DateTime EventTime { get; set; }
        public int MaxParticipants { get; set; }
       

  
        public int EventLocationId { get; set; }
        public Location EventLocation { get; set; }

  

      
    }
}
