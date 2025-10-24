using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Repositories.Interfaces
{
    public interface ICassandraRepository
    {

    // CREATE
    Task<Guid> CreateClientAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive);
    Task<Guid> CreateRoomAsync(int number, int capacity, double pricePerNight, bool isActive);
    Task<Guid> CreateServiceAsync(string name, int price, bool isActive);
    Task<List<Guid>> CreateClientsAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive, int count);
    Task<List<Guid>> CreateRoomsAsync(int number, int capacity, double pricePerNight, bool isActive, int count);

    // READ
    Task<List<Dictionary<string, object>>> ReadReservationsAfterSecondHalf2025Async();
    Task<List<Dictionary<string, object>>> ReadReservationsWithPaymentsAboveAsync(int minSum);
    Task<List<Dictionary<string, object>>> ReadClientsWithActiveReservationsAsync();
    Task<List<Dictionary<string, object>>> ReadActiveServicesUsedInReservationsAsync(int minSum);
    Task<List<Dictionary<string, object>>> ReadCapacityReservationsAsync(int capacityThreshold);

    // UPDATE
    Task<int> UpdateClientsAddressAndPhoneAsync(bool isActive, DateTime dateThreshold);
    Task<int> UpdateRoomsPriceForReservationsAsync(int minCapacity, int priceIncrement);
    Task<int> UpdateServicesPriceAsync(int priceIncrement, bool isActive, int price);
    Task<int> UpdatePriceForInactiveRoomsAsync(double discountMultiplier, double pricePerNight);
    Task<int> UpdateRoomsPriceForReservationsToApril2024Async(int priceDecrement);

    // DELETE
    Task<int> DeletePaymentsOlderThanMarch2024Async();
    Task<int> DeletePaymentsToSumAsync(int sum);
    Task<int> DeleteReservationsServicesOlderThanMarch2023Async();
    Task<int> DeleteReservationsServicesWithServicePriceBelowAsync(int price);
    Task<int> DeleteUnusedServicesPriceBelowAsync(int price);

        // HELPERS
        Task InsertClientsBatchAsync(IEnumerable<CassandraClientDTO> clients);
        Task InsertActiveClientsBatchAsync(IEnumerable<CassandraActiveClientDTO> clients);
        Task InsertRoomsBatchAsync(IEnumerable<CassandraRoomDTO> rooms);
        Task InsertActiveRoomsBatchAsync(IEnumerable<CassandraActiveRoomDTO> rooms);
        Task InsertServicesBatchAsync(IEnumerable<CassandraServiceDTO> services);
        Task InsertActiveServicesBatchAsync(IEnumerable<CassandraActiveServiceDTO> services);
        Task InsertReservationsByClientBatchAsync(IEnumerable<CassandraReservationByClientDTO> reservations);
        Task InsertReservationsByRoomBatchAsync(IEnumerable<CassandraReservationByRoomDTO> reservations);
        Task InsertPaymentsByReservationBatchAsync(IEnumerable<CassandraPaymentDTO> payments);
        Task InsertReservationServicesByReservationBatchAsync(IEnumerable<CassandraReservationServiceByReservationDTO> items);
        Task InsertReservationServicesByServiceBatchAsync(IEnumerable<CassandraReservationServiceByServiceDTO> items);
        Task<TablesCountDTO> GetTablesCountAsync();
        Task DeleteAllAsync();
    }
}