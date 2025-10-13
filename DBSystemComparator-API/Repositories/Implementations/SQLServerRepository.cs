using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace DBSystemComparator_API.Repositories.Implementations
{
    public class SQLServerRepository : ISQLServerRepository
    {
        private readonly string _connectionString;

        public SQLServerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<TablesCountDTO> GetTablesCountAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            async Task<int> GetCountAsync(string tableName)
            {
                var cmdText = $"SELECT COUNT(*) FROM {tableName}";
                using var cmd = new SqlCommand(cmdText, connection);
                return (int)await cmd.ExecuteScalarAsync();
            }

            var clientsCount = await GetCountAsync("Clients");
            var roomsCount = await GetCountAsync("Rooms");
            var reservationsCount = await GetCountAsync("Reservations");
            var paymentsCount = await GetCountAsync("Payments");
            var servicesCount = await GetCountAsync("Services");
            var reservationServicesCount = await GetCountAsync("ReservationServices");

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
    }
}