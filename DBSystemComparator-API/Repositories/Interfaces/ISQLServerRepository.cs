using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Repositories.Interfaces
{
    public interface ISQLServerRepository
    {
        // CREATE
        Task<int> CreateClientAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive);
        Task<int> CreateRoomAsync(int number, int capacity, int pricePerNight, bool isActive);
        Task<int> CreateServiceAsync(string name, int price, bool isActive);
        Task<List<int>> CreateClientsAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive, int count);
        Task<List<int>> CreateRoomsAsync(int number, int capacity, int pricePerNight, bool isActive, int count);
        // READ
        Task<List<Dictionary<string, object>>> ReadReservationsAfter2024Async();
        Task<List<Dictionary<string, object>>> ReadReservationsWithPaymentsAboveAsync(int minSum);
        Task<List<Dictionary<string, object>>> ReadClientsWithActiveReservationsAsync();
        Task<List<Dictionary<string, object>>> ReadActiveServicesUsedInReservationsAsync();
        Task<List<Dictionary<string, object>>> ReadCapacityReservationsAsync(int capacityThreshold);
        // UPDATE
        Task<int> UpdateClientsAddressAndPhoneAsync(bool isActive);
        Task<int> UpdateRoomsPriceForReservationsAsync(int minCapacity, int priceIncrement);
        Task<int> UpdateServicesPriceAsync(int priceIncrement, bool isActive);
        Task<int> UpdatePriceForInactiveRoomsAsync(double discountMultiplier);
        Task<int> UpdateRoomsPriceForReservationsTo2024Async(int priceDecrement);
        // DELETE
        Task<int> DeletePaymentsOlderThan2024Async();
        Task<int> DeleteReservationsWithoutPaymentAsync();
        Task<int> DeleteReservationsServicesOlderThan2024Async();
        Task<int> DeleteReservationsServicesWithServicePriceBelowAsync(int price);
        Task<int> DeleteUnusedServicesAsync();
        // HELPERS
        Task CreateClientsBatchAsync(IEnumerable<(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive)> clients);
        Task CreateRoomsBatchAsync(List<(int number, int capacity, int pricePerNight, bool isActive)> rooms);
        Task CreateServicesBatchAsync(List<(string name, int price, bool isActive)> services);
        Task CreateReservationsBatchAsync(List<(int clientId, int roomId, DateTime checkIn, DateTime checkOut, DateTime creationDate)> reservations);
        Task CreatePaymentsBatchAsync(List<(int reservationId, string description, int sum, DateTime creationDate)> payments);
        Task CreateReservationsServicesBatchAsync(List<(int reservationId, int serviceId, DateTime creationDate)> resServices);
        Task<TablesCountDTO> GetTablesCountAsync();
        Task<List<int>> GetAllClientIdsAsync();
        Task<List<int>> GetAllRoomIdsAsync();
        Task<List<int>> GetAllServiceIdsAsync();
        Task<List<int>> GetAllReservationIdsAsync();
        Task<int> DeleteAllClientsAsync();
        Task<int> DeleteAllRoomsAsync();
        Task<int> DeleteAllReservationsAsync();
        Task<int> DeleteAllReservationsServicesAsync();
        Task<int> DeleteAllPaymentsAsync();
        Task<int> DeleteAllServicesAsync();
    }
}