namespace DBSystemComparator_API.Models.DTOs
{
    public class CassandraClientDTO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class CassandraActiveClientDTO
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class CassandraRoomDTO
    {
        public Guid Id { get; set; }
        public int Number { get; set; }
        public int Capacity { get; set; }
        public int PricePerNight { get; set; }
        public bool IsActive { get; set; }
    }

    public class CassandraActiveRoomDTO
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
        public int Number { get; set; }
        public int Capacity { get; set; }
        public int PricePerNight { get; set; }
    }

    public class CassandraServiceDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public bool IsActive { get; set; }
    }

    public class CassandraActiveServiceDTO
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
        public int Price { get; set; }
        public string Name { get; set; }
    }

    public class CassandraReservationByClientDTO
    {
        public Guid ClientId { get; set; }
        public DateTime CreationDate { get; set; }
        public Guid ReservationId { get; set; }
        public Guid RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }

    public class CassandraReservationByRoomDTO
    {
        public Guid RoomId { get; set; }
        public DateTime CreationDate { get; set; }
        public Guid ReservationId { get; set; }
        public Guid ClientId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }

    public class CassandraPaymentDTO
    {
        public Guid ReservationId { get; set; }
        public DateTime CreationDate { get; set; }
        public Guid PaymentId { get; set; }
        public string Description { get; set; }
        public int Sum { get; set; }
    }

    public class CassandraReservationServiceByReservationDTO
    {
        public Guid ReservationId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class CassandraReservationServiceByServiceDTO
    {
        public Guid ServiceId { get; set; }
        public Guid ReservationId { get; set; }
        public DateTime CreationDate { get; set; }
    }
}