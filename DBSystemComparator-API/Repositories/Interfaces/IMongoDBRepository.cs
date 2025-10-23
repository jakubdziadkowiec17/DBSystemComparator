using DBSystemComparator_API.Models.Collections;
using DBSystemComparator_API.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DBSystemComparator_API.Repositories.Interfaces
{
    public interface IMongoDBRepository
    {
        // CREATE
        Task<string> CreateClientAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive);
        Task<string> CreateRoomAsync(int number, int capacity, int pricePerNight, bool isActive);
        Task<string> CreateServiceAsync(string name, int price, bool isActive);
        Task<List<string>> CreateClientsAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive, int count);
        Task<List<string>> CreateRoomsAsync(int number, int capacity, int pricePerNight, bool isActive, int count);
        // READ
        Task<List<Dictionary<string, object>>> ReadReservationsAfterSecondHalf2025Async();
        Task<List<Dictionary<string, object>>> ReadReservationsWithPaymentsAboveAsync(int minSum);
        Task<List<Dictionary<string, object>>> ReadClientsWithActiveReservationsAsync();
        Task<List<Dictionary<string, object>>> ReadActiveServicesUsedInReservationsAsync(int minSum);
        Task<List<Dictionary<string, object>>> ReadCapacityReservationsAsync(int capacityThreshold);
        // UPDATE
        Task<long> UpdateClientsAddressAndPhoneAsync(bool isActive, DateTime dateThreshold);
        Task<long> UpdateRoomsPriceForReservationsAsync(int minCapacity, int priceIncrement);
        Task<long> UpdateServicesPriceAsync(int priceIncrement, bool isActive, int price);
        Task<long> UpdatePriceForInactiveRoomsAsync(double discountMultiplier, int pricePerNight);
        Task<long> UpdateRoomsPriceForReservationsToApril2024Async(int priceDecrement);
        // DELETE
        Task<long> DeletePaymentsOlderThanMarch2024Async();
        Task<long> DeletePaymentsToSumAsync(int sum);
        Task<long> DeleteReservationsServicesOlderThanMarch2023Async();
        Task<long> DeleteReservationsServicesWithServicePriceBelowAsync(int price);
        Task<long> DeleteUnusedServicesPriceBelowAsync(int price);
        // HELPERS
        Task CreateClientsBatchAsync(IEnumerable<ClientCollection> clients);
        Task CreateRoomsBatchAsync(IEnumerable<RoomCollection> rooms);
        Task CreateServicesBatchAsync(IEnumerable<Models.Collections.ServiceCollection> services);
        Task CreateReservationsBatchAsync(IEnumerable<ReservationCollection> reservations);
        Task<TablesCountDTO> GetTablesCountAsync();
        Task<List<string>> GetAllClientIdsAsync();
        Task<List<string>> GetAllRoomIdsAsync();
        Task<List<string>> GetAllServiceIdsAsync();
        Task<List<string>> GetAllReservationIdsAsync();
        IMongoCollection<BsonDocument> GetReservationCollectionRaw();
        Task<long> DeleteAllClientsAsync();
        Task<long> DeleteAllRoomsAsync();
        Task<long> DeleteAllServicesAsync();
        Task<long> DeleteAllReservationsAsync();
    }
}