using System.ComponentModel.DataAnnotations;

namespace DBSystemComparator_API.Models.Entities
{
    public class Service
    {
        [Key] public int Id { get; set; }
        [Required] [StringLength(50)] public string Name { get; set; }
        [Required] [Range(0, 999999)] public int Price { get; set; }
        public bool IsActive { get; set; }
        public ICollection<ReservationService>? ReservationServices { get; set; }
    }
}