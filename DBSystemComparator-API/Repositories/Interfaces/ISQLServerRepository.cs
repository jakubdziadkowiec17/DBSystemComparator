using DBSystemComparator_API.Models.DTOs;
using Microsoft.Data.SqlClient;
using MongoDB.Driver.Core.Configuration;
using System.Data;

namespace DBSystemComparator_API.Repositories.Interfaces
{
    public interface ISQLServerRepository
    {
        Task<TablesCountDTO> GetTablesCountAsync();
        // CREATE
        Task<int> CreateClientAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive);
        Task<int> CreateRoomAsync(int number, int capacity, int pricePerNight, bool isActive);
        Task<int> CreateServiceAsync(string name, int price, bool isActive);
        Task<int> CreateReservationAsync(int clientId, int roomId, DateTime checkIn, DateTime checkOut, DateTime creationDate);
        Task<int> CreateReservationServiceAsync(int reservationId, int serviceId, DateTime creationDate);
        Task<int> CreatePaymentAsync(int reservationId, string description, int sum, DateTime creationDate);
        // READ
        Task<List<Dictionary<string, object>>> ReadClientsWithRoomsAsync(bool isActive);
        Task<List<Dictionary<string, object>>> ReadRoomsWithReservationCountAsync();
        Task<List<Dictionary<string, object>>> ReadServicesUsageAsync();
        Task<List<Dictionary<string, object>>> ReadPaymentsAboveAsync(int minSum);
        Task<List<Dictionary<string, object>>> ReadReservationsWithServicesAsync(bool clientActive, bool serviceActive);
        // UPDATE
        Task<int> UpdateClientsAddressPhoneAsync(bool isActive);
        Task<int> UpdateRoomsPriceJoinReservationsAsync(int minCapacity);
        Task<int> UpdateServicesPriceAsync(bool isActive);
        Task<int> UpdateRoomsPriceInactiveAsync();
        Task<int> UpdateRoomsPriceFutureReservationsAsync();
        // DELETE
        Task<int> DeleteReservationsSmallRoomsAsync(int capacityThreshold);
        Task<int> DeleteReservationsServicesFutureAsync(int topRows);
        Task<int> DeleteReservationsWithoutPaymentsAsync();
        Task<int> DeleteInactiveClientsWithoutReservationsAsync();
        Task<int> DeleteRoomsWithoutReservationsAsync();
        Task<int> DeleteAllClientsAsync();
        Task<int> DeleteAllRoomsAsync();
        Task<int> DeleteAllReservationsAsync();
        Task<int> DeleteAllReservationsServicesAsync();
        Task<int> DeleteAllPaymentsAsync();
        Task<int> DeleteAllServicesAsync();

        Task CreateClientsBatchAsync(IEnumerable<(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive)> clients);
        Task CreateRoomsBatchAsync(List<(int number, int capacity, int pricePerNight, bool isActive)> rooms);
        Task CreateServicesBatchAsync(List<(string name, int price, bool isActive)> services);
        Task CreateReservationsBatchAsync(List<(int clientId, int roomId, DateTime checkIn, DateTime checkOut, DateTime creationDate)> reservations);
        Task CreatePaymentsBatchAsync(List<(int reservationId, string description, int sum, DateTime creationDate)> payments);
        Task CreateReservationsServicesBatchAsync(List<(int reservationId, int serviceId, DateTime creationDate)> resServices);
        Task<List<int>> GetAllClientIdsAsync();
        Task<List<int>> GetAllRoomIdsAsync();
        Task<List<int>> GetAllServiceIdsAsync();
        Task<List<int>> GetAllReservationIdsAsync();
    }
}