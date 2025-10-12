using System.ComponentModel.DataAnnotations;

namespace DBSystemComparator_API.Models.Entities
{
    public class Reservation
    {
        [Key] public int Id { get; set; }
        [Required] public int ClientId { get; set; }
        public Client Client { get; set; }
        [Required] public int RoomId { get; set; }
        public Room Room { get; set; }
        [Required] public DateTime CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        [Required] public DateTime CreationDate { get; set; }
        public ICollection<Payment>? Payments { get; set; }
        public ICollection<ReservationService>? ReservationServices { get; set; }
    }
}