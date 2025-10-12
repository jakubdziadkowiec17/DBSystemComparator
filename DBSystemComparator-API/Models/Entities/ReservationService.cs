using System.ComponentModel.DataAnnotations;

namespace DBSystemComparator_API.Models.Entities
{
    public class ReservationService
    {
        [Required] public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }
        [Required] public int ServiceId { get; set; }
        public Service Service { get; set; }
        [Required] public DateTime CreationDate { get; set; }
    }
}