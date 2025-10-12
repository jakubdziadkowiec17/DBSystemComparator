using System.ComponentModel.DataAnnotations;

namespace DBSystemComparator_API.Models.Entities
{
    public class Payment
    {
        [Key] public int Id { get; set; }
        [Required] public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }
        public string? Description { get; set; }
        [Required] [Range(0, 999999)] public int Sum { get; set; }
        [Required] public DateTime CreationDate { get; set; }
    }
}