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
        Task<List<Dictionary<string, object>>> ReadReservationsAfterSecondHalf2025Async();
        Task<List<Dictionary<string, object>>> ReadReservationsWithPaymentsAboveAsync(int minSum);
        Task<List<Dictionary<string, object>>> ReadClientsWithActiveReservationsAsync();
        Task<List<Dictionary<string, object>>> ReadActiveServicesUsedInReservationsAsync(int minSum);
        Task<List<Dictionary<string, object>>> ReadCapacityReservationsAsync(int capacityThreshold);
        // UPDATE
        Task<int> UpdateClientsAddressAndPhoneAsync(bool isActive, DateTime dateThreshold);
        Task<int> UpdateRoomsPriceForReservationsAsync(int minCapacity, int priceIncrement);
        Task<int> UpdateServicesPriceAsync(int priceIncrement, bool isActive, int price);
        Task<int> UpdatePriceForInactiveRoomsAsync(double discountMultiplier, int pricePerNight);
        Task<int> UpdateRoomsPriceForReservationsToApril2024Async(int priceDecrement);
        // DELETE
        Task<int> DeletePaymentsOlderThanMarch2024Async();
        Task<int> DeletePaymentsToSumAsync(int sum);
        Task<int> DeleteReservationsServicesOlderThanMarch2023Async();
        Task<int> DeleteReservationsServicesWithServicePriceBelowAsync(int price);
        Task<int> DeleteUnusedServicesPriceBelowAsync(int price);
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