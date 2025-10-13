using Cassandra;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using MongoDB.Driver;

namespace DBSystemComparator_API.Repositories.Implementations
{
    public class CassandraRepository : ICassandraRepository
    {
        private readonly Cassandra.ISession _session;

        public CassandraRepository(Cassandra.ISession session)
        {
            _session = session;
        }

        public async Task<TablesCountDTO> GetTablesCountAsync()
        {
            var clientsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT COUNT(*) FROM clients;"));
            var clientsCount = (long)clientsRs.FirstOrDefault()?["count"]!;

            var roomsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT COUNT(*) FROM rooms;"));
            var roomsCount = (long)roomsRs.FirstOrDefault()?["count"]!;

            var reservationsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT COUNT(*) FROM reservations;"));
            var reservationsCount = (long)reservationsRs.FirstOrDefault()?["count"]!;

            var paymentsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT COUNT(*) FROM payments;"));
            var paymentsCount = (long)paymentsRs.FirstOrDefault()?["count"]!;

            var servicesRs = await _session.ExecuteAsync(new SimpleStatement("SELECT COUNT(*) FROM services;"));
            var servicesCount = (long)servicesRs.FirstOrDefault()?["count"]!;

            var resServRs = await _session.ExecuteAsync(new SimpleStatement("SELECT COUNT(*) FROM reservation_services;"));
            var resServCount = (long)resServRs.FirstOrDefault()?["count"]!;

            return new TablesCountDTO
            {
                ClientsCount = (int)clientsCount,
                RoomsCount = (int)roomsCount,
                ReservationsCount = (int)reservationsCount,
                PaymentsCount = (int)paymentsCount,
                ServicesCount = (int)servicesCount,
                ReservationServicesCount = (int)resServCount
            };
        }
    }
}