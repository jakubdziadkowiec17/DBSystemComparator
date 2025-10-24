using DBSystemComparator_API.Models.DTOs;

namespace DBSystemComparator_API.Repositories.Interfaces
{
    public interface ICassandraRepository
    {
        // CREATE

        // READ

        // UPDATE

        // DELETE

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