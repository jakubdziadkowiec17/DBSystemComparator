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
            var reservationsServicesCount = await GetCountAsync("reservationsservices");

            return new TablesCountDTO()
            {
                ClientsCount = clientsCount,
                RoomsCount = roomsCount,
                ReservationsCount = reservationsCount,
                PaymentsCount = paymentsCount,
                ServicesCount = servicesCount,
                ReservationsServicesCount = reservationsServicesCount
            };
        }

        private async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(sql, connection);

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                }
            }

            return await cmd.ExecuteNonQueryAsync();
        }

        private async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            var result = new List<Dictionary<string, object>>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(sql, connection);

            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
            }

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                result.Add(row);
            }
            return result;
        }

        // CREATE
        public Task<int> CreateClientAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive)
        {
            var sql = @"INSERT INTO Clients (FirstName, SecondName, LastName, Email, DateOfBirth, Address, PhoneNumber, IsActive) 
                        VALUES (@FirstName, @SecondName, @LastName, @Email, @DOB, @Address, @Phone, @IsActive)";
            var parameters = new Dictionary<string, object>
            {
                {"@FirstName", firstName},
                {"@SecondName", secondName},
                {"@LastName", lastName},
                {"@Email", email},
                {"@DOB", dob},
                {"@Address", address},
                {"@Phone", phone},
                {"@IsActive", isActive ? 1 : 0}
            };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> CreateRoomAsync(int number, int capacity, int pricePerNight, bool isActive)
        {
            var sql = "INSERT INTO Rooms (Number, Capacity, PricePerNight, IsActive) VALUES (@Number, @Capacity, @Price, @IsActive)";
            var parameters = new Dictionary<string, object>
            {
                {"@Number", number},
                {"@Capacity", capacity},
                {"@Price", pricePerNight},
                {"@IsActive", isActive ? 1 : 0}
            };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> CreateServiceAsync(string name, int price, bool isActive)
        {
            var sql = "INSERT INTO Services (Name, Price, IsActive) VALUES (@Name, @Price, @IsActive)";
            var parameters = new Dictionary<string, object>
            {
                {"@Name", name},
                {"@Price", price},
                {"@IsActive", isActive ? 1 : 0}
            };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> CreateReservationAsync(int clientId, int roomId, DateTime checkIn, DateTime checkOut, DateTime creationDate)
        {
            var sql = @"INSERT INTO Reservations (ClientId, RoomId, CheckInDate, CheckOutDate, CreationDate)
                        VALUES (@ClientId, @RoomId, @CheckIn, @CheckOut, @CreationDate)";
            var parameters = new Dictionary<string, object>
            {
                {"@ClientId", clientId},
                {"@RoomId", roomId},
                {"@CheckIn", checkIn},
                {"@CheckOut", checkOut},
                {"@CreationDate", creationDate}
            };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> CreateReservationServiceAsync(int reservationId, int serviceId, DateTime creationDate)
        {
            var sql = @"INSERT INTO ReservationsServices (ReservationId, ServiceId, CreationDate)
                        VALUES (@ReservationId, @ServiceId, @CreationDate)";
            var parameters = new Dictionary<string, object>
            {
                {"@ReservationId", reservationId},
                {"@ServiceId", serviceId},
                {"@CreationDate", creationDate}
            };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> CreatePaymentAsync(int reservationId, string description, int sum, DateTime creationDate)
        {
            var sql = @"INSERT INTO Payments (ReservationId, Description, Sum, CreationDate)
                        VALUES (@ReservationId, @Description, @Sum, @CreationDate)";
            var parameters = new Dictionary<string, object>
            {
                {"@ReservationId", reservationId},
                {"@Description", description},
                {"@Sum", sum},
                {"@CreationDate", creationDate}
            };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        // READ
        public Task<List<Dictionary<string, object>>> ReadClientsWithRoomsAsync(bool isActive)
        {
            var sql = @"SELECT c.Id, c.FirstName, c.LastName, r.Number, r.PricePerNight
                        FROM Clients c
                        LEFT JOIN Reservations res ON res.ClientId = c.Id
                        LEFT JOIN Rooms r ON res.RoomId = r.Id
                        WHERE c.IsActive = @IsActive AND r.IsActive = @IsActive";
            var parameters = new Dictionary<string, object> { { "@IsActive", isActive ? 1 : 0 } };
            return ExecuteQueryAsync(sql, parameters);
        }

        public Task<List<Dictionary<string, object>>> ReadRoomsWithReservationCountAsync()
        {
            var sql = @"SELECT r.Id, r.Number, r.Capacity, COUNT(res.Id) AS ReservationCount
                        FROM Rooms r
                        LEFT JOIN Reservations res ON res.RoomId = r.Id
                        GROUP BY r.Id, r.Number, r.Capacity
                        HAVING COUNT(res.Id) > 0";
            return ExecuteQueryAsync(sql);
        }

        public Task<List<Dictionary<string, object>>> ReadServicesUsageAsync()
        {
            var sql = @"SELECT s.Name AS ServiceName, s.Price, COUNT(rs.ReservationId) AS UsageCount
                        FROM Services s
                        LEFT JOIN ReservationsServices rs ON s.Id = rs.ServiceId
                        GROUP BY s.Name, s.Price
                        ORDER BY UsageCount DESC";
            return ExecuteQueryAsync(sql);
        }

        public Task<List<Dictionary<string, object>>> ReadPaymentsAboveAsync(int minSum)
        {
            var sql = @"SELECT p.Id, p.Sum, p.CreationDate, c.FirstName AS ClientName, r.Number AS RoomNumber
                        FROM Payments p
                        LEFT JOIN Reservations res ON res.Id = p.ReservationId
                        LEFT JOIN Clients c ON res.ClientId = c.Id
                        LEFT JOIN Rooms r ON res.RoomId = r.Id
                        WHERE p.Sum > @MinSum";
            var parameters = new Dictionary<string, object> { { "@MinSum", minSum } };
            return ExecuteQueryAsync(sql, parameters);
        }

        public Task<List<Dictionary<string, object>>> ReadReservationsWithServicesAsync(bool clientActive, bool serviceActive)
        {
            var sql = @"SELECT res.Id AS ReservationId, c.LastName, s.Name AS ServiceName, s.Price, res.CheckInDate, res.CheckOutDate
                        FROM Reservations res
                        LEFT JOIN Clients c ON res.ClientId = c.Id
                        LEFT JOIN ReservationsServices rs ON rs.ReservationId = res.Id
                        LEFT JOIN Services s ON rs.ServiceId = s.Id
                        WHERE c.IsActive = @ClientActive AND s.IsActive = @ServiceActive";
            var parameters = new Dictionary<string, object>
            {
                {"@ClientActive", clientActive ? 1 : 0},
                {"@ServiceActive", serviceActive ? 1 : 0}
            };
            return ExecuteQueryAsync(sql, parameters);
        }

        // UPDATE
        public Task<int> UpdateClientsAddressPhoneAsync(bool isActive)
        {
            var sql = @"UPDATE Clients SET Address = 'Cracow, ul. abc 4', PhoneNumber = '123456789'
                        WHERE Id IN (SELECT TOP 200 Id FROM Clients WHERE IsActive = @IsActive)";
            var parameters = new Dictionary<string, object> { { "@IsActive", isActive ? 1 : 0 } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateRoomsPriceJoinReservationsAsync(int minCapacity)
        {
            var sql = @"UPDATE r SET r.PricePerNight = r.PricePerNight + 150
                        FROM Rooms r
                        INNER JOIN Reservations res ON r.Id = res.RoomId
                        WHERE r.Capacity >= @MinCapacity";
            var parameters = new Dictionary<string, object> { { "@MinCapacity", minCapacity } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateServicesPriceAsync(bool isActive)
        {
            var sql = "UPDATE Services SET Price = Price + 25 WHERE IsActive = @IsActive";
            var parameters = new Dictionary<string, object> { { "@IsActive", isActive ? 1 : 0 } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateRoomsPriceInactiveAsync()
        {
            var sql = "UPDATE Rooms SET PricePerNight = PricePerNight * 0.8 WHERE IsActive = @IsActive";
            var parameters = new Dictionary<string, object> { { "@IsActive", 0 } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateRoomsPriceFutureReservationsAsync()
        {
            var sql = "UPDATE Rooms SET PricePerNight = PricePerNight - 15 WHERE Id IN (SELECT RoomId FROM Reservations WHERE CheckInDate > GETDATE())";
            return ExecuteNonQueryAsync(sql);
        }

        // DELETE
        public Task<int> DeleteReservationsSmallRoomsAsync(int capacityThreshold)
        {
            var sql = "DELETE FROM Reservations WHERE RoomId IN (SELECT Id FROM Rooms WHERE Capacity < @CapacityThreshold AND CheckInDate > GETDATE())";
            var parameters = new Dictionary<string, object> { { "@CapacityThreshold", capacityThreshold } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> DeleteReservationsServicesFutureAsync(int topRows)
        {
            var sql = "DELETE FROM ReservationsServices WHERE ReservationId IN (SELECT TOP (@TopRows) Id FROM Reservations WHERE CheckInDate > GETDATE())";
            var parameters = new Dictionary<string, object> { { "@TopRows", topRows } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> DeleteReservationsWithoutPaymentsAsync()
        {
            var sql = "DELETE FROM Reservations WHERE Id NOT IN (SELECT DISTINCT ReservationId FROM Payments)";
            return ExecuteNonQueryAsync(sql);
        }

        public Task<int> DeleteInactiveClientsWithoutReservationsAsync()
        {
            var sql = "DELETE FROM Clients WHERE IsActive = @IsActive AND Id NOT IN (SELECT DISTINCT ClientId FROM Reservations)";
            var parameters = new Dictionary<string, object> { { "@IsActive", 0 } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> DeleteRoomsWithoutReservationsAsync()
        {
            var sql = "DELETE FROM Rooms WHERE Id NOT IN (SELECT DISTINCT RoomId FROM Reservations) AND IsActive = @IsActive";
            var parameters = new Dictionary<string, object> { { "@IsActive", 0 } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> DeleteAllClientsAsync() => ExecuteNonQueryAsync("DELETE FROM Clients");

        public Task<int> DeleteAllRoomsAsync() => ExecuteNonQueryAsync("DELETE FROM Rooms");

        public Task<int> DeleteAllReservationsAsync() => ExecuteNonQueryAsync("DELETE FROM Reservations");

        public Task<int> DeleteAllReservationsServicesAsync() => ExecuteNonQueryAsync("DELETE FROM ReservationsServices");

        public Task<int> DeleteAllPaymentsAsync() => ExecuteNonQueryAsync("DELETE FROM Payments");

        public Task<int> DeleteAllServicesAsync() => ExecuteNonQueryAsync("DELETE FROM Services");
    }
}