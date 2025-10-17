using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using Npgsql;
using System.Data;

namespace DBSystemComparator_API.Repositories.Implementations
{
    public class PostgreSQLRepository : IPostgreSQLRepository
    {
        private readonly string _connectionString;

        public PostgreSQLRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // CREATE
        public Task<int> CreateClientAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive)
        {
            var sql = @"
                INSERT INTO clients (firstname, secondname, lastname, email, dateofbirth, address, phonenumber, isactive)
                VALUES (@FirstName, @SecondName, @LastName, @Email, @DOB, @Address, @Phone, @IsActive)
                RETURNING id;";

            var parameters = new Dictionary<string, object>
            {
                {"@FirstName", firstName},
                {"@SecondName", secondName},
                {"@LastName", lastName},
                {"@Email", email},
                {"@DOB", dob},
                {"@Address", address},
                {"@Phone", phone},
                {"@IsActive", isActive}
            };

            return ExecuteScalarAsync<int>(sql, parameters);
        }

        public Task<int> CreateRoomAsync(int number, int capacity, int pricePerNight, bool isActive)
        {
            var sql = @"
                INSERT INTO rooms (number, capacity, pricepernight, isactive)
                VALUES (@Number, @Capacity, @Price, @IsActive)
                RETURNING id;";

            var parameters = new Dictionary<string, object>
            {
                {"@Number", number},
                {"@Capacity", capacity},
                {"@Price", pricePerNight},
                {"@IsActive", isActive}
            };

            return ExecuteScalarAsync<int>(sql, parameters);
        }

        public Task<int> CreateServiceAsync(string name, int price, bool isActive)
        {
            var sql = @"
                INSERT INTO services (name, price, isactive)
                VALUES (@Name, @Price, @IsActive)
                RETURNING id;";

            var parameters = new Dictionary<string, object>
            {
                {"@Name", name},
                {"@Price", price},
                {"@IsActive", isActive}
            };

            return ExecuteScalarAsync<int>(sql, parameters);
        }

        public async Task<List<int>> CreateClientsAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive, int count)
        {
            var tasks = new List<Task<int>>();
            for (int i = 0; i < count; i++)
                tasks.Add(CreateClientAsync(firstName, secondName, lastName, email, dob, address, phone, isActive));

            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async Task<List<int>> CreateRoomsAsync(int number, int capacity, int pricePerNight, bool isActive, int count)
        {
            var tasks = new List<Task<int>>();
            for (int i = 0; i < count; i++)
                tasks.Add(CreateRoomAsync(number, capacity, pricePerNight, isActive));

            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        // READ
        public Task<List<Dictionary<string, object>>> ReadReservationsAfter2024Async()
        {
            var sql = @"
                SELECT r.id AS reservationid, r.checkindate, r.checkoutdate, c.firstname, c.lastname
                FROM reservations r
                JOIN clients c ON r.clientid = c.id
                WHERE r.checkindate > @CheckInThreshold";

            var parameters = new Dictionary<string, object>
            {
                { "@CheckInThreshold", new DateTime(2024, 1, 1) }
            };

            return ExecuteQueryAsync(sql, parameters);
        }

        public Task<List<Dictionary<string, object>>> ReadReservationsWithPaymentsAboveAsync(int minSum)
        {
            var sql = @"
                SELECT r.id AS reservationid, r.checkindate, r.checkoutdate, p.sum, c.firstname, c.lastname
                FROM reservations r
                JOIN clients c ON r.clientid = c.id
                JOIN payments p ON r.id = p.reservationid
                WHERE p.sum > @MinSum";

            var parameters = new Dictionary<string, object> { { "@MinSum", minSum } };
            return ExecuteQueryAsync(sql, parameters);
        }

        public Task<List<Dictionary<string, object>>> ReadClientsWithActiveReservationsAsync()
        {
            var sql = @"
                SELECT DISTINCT c.id, c.firstname, c.lastname, c.email
                FROM clients c
                JOIN reservations r ON r.clientid = c.id
                WHERE r.checkoutdate IS NULL OR r.checkoutdate >= NOW()";

            return ExecuteQueryAsync(sql);
        }

        public Task<List<Dictionary<string, object>>> ReadActiveServicesUsedInReservationsAsync()
        {
            var sql = @"
                SELECT DISTINCT s.id AS serviceid, s.name, s.price
                FROM services s
                JOIN reservationsservices rs ON s.id = rs.serviceid
                WHERE s.isactive = @IsActive";

            var parameters = new Dictionary<string, object> { { "@IsActive", true } };
            return ExecuteQueryAsync(sql, parameters);
        }

        public Task<List<Dictionary<string, object>>> ReadCapacityReservationsAsync(int capacityThreshold)
        {
            var sql = @"
                SELECT r.id AS reservationid, r.checkindate, r.checkoutdate, c.firstname, c.lastname, rm.number AS roomnumber, rm.capacity
                FROM reservations r
                JOIN clients c ON r.clientid = c.id
                JOIN rooms rm ON r.roomid = rm.id
                WHERE rm.capacity > @CapacityThreshold";

            var parameters = new Dictionary<string, object> { { "@CapacityThreshold", capacityThreshold } };
            return ExecuteQueryAsync(sql, parameters);
        }

        // UPDATE
        public Task<int> UpdateClientsAddressAndPhoneAsync(bool isActive)
        {
            var sql = @"
                UPDATE clients
                SET address = 'Cracow, ul. abc 4',
                    phonenumber = '123456789'
                WHERE isactive = @IsActive";

            var parameters = new Dictionary<string, object> { { "@IsActive", isActive } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateRoomsPriceForReservationsAsync(int minCapacity, int priceIncrement)
        {
            var sql = @"
                UPDATE rooms
                SET pricepernight = pricepernight + @PriceIncrement
                WHERE capacity >= @MinCapacity
                  AND id IN (SELECT roomid FROM reservations)";

            var parameters = new Dictionary<string, object>
            {
                { "@MinCapacity", minCapacity },
                { "@PriceIncrement", priceIncrement }
            };

            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateServicesPriceAsync(int priceIncrement, bool isActive)
        {
            var sql = @"
                UPDATE services
                SET price = price + @PriceIncrement
                WHERE isactive = @IsActive";

            var parameters = new Dictionary<string, object>
            {
                { "@PriceIncrement", priceIncrement },
                { "@IsActive", isActive }
            };

            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdatePriceForInactiveRoomsAsync(double discountMultiplier)
        {
            var sql = @"
                UPDATE rooms
                SET pricepernight = pricepernight * @DiscountMultiplier
                WHERE isactive = false";

            var parameters = new Dictionary<string, object> { { "@DiscountMultiplier", discountMultiplier } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateRoomsPriceForReservationsTo2024Async(int priceDecrement)
        {
            var sql = @"
                UPDATE rooms
                SET pricepernight = pricepernight - @PriceDecrement
                WHERE id IN (SELECT roomid FROM reservations WHERE checkindate < '2024-01-01')";

            var parameters = new Dictionary<string, object> { { "@PriceDecrement", priceDecrement } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        // DELETE
        public Task<int> DeletePaymentsOlderThan2024Async()
        {
            var sql = @"DELETE FROM payments WHERE reservationid IN (SELECT id FROM reservations WHERE checkindate < '2024-01-01')";
            return ExecuteNonQueryAsync(sql);
        }

        public Task<int> DeletePaymentsWithoutReservationAsync()
        {
            var sql = @"DELETE FROM payments WHERE reservationid NOT IN (SELECT id FROM reservations)";
            return ExecuteNonQueryAsync(sql);
        }

        public Task<int> DeleteReservationsServicesOlderThan2024Async()
        {
            var sql = @"DELETE FROM reservationsservices WHERE reservationid IN (SELECT id FROM reservations WHERE checkindate < '2024-01-01')";
            return ExecuteNonQueryAsync(sql);
        }

        public Task<int> DeleteReservationsServicesWithServicePriceBelowAsync(int price)
        {
            var sql = @"DELETE FROM reservationsservices WHERE serviceid IN (SELECT id FROM services WHERE price < @price)";
            var parameters = new Dictionary<string, object> { { "@price", price } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> DeleteUnusedServicesAsync()
        {
            var sql = @"DELETE FROM services WHERE id NOT IN (SELECT DISTINCT serviceid FROM reservationsservices)";
            return ExecuteNonQueryAsync(sql);
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
                dt.Rows.Add(c.firstName, c.secondName, c.lastName, c.email, c.dob, c.address, c.phone, c.isActive);

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var writer = connection.BeginBinaryImport("COPY Clients (FirstName, SecondName, LastName, Email, DateOfBirth, Address, PhoneNumber, IsActive) FROM STDIN (FORMAT BINARY)");
            foreach (DataRow row in dt.Rows)
            {
                writer.StartRow();
                for (int i = 0; i < dt.Columns.Count; i++)
                    writer.Write(row[i]);
            }
            await writer.CompleteAsync();
        }

        public async Task CreateRoomsBatchAsync(IEnumerable<(int number, int capacity, int pricePerNight, bool isActive)> rooms)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var writer = connection.BeginBinaryImport("COPY Rooms (Number, Capacity, PricePerNight, IsActive) FROM STDIN (FORMAT BINARY)");
            foreach (var r in rooms)
            {
                writer.StartRow();
                writer.Write(r.number);
                writer.Write(r.capacity);
                writer.Write(r.pricePerNight);
                writer.Write(r.isActive);
            }
            await writer.CompleteAsync();
        }

        public async Task CreateServicesBatchAsync(IEnumerable<(string name, int price, bool isActive)> services)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var writer = connection.BeginBinaryImport("COPY Services (Name, Price, IsActive) FROM STDIN (FORMAT BINARY)");
            foreach (var s in services)
            {
                writer.StartRow();
                writer.Write(s.name);
                writer.Write(s.price);
                writer.Write(s.isActive);
            }
            await writer.CompleteAsync();
        }

        public async Task CreateReservationsBatchAsync(IEnumerable<(int clientId, int roomId, DateTime checkIn, DateTime checkOut, DateTime creationDate)> reservations)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var writer = connection.BeginBinaryImport("COPY Reservations (ClientId, RoomId, CheckInDate, CheckOutDate, CreationDate) FROM STDIN (FORMAT BINARY)");
            foreach (var r in reservations)
            {
                writer.StartRow();
                writer.Write(r.clientId);
                writer.Write(r.roomId);
                writer.Write(r.checkIn);
                writer.Write(r.checkOut);
                writer.Write(r.creationDate);
            }
            await writer.CompleteAsync();
        }

        public async Task CreatePaymentsBatchAsync(IEnumerable<(int reservationId, string description, int sum, DateTime creationDate)> payments)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var writer = connection.BeginBinaryImport("COPY Payments (ReservationId, Description, Sum, CreationDate) FROM STDIN (FORMAT BINARY)");
            foreach (var p in payments)
            {
                writer.StartRow();
                writer.Write(p.reservationId);
                writer.Write(p.description);
                writer.Write(p.sum);
                writer.Write(p.creationDate);
            }
            await writer.CompleteAsync();
        }

        public async Task CreateReservationsServicesBatchAsync(IEnumerable<(int reservationId, int serviceId, DateTime creationDate)> resServices)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var writer = connection.BeginBinaryImport("COPY ReservationsServices (ReservationId, ServiceId, CreationDate) FROM STDIN (FORMAT BINARY)");
            foreach (var rs in resServices)
            {
                writer.StartRow();
                writer.Write(rs.reservationId);
                writer.Write(rs.serviceId);
                writer.Write(rs.creationDate);
            }
            await writer.CompleteAsync();
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
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand("SELECT Id FROM Clients", connection);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                ids.Add(reader.GetInt32(0));

            return ids;
        }

        public async Task<List<int>> GetAllRoomIdsAsync()
        {
            var ids = new List<int>();
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand("SELECT Id FROM Rooms", connection);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                ids.Add(reader.GetInt32(0));

            return ids;
        }

        public async Task<List<int>> GetAllServiceIdsAsync()
        {
            var ids = new List<int>();
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand("SELECT Id FROM Services", connection);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                ids.Add(reader.GetInt32(0));

            return ids;
        }

        public async Task<List<int>> GetAllReservationIdsAsync()
        {
            var ids = new List<int>();
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand("SELECT Id FROM Reservations", connection);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                ids.Add(reader.GetInt32(0));

            return ids;
        }

        public Task<int> DeleteAllClientsAsync() => ExecuteNonQueryAsync("DELETE FROM clients");
        public Task<int> DeleteAllRoomsAsync() => ExecuteNonQueryAsync("DELETE FROM rooms");
        public Task<int> DeleteAllReservationsAsync() => ExecuteNonQueryAsync("DELETE FROM reservations");
        public Task<int> DeleteAllReservationsServicesAsync() => ExecuteNonQueryAsync("DELETE FROM reservationsservices");
        public Task<int> DeleteAllPaymentsAsync() => ExecuteNonQueryAsync("DELETE FROM payments");
        public Task<int> DeleteAllServicesAsync() => ExecuteNonQueryAsync("DELETE FROM services");

        private async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, connection);

            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
            }

            return await cmd.ExecuteNonQueryAsync();
        }

        private async Task<int> ExecuteNonQueryAsync(string sql)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, connection);
            return await cmd.ExecuteNonQueryAsync();
        }

        private async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            var result = new List<Dictionary<string, object>>();
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, connection);

            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
            }

            await using var reader = await cmd.ExecuteReaderAsync();
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
            await using var connection = new NpgsqlConnection(_connectionString);
            await using var command = new NpgsqlCommand(sql, connection);

            foreach (var p in parameters)
                command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return (T)Convert.ChangeType(result, typeof(T));
        }
    }
}