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

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmdText = "SELECT roomId FROM reservations;";
            using var cmd = new SqlCommand(cmdText, connection);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (reader["roomId"] != DBNull.Value)
                    roomIds.Add(Convert.ToInt32(reader["roomId"]));
            }

            return roomIds;
        }
    }
}