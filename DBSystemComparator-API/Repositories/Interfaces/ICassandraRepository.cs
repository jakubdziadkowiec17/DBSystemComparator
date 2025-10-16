using Cassandra;
using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Repositories.Interfaces
{
    public interface ICassandraRepository
    {
        Task<TablesCountDTO> GetTablesCountAsync();
        // CREATE METHODS
        Task CreateClientAsync(Guid id, string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive);
        Task CreateRoomAsync(Guid id, int number, int capacity, int pricePerNight, bool isActive);
        Task CreateServiceAsync(Guid id, string name, int price, bool isActive);
        Task CreateReservationAsync(Guid id, Guid clientId, Guid roomId, DateTime checkIn, DateTime checkOut, DateTime creationDate);
        Task CreateReservationServiceAsync(Guid reservationId, Guid serviceId, DateTime creationDate);
        Task CreatePaymentAsync(Guid id, Guid reservationId, string description, int sum, DateTime creationDate);
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