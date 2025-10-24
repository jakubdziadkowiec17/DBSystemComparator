namespace DBSystemComparator_API.Models.DTOs
{
    public class ClientDTO 
    {
        public int Id;
        public string FirstName;
        public string SecondName;
        public string LastName;
        public string Email;
        public DateTime BirthDate;
        public string Address;
        public string PhoneNumber;
        public bool IsActive;
    }
    
    public class RoomDTO
    {
        public int Id;
        public int RoomNumber;
        public int Floor;
        public double Price;
        public bool IsAvailable;
    }
    
    public class ServiceDTO
    {
        public int Id;
        public string Name;
        public int Price;
        public bool IsAvailable;
    }
    
    public class ReservationDTO
    {
        public int Id;
        public int ClientId;
        public int RoomId;
        public DateTime CheckInDate;
        public DateTime CheckOutDate;
        public DateTime CreationDate;
    }
    
    public class ReservationServiceDTO 
    {
        public int ReservationId;
        public int ServiceId;
        public DateTime CreationDate; 
    }

    public class PaymentDTO 
    { 
        public int Id;
        public int ReservationId;
        public string Description;
        public int Amount;
        public DateTime PaymentDate;
    }
}