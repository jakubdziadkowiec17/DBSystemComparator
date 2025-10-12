using System.ComponentModel.DataAnnotations;

namespace DBSystemComparator_API.Models.Entities
{
    public class Room
    {
        [Key] public int Id { get; set; }
        [Required][Range(0, 999999)] public int Number { get; set; }
        [Required] [Range(0, 99)] public int Capacity { get; set; }
        [Required] [Range(0, 999999)] public int PricePerNight { get; set; }
        public bool IsActive { get; set; }
        public ICollection<Reservation>? Reservations { get; set; }
    }
}