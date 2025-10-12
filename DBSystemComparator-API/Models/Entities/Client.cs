using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace DBSystemComparator_API.Models.Entities
{
    public class Client
    {
        [Key] public int Id { get; set; }
        [Required] [StringLength(30)] public string FirstName { get; set; }
        [StringLength(30)] public string? SecondName { get; set; }
        [Required] [StringLength(30)] public string LastName { get; set; }
        [Required] [StringLength(1000)] public string Email { get; set; }
        [Required] public DateTime DateOfBirth { get; set; }
        [Required] [StringLength(200)] public string Address { get; set; }
        [Required] [Range(100000000, 999999999)] public int PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public ICollection<Reservation>? Reservations { get; set; }
    }
}