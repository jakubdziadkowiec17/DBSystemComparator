using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using Npgsql;

namespace DBSystemComparator_API.Repositories.Implementations
{
    public class PostgreSQLRepository : IPostgreSQLRepository
    {
        private readonly string _connectionString;

        public PostgreSQLRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<TablesCountDTO> GetTablesCountAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            async Task<int> GetCountAsync(string tableName)
            {
                var cmdText = $"SELECT COUNT(*) FROM \"{tableName}\"";
                await using var cmd = new NpgsqlCommand(cmdText, connection);
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }

            var clientsCount = await GetCountAsync("clients");
            var roomsCount = await GetCountAsync("rooms");
            var reservationsCount = await GetCountAsync("reservations");
            var paymentsCount = await GetCountAsync("payments");
            var servicesCount = await GetCountAsync("services");
            var reservationServicesCount = await GetCountAsync("reservationservices");

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

        public async Task<List<int>> GetAllRoomIdsAsync()
        {
            var roomIds = new List<int>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmdText = "SELECT roomId FROM reservations;";
            await using var cmd = new NpgsqlCommand(cmdText, connection);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (reader["roomId"] != DBNull.Value)
                    roomIds.Add(Convert.ToInt32(reader["roomId"]));
            }

            return roomIds;
        }
    }
}