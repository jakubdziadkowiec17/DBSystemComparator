using Cassandra;
using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Repositories.Interfaces
{
    public interface ICassandraRepository
    {
        Task<TablesCountDTO> GetTablesCountAsync();
        // CREATE METHODS
        Task<Guid> CreateClientAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive);
        Task<Guid> CreateRoomAsync(int number, int capacity, int pricePerNight, bool isActive);
        Task<Guid> CreateServiceAsync(string name, int price, bool isActive);
        Task<Guid> CreateReservationAsync(Guid clientId, Guid roomId, DateTime checkIn, DateTime checkOut, DateTime creationDate);
        Task CreateReservationServiceAsync(Guid reservationId, Guid serviceId, DateTime creationDate);
        Task<Guid> CreatePaymentAsync(Guid reservationId, string description, int sum, DateTime creationDate);
        // READ METHODS
        Task<List<Dictionary<string, object>>> ReadClientsWithRoomsAsync(bool isActive);
        Task<List<Dictionary<string, object>>> ReadRoomsWithReservationCountAsync();
        Task<List<Dictionary<string, object>>> ReadServicesUsageAsync();
        Task<List<Dictionary<string, object>>> ReadPaymentsAboveAsync(int minSum);
        Task<List<Dictionary<string, object>>> ReadReservationsWithServicesAsync(bool clientActive, bool serviceActive);
        // UPDATE METHODS
        Task UpdateClientsAddressPhoneAsync(bool isActive);
        Task UpdateRoomsPriceJoinReservationsAsync(int minCapacity);
        Task UpdateServicesPriceAsync(bool isActive);
        Task UpdateRoomsPriceInactiveAsync();
        Task UpdateRoomsPriceFutureReservationsAsync();
        // DELETE
        Task DeleteClientAsync(Guid clientId);
        Task DeleteRoomAsync(Guid roomId);
        Task DeleteReservationAsync(Guid reservationId);
        Task DeleteReservationsSmallRoomsAsync(int capacityThreshold);
        Task DeleteReservationsServicesFutureAsync(int topRows);
        Task DeleteReservationsWithoutPaymentsAsync();
        Task DeleteInactiveClientsWithoutReservationsAsync();
        Task DeleteRoomsWithoutReservationsAsync();
        Task DeleteAllClientsAsync();
        Task DeleteAllRoomsAsync();
        Task DeleteAllReservationsAsync();
        Task DeleteAllReservationsServicesAsync();
        Task DeleteAllPaymentsAsync();
        Task DeleteAllServicesAsync();
    }
}