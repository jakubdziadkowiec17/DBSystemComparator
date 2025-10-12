namespace DBSystemComparator_API.Models.DTOs
{
    public class TablesCountDTO
    {
        public int ClientsCount { get; set; }
        public int RoomsCount { get; set; }
        public int ReservationsCount { get; set; }
        public int PaymentsCount { get; set; }
        public int ServicesCount { get; set; }
        public int ReservationServicesCount { get; set; }
    }
}