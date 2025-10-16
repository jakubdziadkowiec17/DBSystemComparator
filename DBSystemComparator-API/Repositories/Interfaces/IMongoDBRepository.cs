using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Repositories.Interfaces
{
    public interface IMongoDBRepository
    {
        Task<TablesCountDTO> GetTablesCountAsync();
        // CREATE
        Task CreateClientAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive);
        Task CreateRoomAsync(int number, int capacity, int pricePerNight, bool isActive);
        Task CreateServiceAsync(string name, int price, bool isActive);
        Task CreateReservationAsync(string clientId, string roomId, DateTime checkIn, DateTime checkOut, DateTime creationDate);
        Task CreateReservationServiceAsync(string reservationId, string serviceId, DateTime creationDate);
        Task CreatePaymentAsync(string reservationId, string description, int sum, DateTime creationDate);
        // READ
        Task<List<Dictionary<string, object>>> ReadClientsWithRoomsAsync(bool isActive);
        Task<List<Dictionary<string, object>>> ReadRoomsWithReservationCountAsync();
        Task<List<Dictionary<string, object>>> ReadServicesUsageAsync();
        Task<List<Dictionary<string, object>>> ReadPaymentsAboveAsync(int minSum);
        Task<List<Dictionary<string, object>>> ReadReservationsWithServicesAsync(bool clientActive, bool serviceActive);
        // UPDATE
        Task<long> UpdateClientsAddressPhoneAsync(bool isActive);
        Task<long> UpdateRoomsPriceJoinReservationsAsync(int minCapacity);
        Task<long> UpdateServicesPriceAsync(bool isActive);
        Task<long> UpdateRoomsPriceInactiveAsync();
        Task<long> UpdateRoomsPriceFutureReservationsAsync();
        // DELETE
        Task<long> DeleteReservationsSmallRoomsAsync(int capacityThreshold);
        Task<long> DeleteReservationsServicesFutureAsync(int topRows);
        Task<long> DeleteReservationsWithoutPaymentsAsync();
        Task<long> DeleteInactiveClientsWithoutReservationsAsync();
        Task<long> DeleteRoomsWithoutReservationsAsync();
        Task DeleteAllClientsAsync();
        Task DeleteAllRoomsAsync();
        Task DeleteAllReservationsAsync();
        Task DeleteAllReservationsServicesAsync();
        Task DeleteAllPaymentsAsync();
        Task DeleteAllServicesAsync();
    }
}