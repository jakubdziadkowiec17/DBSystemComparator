using Cassandra;
using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Repositories.Interfaces
{
    public interface ICassandraRepository
    {
        Task<TablesCountDTO> GetTablesCountAsync();
        // CREATE
        Task CreateClientAsync(Guid id, string firstname, string secondname, string lastname, string email, DateTime dateofbirth, string address, string phonenumber, bool isactive);
        Task CreateRoomAsync(Guid id, int number, int capacity, int pricepernight, bool isactive);
        Task CreateServiceAsync(Guid id, string name, int price, bool isactive);
        Task CreateReservationAsync(Guid id, Guid clientid, Guid roomid, DateTime checkindate, DateTime checkoutdate, DateTime creationdate);
        Task CreateReservationServiceAsync(Guid id, Guid reservationid, Guid serviceid, DateTime creationdate);
        Task CreatePaymentAsync(Guid id, Guid reservationid, string description, int sum, DateTime creationdate);
        // READ
        Task<RowSet> ReadClientsWithRoomsAsync(bool isactive);
        Task<RowSet> ReadRoomsWithReservationCountAsync();
        Task<RowSet> ReadServicesUsageAsync();
        Task<RowSet> ReadPaymentsAboveAsync(int minsum);
        Task<RowSet> ReadReservationsWithServicesAsync(bool clientactive, bool serviceactive);
        // UPDATE
        Task UpdateClientsAddressPhoneAsync(bool isactive);
        Task UpdateRoomsPriceJoinReservationsAsync(int mincapacity);
        Task UpdateServicesPriceAsync(bool isactive);
        Task UpdateRoomsPriceInactiveAsync();
        Task UpdateRoomsPriceFutureReservationsAsync();
        // DELETE
        Task DeleteReservationsSmallRoomsAsync(int capacitythreshold);
        Task DeleteReservationsServicesFutureAsync(int limitRows);
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