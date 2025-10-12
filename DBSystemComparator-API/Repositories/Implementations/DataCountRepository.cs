using Cassandra;
using DBSystemComparator_API.Database;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace DBSystemComparator_API.Repositories.Implementations
{
    public class DataCountRepository : IDataCountRepository
    {
        private readonly SqlServerDbContext _sqlServerDbContext;
        private readonly PostgresDbContext _postgresDbContext;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly Cassandra.ISession _session;

        public DataCountRepository(SqlServerDbContext sqlServerDbContext, PostgresDbContext postgresDbContext, IMongoDatabase mongoDatabase, Cassandra.ISession session)
        {
            _sqlServerDbContext = sqlServerDbContext;
            _postgresDbContext = postgresDbContext;
            _mongoDatabase = mongoDatabase;
            _session = session;
        }

        public async Task<TablesCountDTO> GetTablesCountForPostgreSQLAsync()
        {
            var clientsCount = await _postgresDbContext.Clients.CountAsync();
            var roomsCount = await _postgresDbContext.Rooms.CountAsync();
            var reservationsCount = await _postgresDbContext.Reservations.CountAsync();
            var paymentsCount = await _postgresDbContext.Payments.CountAsync();
            var servicesCount = await _postgresDbContext.Services.CountAsync();
            var reservationServicesCount = await _postgresDbContext.ReservationServices.CountAsync();

            return new TablesCountDTO()
            {
                ClientsCount = clientsCount,
                RoomsCount = roomsCount,
                ReservationsCount = reservationsCount,
                PaymentsCount = paymentsCount,
                ServicesCount = servicesCount,
                ReservationServicesCount = reservationServicesCount
            };
        }

        public async Task<TablesCountDTO> GetTablesCountForSQLServerAsync()
        {
            var clientsCount =  await _sqlServerDbContext.Clients.CountAsync();
            var roomsCount = await _sqlServerDbContext.Rooms.CountAsync();
            var reservationsCount = await _sqlServerDbContext.Reservations.CountAsync();
            var paymentsCount = await _sqlServerDbContext.Payments.CountAsync();
            var servicesCount = await _sqlServerDbContext.Services.CountAsync();
            var reservationServicesCount = await _sqlServerDbContext.ReservationServices.CountAsync();

            return new TablesCountDTO()
            {
                ClientsCount = clientsCount,
                RoomsCount = roomsCount,
                ReservationsCount = reservationsCount,
                PaymentsCount = paymentsCount,
                ServicesCount = servicesCount,
                ReservationServicesCount = reservationServicesCount
            };
        }

        public async Task<TablesCountDTO> GetTablesCountForMongoDBAsync()
        {
            var clientsCount = await _mongoDatabase.GetCollection<dynamic>("Clients").CountDocumentsAsync(FilterDefinition<dynamic>.Empty);
            var roomsCount = await _mongoDatabase.GetCollection<dynamic>("Rooms").CountDocumentsAsync(FilterDefinition<dynamic>.Empty);
            var reservationsCount = await _mongoDatabase.GetCollection<dynamic>("Reservations").CountDocumentsAsync(FilterDefinition<dynamic>.Empty);
            var paymentsCount = await _mongoDatabase.GetCollection<dynamic>("Payments").CountDocumentsAsync(FilterDefinition<dynamic>.Empty);
            var servicesCount = await _mongoDatabase.GetCollection<dynamic>("Services").CountDocumentsAsync(FilterDefinition<dynamic>.Empty);
            var reservationServicesCount = await _mongoDatabase.GetCollection<dynamic>("ReservationServices").CountDocumentsAsync(FilterDefinition<dynamic>.Empty);

            return new TablesCountDTO()
            {
                ClientsCount = (int)clientsCount,
                RoomsCount = (int)roomsCount,
                ReservationsCount = (int)reservationsCount,
                PaymentsCount = (int)paymentsCount,
                ServicesCount = (int)servicesCount,
                ReservationServicesCount = (int)reservationServicesCount
            };
        }

        public async Task<TablesCountDTO> GetTablesCountForCassandraAsync()
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