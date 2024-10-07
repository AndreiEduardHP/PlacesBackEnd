using System.ComponentModel.DataAnnotations.Schema;

namespace Places.Models
{
    public class EventAlbumImage
    {
        public int Id { get; set; }

        [Column(TypeName = "VARCHAR(MAX)")]
        public string ImageUrl { get; set; }  // Poza va fi stocată ca string

        // Foreign Key către modelul Event
        public int EventId { get; set; }
        public Event Event { get; set; }  // Relația cu modelul Event
    }
}
