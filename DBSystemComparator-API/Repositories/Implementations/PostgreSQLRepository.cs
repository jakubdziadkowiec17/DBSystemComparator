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

        public Task<int> CreateRoomAsync(int number, int capacity, double pricePerNight, bool isActive)
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
            var clients = new List<(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive)>();
            for (int i = 0; i < count; i++)
            {
                clients.Add((firstName, secondName, lastName, email, dob, address, phone, isActive));
            }
            await CreateClientsBatchAsync(clients);
            return Enumerable.Repeat(0, count).ToList();
        }

        public async Task<List<int>> CreateRoomsAsync(int number, int capacity, double pricePerNight, bool isActive, int count)
        {
            var rooms = new List<(int number, int capacity, double pricePerNight, bool isActive)>();
            for (int i = 0; i < count; i++)
            {
                rooms.Add((number, capacity, pricePerNight, isActive));
            }
            await CreateRoomsBatchAsync(rooms);
            return Enumerable.Repeat(0, count).ToList();
        }

        // READ
        public Task<List<Dictionary<string, object>>> ReadReservationsAfterSecondHalf2025Async()
        {
            var sql = @"
                SELECT r.id AS reservationid, r.checkindate, r.checkoutdate, c.firstname, c.lastname
                FROM reservations r
                JOIN clients c ON r.clientid = c.id
                WHERE r.checkindate > @CheckInThreshold";

            var parameters = new Dictionary<string, object>
            {
                { "@CheckInThreshold", new DateTime(2025, 06, 30) }
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
                WHERE r.checkoutdate >= NOW()";

            return ExecuteQueryAsync(sql);
        }

        public Task<List<Dictionary<string, object>>> ReadActiveServicesUsedInReservationsAsync(int minSum)
        {
            var sql = @"
                SELECT DISTINCT s.id AS serviceid, s.name, s.price
                FROM services s
                JOIN reservationsservices rs ON s.id = rs.serviceid
                WHERE s.isactive = @IsActive AND s.Price > @MinSum";

            var parameters = new Dictionary<string, object> { { "@IsActive", true }, { "@MinSum", minSum } };
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
        public Task<int> UpdateClientsAddressAndPhoneAsync(bool isActive, DateTime dateThreshold)
        {
            var sql = @"
                UPDATE clients
                SET address = 'Cracow, ul. abc 4',
                    phonenumber = '123456789'
                WHERE isactive = @IsActive AND dateofbirth > @DateOfBirth";

            var parameters = new Dictionary<string, object> { { "@IsActive", isActive }, { "@DateOfBirth", dateThreshold } };
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

        public Task<int> UpdateServicesPriceAsync(int priceIncrement, bool isActive, int price)
        {
            var sql = @"
                UPDATE services
                SET price = price + @PriceIncrement
                WHERE isactive = @IsActive AND price > @Price";

            var parameters = new Dictionary<string, object>
            {
                { "@PriceIncrement", priceIncrement },
                { "@IsActive", isActive },
                { "@Price", price }
            };

            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdatePriceForInactiveRoomsAsync(double discountMultiplier, double pricePerNight)
        {
            var sql = @"
                UPDATE rooms
                SET pricepernight = pricepernight * @DiscountMultiplier
                WHERE isactive = false AND pricepernight > @PricePerNight";

            var parameters = new Dictionary<string, object> { { "@DiscountMultiplier", discountMultiplier }, { "@PricePerNight", pricePerNight } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateRoomsPriceForReservationsToApril2024Async(int priceDecrement)
        {
            var sql = @"
                UPDATE rooms
                SET pricepernight = pricepernight - @PriceDecrement
                WHERE id IN (SELECT roomid FROM reservations WHERE checkindate < '2023-04-01')";

            var parameters = new Dictionary<string, object> { { "@PriceDecrement", priceDecrement } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        // DELETE
        public Task<int> DeletePaymentsOlderThanMarch2024Async()
        {
            var sql = @"DELETE FROM payments WHERE reservationid IN (SELECT id FROM reservations WHERE checkindate < '2023-03-01')";
            return ExecuteNonQueryAsync(sql);
        }

        public Task<int> DeletePaymentsToSumAsync(int sum)
        {
            var sql = @"DELETE FROM payments WHERE sum < @Sum";
            var parameters = new Dictionary<string, object> { { "@Sum", sum } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> DeleteReservationsServicesOlderThanMarch2023Async()
        {
            var sql = @"DELETE FROM reservationsservices WHERE reservationid IN (SELECT id FROM reservations WHERE checkindate < '2023-03-01')";
            return ExecuteNonQueryAsync(sql);
        }

        public Task<int> DeleteReservationsServicesWithServicePriceBelowAsync(int price)
        {
            var sql = @"DELETE FROM reservationsservices WHERE serviceid IN (SELECT id FROM services WHERE price < @price)";
            var parameters = new Dictionary<string, object> { { "@price", price } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<int> DeleteUnusedServicesPriceBelowAsync(int price)
        {
            const int batchSize = 10000;
            var totalDeleted = 0;

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            while (true)
            {
                var sql = @"
                    WITH to_del AS (
                        SELECT s.id
                        FROM services s
                        WHERE s.price < @Price
                          AND NOT EXISTS (
                              SELECT 1 FROM reservationsservices rs WHERE rs.serviceid = s.id
                          )
                        ORDER BY s.id
                        LIMIT @Batch
                    )
                    DELETE FROM services s
                    USING to_del d
                    WHERE s.id = d.id;";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Price", price);
                cmd.Parameters.AddWithValue("@Batch", batchSize);

                var affected = await cmd.ExecuteNonQueryAsync();
                totalDeleted += affected;

                if (affected == 0)
                    break;
            }

            return totalDeleted;
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

        public async Task CreateRoomsBatchAsync(IEnumerable<(int number, int capacity, double pricePerNight, bool isActive)> rooms)
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

        public async Task<List<ClientDTO>> GetAllClientsAsync()
        {
            var clients = new List<ClientDTO>();
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"SELECT id, firstname, secondname, lastname, email, dateofbirth, address, phonenumber, isactive FROM clients";

            await using var cmd = new NpgsqlCommand(sql, connection);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                clients.Add(new ClientDTO
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    SecondName = reader.GetString(2),
                    LastName = reader.GetString(3),
                    Email = reader.GetString(4),
                    BirthDate = reader.GetDateTime(5),
                    Address = reader.GetString(6),
                    PhoneNumber = reader.GetString(7),
                    IsActive = reader.GetBoolean(8)
                });
            }

            return clients;
        }

        public async Task<List<RoomDTO>> GetAllRoomsAsync()
        {
            var rooms = new List<RoomDTO>();
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"SELECT id, number, capacity, pricepernight, isactive FROM rooms";

            await using var cmd = new NpgsqlCommand(sql, connection);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                rooms.Add(new RoomDTO
                {
                    Id = reader.GetInt32(0),
                    RoomNumber = reader.GetInt32(1),
                    Floor = reader.GetInt32(2),
                    Price = reader.GetDouble(3),
                    IsAvailable = reader.GetBoolean(4)
                });
            }

            return rooms;
        }

        public async Task<List<ServiceDTO>> GetAllServicesAsync()
        {
            var services = new List<ServiceDTO>();
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"SELECT id, name, price, isactive FROM services";

            await using var cmd = new NpgsqlCommand(sql, connection);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                services.Add(new ServiceDTO
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetInt32(2),
                    IsAvailable = reader.GetBoolean(3)
                });
            }

            return services;
        }

        public async Task<List<ReservationDTO>> GetAllReservationsAsync()
        {
            var reservations = new List<ReservationDTO>();
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"SELECT id, clientid, roomid, checkindate, checkoutdate, creationdate FROM reservations";

            await using var cmd = new NpgsqlCommand(sql, connection);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reservations.Add(new ReservationDTO
                {
                    Id = reader.GetInt32(0),
                    ClientId = reader.GetInt32(1),
                    RoomId = reader.GetInt32(2),
                    CheckInDate = reader.GetDateTime(3),
                    CheckOutDate = reader.GetDateTime(4),
                    CreationDate = reader.GetDateTime(5)
                });
            }

            return reservations;
        }

        public async Task<List<ReservationServiceDTO>> GetAllReservationsServicesAsync()
        {
            var resServices = new List<ReservationServiceDTO>();
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"SELECT reservationid, serviceid, creationdate FROM reservationsservices";

            await using var cmd = new NpgsqlCommand(sql, connection);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                resServices.Add(new ReservationServiceDTO
                {
                    ReservationId = reader.GetInt32(0),
                    ServiceId = reader.GetInt32(1),
                    CreationDate = reader.GetDateTime(2)
                });
            }

            return resServices;
        }

        public async Task<List<PaymentDTO>> GetAllPaymentsAsync()
        {
            var payments = new List<PaymentDTO>();
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"SELECT id, reservationid, description, sum, creationdate FROM payments";

            await using var cmd = new NpgsqlCommand(sql, connection);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                payments.Add(new PaymentDTO
                {
                    Id = reader.GetInt32(0),
                    ReservationId = reader.GetInt32(1),
                    Description = reader.GetString(2),
                    Amount = reader.GetInt32(3),
                    PaymentDate = reader.GetDateTime(4)
                });
            }

            return payments;
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