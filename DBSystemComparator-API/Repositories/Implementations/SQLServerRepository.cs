using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DBSystemComparator_API.Repositories.Implementations
{
    public class SQLServerRepository : ISQLServerRepository
    {
        private readonly string _connectionString;

        public SQLServerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // CREATE

        public Task<int> CreateClientAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive)
        {
            var sql = @"
                INSERT INTO Clients (FirstName, SecondName, LastName, Email, DateOfBirth, Address, PhoneNumber, IsActive)
                OUTPUT INSERTED.Id
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

            return ExecuteScalarAsync<int>(sql, parameters);
        }

        public Task<int> CreateRoomAsync(int number, int capacity, int pricePerNight, bool isActive)
        {
            var sql = @"
                INSERT INTO Rooms (Number, Capacity, PricePerNight, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@Number, @Capacity, @Price, @IsActive)";

            var parameters = new Dictionary<string, object>
            {
                {"@Number", number},
                {"@Capacity", capacity},
                {"@Price", pricePerNight},
                {"@IsActive", isActive ? 1 : 0}
            };

            return ExecuteScalarAsync<int>(sql, parameters);
        }

        public Task<int> CreateServiceAsync(string name, int price, bool isActive)
        {
            var sql = @"
                INSERT INTO Services (Name, Price, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@Name, @Price, @IsActive)";

            var parameters = new Dictionary<string, object>
            {
                {"@Name", name},
                {"@Price", price},
                {"@IsActive", isActive ? 1 : 0}
            };

            return ExecuteScalarAsync<int>(sql, parameters);
        }

        public async Task<List<int>> CreateClientsAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive, int count)
        {
            var tasks = new List<Task<int>>();

            for (int i = 0; i < count; i++)
            {
                tasks.Add(CreateClientAsync(firstName, secondName, lastName, email, dob, address, phone, isActive));
            }

            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async Task<List<int>> CreateRoomsAsync(int number, int capacity, int pricePerNight, bool isActive, int count)
        {
            var tasks = new List<Task<int>>();

            for (int i = 0; i < count; i++)
            {
                tasks.Add(CreateRoomAsync(number, capacity, pricePerNight, isActive));
            }

            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        // READ

        public Task<List<Dictionary<string, object>>> ReadReservationsAfterSecondHalf2025Async()
        {
            var sql = @"
                SELECT r.Id AS ReservationId, r.CheckInDate, r.CheckOutDate, c.FirstName, c.LastName
                FROM Reservations r
                JOIN Clients c ON r.ClientId = c.Id
                WHERE r.CheckInDate > @CheckInThreshold";

            var parameters = new Dictionary<string, object>
            {
                { "@CheckInThreshold", new DateTime(2025, 06, 30) }
            };

            return ExecuteQueryAsync(sql, parameters);
        }

        public Task<List<Dictionary<string, object>>> ReadReservationsWithPaymentsAboveAsync(int minSum)
        {
            var sql = @"
                SELECT r.Id AS ReservationId, r.CheckInDate, r.CheckOutDate, p.Sum, c.FirstName, c.LastName
                FROM Reservations r
                JOIN Clients c ON r.ClientId = c.Id
                JOIN Payments p ON r.Id = p.ReservationId
                WHERE p.Sum > @MinSum";

            var parameters = new Dictionary<string, object>
            {
                { "@MinSum", minSum }
            };

            return ExecuteQueryAsync(sql, parameters);
        }

        public Task<List<Dictionary<string, object>>> ReadClientsWithActiveReservationsAsync()
        {
            var sql = @"
                SELECT DISTINCT c.Id, c.FirstName, c.LastName, c.Email
                FROM Clients c
                JOIN Reservations r ON r.ClientId = c.Id
                WHERE r.CheckOutDate >= GETDATE()";

            return ExecuteQueryAsync(sql);
        }

        public Task<List<Dictionary<string, object>>> ReadActiveServicesUsedInReservationsAsync(int minSum)
        {
            var sql = @"
                SELECT DISTINCT s.Id AS ServiceId, s.Name, s.Price
                FROM Services s
                JOIN ReservationsServices rs ON s.Id = rs.ServiceId
                WHERE s.IsActive = @IsActive AND s.Price > @MinSum";

            var parameters = new Dictionary<string, object>
            {
                { "@IsActive", 1 },
                { "@MinSum", minSum }
            };

            return ExecuteQueryAsync(sql, parameters);
        }

        public Task<List<Dictionary<string, object>>> ReadCapacityReservationsAsync(int capacityThreshold)
        {
            var sql = @"
                SELECT r.Id AS ReservationId, r.CheckInDate, r.CheckOutDate, c.FirstName, c.LastName, rm.Number AS RoomNumber, rm.Capacity
                FROM Reservations r
                JOIN Clients c ON r.ClientId = c.Id
                JOIN Rooms rm ON r.RoomId = rm.Id
                WHERE rm.Capacity > @CapacityThreshold";

            var parameters = new Dictionary<string, object>
            {
                { "@CapacityThreshold", capacityThreshold }
            };

            return ExecuteQueryAsync(sql, parameters);
        }

        // UPDATE

        public Task<int> UpdateClientsAddressAndPhoneAsync(bool isActive, DateTime dateThreshold)
        {
            var sql = @"
                UPDATE Clients
                SET Address = 'Cracow, ul. abc 4',
                    PhoneNumber = '123456789'
                WHERE IsActive = @IsActive AND DateOfBirth > @DateOfBirth";

                    var parameters = new Dictionary<string, object>
                    {
                        { "@IsActive", isActive ? 1 : 0 },
                        { "@DateOfBirth", dateThreshold }
                    };

            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateRoomsPriceForReservationsAsync(int minCapacity, int priceIncrement)
        {
            var sql = @"
                UPDATE Rooms
                SET PricePerNight = PricePerNight + @PriceIncrement
                WHERE Capacity >= @MinCapacity
                  AND Id IN (SELECT RoomId FROM Reservations)";
            
            var parameters = new Dictionary<string, object>
            {
                { "@MinCapacity", minCapacity },
                { "@PriceIncrement", priceIncrement }
            };

            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateServicesPriceAsync(int priceIncrement, bool isActive, int price)
        {
            var sql = @"
                UPDATE Services
                SET Price = Price + @PriceIncrement
                WHERE IsActive = @IsActive AND Price > @Price";
            
            var parameters = new Dictionary<string, object>
            {
                { "@PriceIncrement", priceIncrement },
                { "@IsActive", isActive ? 1 : 0 },
                { "@Price", price }
            };

            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdatePriceForInactiveRoomsAsync(double discountMultiplier, int pricePerNight)
        {
            var sql = @"
                UPDATE Rooms
                SET PricePerNight = PricePerNight * @DiscountMultiplier
                WHERE IsActive = @IsActive AND PricePerNight > @PricePerNight";

            var parameters = new Dictionary<string, object>
            {
                { "@DiscountMultiplier", discountMultiplier },
                { "@IsActive", 0 },
                { "@PricePerNight", pricePerNight }
            };

            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateRoomsPriceForReservationsToApril2024Async(int priceDecrement)
        {
            var sql = @"
                UPDATE Rooms
                SET PricePerNight = PricePerNight - @PriceDecrement
                WHERE Id IN (
                    SELECT RoomId
                    FROM Reservations
                    WHERE CheckInDate < '2023-04-01'
                )";
            
            var parameters = new Dictionary<string, object>
            {
                { "@PriceDecrement", priceDecrement }
            };

            return ExecuteNonQueryAsync(sql, parameters);
        }

        // DELETE

        public Task<int> DeletePaymentsOlderThanMarch2024Async()
        {
            var sql = @"DELETE FROM Payments WHERE ReservationId IN (SELECT Id FROM Reservations WHERE CheckInDate < '2023-03-01')";
            return ExecuteNonQueryAsync(sql);
        }

        public Task<int> DeletePaymentsToSumAsync(int sum)
        {
            var sql = @"DELETE FROM Payments WHERE Sum < @Sum";
            var parameters = new Dictionary<string, object> { { "@Sum", sum } };

            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> DeleteReservationsServicesOlderThanMarch2023Async()
        {
            var sql = @"DELETE FROM ReservationsServices WHERE ReservationId IN (SELECT Id FROM Reservations WHERE CheckInDate < '2023-03-01')";
            return ExecuteNonQueryAsync(sql);
        }

        public Task<int> DeleteReservationsServicesWithServicePriceBelowAsync(int price)
        {
            var sql = @"DELETE FROM ReservationsServices WHERE ServiceId IN (SELECT Id FROM Services WHERE Price < @Price)";
            var parameters = new Dictionary<string, object> { { "@Price", price } };

            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> DeleteUnusedServicesPriceBelowAsync(int price)
        {
            var sql = @"DELETE FROM Services WHERE Price < @Price AND Id NOT IN (SELECT DISTINCT ServiceId FROM ReservationsServices)";
            var parameters = new Dictionary<string, object> { { "@Price", price } };

            return ExecuteNonQueryAsync(sql, parameters);
        }

        // HELPERS

        public async Task CreateClientsBatchAsync(IEnumerable<(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive)> clients)
        {
            var dt = new DataTable();
            dt.Columns.Add("FirstName", typeof(string));
            dt.Columns.Add("SecondName", typeof(string));
            dt.Columns.Add("LastName", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("DateOfBirth", typeof(DateTime));
            dt.Columns.Add("Address", typeof(string));
            dt.Columns.Add("PhoneNumber", typeof(string));
            dt.Columns.Add("IsActive", typeof(bool));

            foreach (var c in clients)
            {
                dt.Rows.Add(c.firstName, c.secondName, c.lastName, c.email, c.dob, c.address, c.phone, c.isActive);
            }

            using var bulk = new SqlBulkCopy(_connectionString)
            {
                DestinationTableName = "Clients"
            };

            bulk.ColumnMappings.Add("FirstName", "FirstName");
            bulk.ColumnMappings.Add("SecondName", "SecondName");
            bulk.ColumnMappings.Add("LastName", "LastName");
            bulk.ColumnMappings.Add("Email", "Email");
            bulk.ColumnMappings.Add("DateOfBirth", "DateOfBirth");
            bulk.ColumnMappings.Add("Address", "Address");
            bulk.ColumnMappings.Add("PhoneNumber", "PhoneNumber");
            bulk.ColumnMappings.Add("IsActive", "IsActive");

            await bulk.WriteToServerAsync(dt);
        }

        public async Task CreateRoomsBatchAsync(List<(int number, int capacity, int pricePerNight, bool isActive)> rooms)
        {
            var dt = new DataTable();
            dt.Columns.Add("Number", typeof(int));
            dt.Columns.Add("Capacity", typeof(int));
            dt.Columns.Add("PricePerNight", typeof(int));
            dt.Columns.Add("IsActive", typeof(bool));

            foreach (var r in rooms)
            {
                dt.Rows.Add(
                    r.number,
                    r.capacity,
                    r.pricePerNight,
                    r.isActive ? true : false
                );
            }

            using var bulk = new SqlBulkCopy(_connectionString)
            {
                DestinationTableName = "Rooms"
            };

            bulk.ColumnMappings.Add("Number", "Number");
            bulk.ColumnMappings.Add("Capacity", "Capacity");
            bulk.ColumnMappings.Add("PricePerNight", "PricePerNight");
            bulk.ColumnMappings.Add("IsActive", "IsActive");

            await bulk.WriteToServerAsync(dt);
        }

        public async Task CreateServicesBatchAsync(List<(string name, int price, bool isActive)> services)
        {
            if (services == null || services.Count == 0) return;

            var dt = new DataTable();
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Price", typeof(int));
            dt.Columns.Add("IsActive", typeof(bool));

            foreach (var s in services)
            {
                dt.Rows.Add(s.name ?? string.Empty, s.price, s.isActive ? true : false);
            }

            using var bulk = new SqlBulkCopy(_connectionString)
            {
                DestinationTableName = "Services"
            };

            bulk.ColumnMappings.Add("Name", "Name");
            bulk.ColumnMappings.Add("Price", "Price");
            bulk.ColumnMappings.Add("IsActive", "IsActive");

            await bulk.WriteToServerAsync(dt);
        }

        public async Task CreateReservationsBatchAsync(List<(int clientId, int roomId, DateTime checkIn, DateTime checkOut, DateTime creationDate)> reservations)
        {
            if (reservations == null || reservations.Count == 0) return;

            var dt = new DataTable();
            dt.Columns.Add("ClientId", typeof(int));
            dt.Columns.Add("RoomId", typeof(int));
            dt.Columns.Add("CheckInDate", typeof(DateTime));
            dt.Columns.Add("CheckOutDate", typeof(DateTime));
            dt.Columns.Add("CreationDate", typeof(DateTime));

            foreach (var r in reservations)
                dt.Rows.Add(r.clientId, r.roomId, r.checkIn, r.checkOut, r.creationDate);

            using var bulk = new SqlBulkCopy(_connectionString)
            {
                DestinationTableName = "Reservations",
                EnableStreaming = true
            };

            bulk.ColumnMappings.Add("ClientId", "ClientId");
            bulk.ColumnMappings.Add("RoomId", "RoomId");
            bulk.ColumnMappings.Add("CheckInDate", "CheckInDate");
            bulk.ColumnMappings.Add("CheckOutDate", "CheckOutDate");
            bulk.ColumnMappings.Add("CreationDate", "CreationDate");

            await bulk.WriteToServerAsync(dt);
        }

        public async Task CreatePaymentsBatchAsync(List<(int reservationId, string description, int sum, DateTime creationDate)> payments)
        {
            if (payments == null || payments.Count == 0) return;

            var dt = new DataTable();
            dt.Columns.Add("ReservationId", typeof(int));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("Sum", typeof(int));
            dt.Columns.Add("CreationDate", typeof(DateTime));

            foreach (var p in payments)
                dt.Rows.Add(p.reservationId, p.description ?? string.Empty, p.sum, p.creationDate);

            using var bulk = new SqlBulkCopy(_connectionString)
            {
                DestinationTableName = "Payments",
                EnableStreaming = true
            };

            bulk.ColumnMappings.Add("ReservationId", "ReservationId");
            bulk.ColumnMappings.Add("Description", "Description");
            bulk.ColumnMappings.Add("Sum", "Sum");
            bulk.ColumnMappings.Add("CreationDate", "CreationDate");

            await bulk.WriteToServerAsync(dt);
        }

        public async Task CreateReservationsServicesBatchAsync(List<(int reservationId, int serviceId, DateTime creationDate)> resServices)
        {
            if (resServices == null || resServices.Count == 0) return;

            var dt = new DataTable();
            dt.Columns.Add("ReservationId", typeof(int));
            dt.Columns.Add("ServiceId", typeof(int));
            dt.Columns.Add("CreationDate", typeof(DateTime));

            foreach (var rs in resServices)
                dt.Rows.Add(rs.reservationId, rs.serviceId, rs.creationDate);

            using var bulk = new SqlBulkCopy(_connectionString)
            {
                DestinationTableName = "ReservationsServices",
                EnableStreaming = true
            };

            bulk.ColumnMappings.Add("ReservationId", "ReservationId");
            bulk.ColumnMappings.Add("ServiceId", "ServiceId");
            bulk.ColumnMappings.Add("CreationDate", "CreationDate");

            await bulk.WriteToServerAsync(dt);
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

        public async Task<List<int>> GetAllClientIdsAsync()
        {
            var ids = new List<int>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand("SELECT Id FROM Clients", connection);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ids.Add(reader.GetInt32(0));
            }
            return ids;
        }

        public async Task<List<int>> GetAllRoomIdsAsync()
        {
            var ids = new List<int>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand("SELECT Id FROM Rooms", connection);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ids.Add(reader.GetInt32(0));
            }
            return ids;
        }

        public async Task<List<int>> GetAllServiceIdsAsync()
        {
            var ids = new List<int>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand("SELECT Id FROM Services", connection);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ids.Add(reader.GetInt32(0));
            }
            return ids;
        }

        public async Task<List<int>> GetAllReservationIdsAsync()
        {
            var result = new List<int>();
            var sql = "SELECT Id FROM Reservations";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(sql, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(reader.GetInt32(0));
            }

            return result;
        }

        public Task<int> DeleteAllClientsAsync() => ExecuteNonQueryAsync("DELETE FROM Clients");

        public Task<int> DeleteAllRoomsAsync() => ExecuteNonQueryAsync("DELETE FROM Rooms");

        public Task<int> DeleteAllReservationsAsync() => ExecuteNonQueryAsync("DELETE FROM Reservations");

        public Task<int> DeleteAllReservationsServicesAsync() => ExecuteNonQueryAsync("DELETE FROM ReservationsServices");

        public Task<int> DeleteAllPaymentsAsync() => ExecuteNonQueryAsync("DELETE FROM Payments");

        public Task<int> DeleteAllServicesAsync() => ExecuteNonQueryAsync("DELETE FROM Services");

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

        private async Task<int> ExecuteNonQueryAsync(string sql)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(sql, connection);
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

        private async Task<T> ExecuteScalarAsync<T>(string sql, Dictionary<string, object> parameters)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            foreach (var p in parameters)
                command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return (T)Convert.ChangeType(result, typeof(T));
        }
    }
}