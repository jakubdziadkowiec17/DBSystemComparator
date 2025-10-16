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

        // CREATE
        public Task<int> CreateClientAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive)
        {
            var sql = @"
                INSERT INTO clients (firstname, secondname, lastname, email, dateofbirth, address, phonenumber, isactive)
                VALUES (@firstname, @secondname, @lastname, @email, @dob, @address, @phone, @isactive)
                RETURNING id";

            var parameters = new Dictionary<string, object>
            {
                {"@firstname", firstName},
                {"@secondname", secondName},
                {"@lastname", lastName},
                {"@email", email},
                {"@dob", dob},
                {"@address", address},
                {"@phone", phone},
                {"@isactive", isActive}
            };

            return ExecuteScalarAsync<int>(sql, parameters);
        }

        public Task<int> CreateRoomAsync(int number, int capacity, int pricePerNight, bool isActive)
        {
            var sql = @"
                INSERT INTO rooms (number, capacity, pricepernight, isactive)
                VALUES (@number, @capacity, @price, @isactive)
                RETURNING id";

            var parameters = new Dictionary<string, object>
            {
                {"@number", number},
                {"@capacity", capacity},
                {"@price", pricePerNight},
                {"@isactive", isActive}
            };

            return ExecuteScalarAsync<int>(sql, parameters);
        }

        public Task<int> CreateServiceAsync(string name, int price, bool isActive)
        {
            var sql = @"
                INSERT INTO services (name, price, isactive)
                VALUES (@name, @price, @isactive)
                RETURNING id";

            var parameters = new Dictionary<string, object>
            {
                {"@name", name},
                {"@price", price},
                {"@isactive", isActive}
            };

            return ExecuteScalarAsync<int>(sql, parameters);
        }

        public Task<int> CreateReservationAsync(int clientId, int roomId, DateTime checkIn, DateTime checkOut, DateTime creationDate)
        {
            var sql = @"
                INSERT INTO reservations (clientid, roomid, checkindate, checkoutdate, creationdate)
                VALUES (@clientid, @roomid, @checkin, @checkout, @creationdate)
                RETURNING id";

            var parameters = new Dictionary<string, object>
            {
                {"@clientid", clientId},
                {"@roomid", roomId},
                {"@checkin", checkIn},
                {"@checkout", checkOut},
                {"@creationdate", creationDate}
            };

            return ExecuteScalarAsync<int>(sql, parameters);
        }

        public Task<int> CreateReservationServiceAsync(int reservationId, int serviceId, DateTime creationDate)
        {
            var sql = @"
                INSERT INTO reservationsservices (reservationid, serviceid, creationdate)
                VALUES (@reservationid, @serviceid, @creationdate)";

            var parameters = new Dictionary<string, object>
            {
                {"@reservationid", reservationId},
                {"@serviceid", serviceId},
                {"@creationdate", creationDate}
            };

            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> CreatePaymentAsync(int reservationId, string description, int sum, DateTime creationDate)
        {
            var sql = @"
                INSERT INTO payments (reservationid, description, sum, creationdate)
                VALUES (@reservationid, @description, @sum, @creationdate)
                RETURNING id";

            var parameters = new Dictionary<string, object>
            {
                {"@reservationid", reservationId},
                {"@description", description},
                {"@sum", sum},
                {"@creationdate", creationDate}
            };

            return ExecuteScalarAsync<int>(sql, parameters);
        }

        // READ
        public Task<List<Dictionary<string, object>>> ReadClientsWithRoomsAsync(bool isActive)
        {
            var sql = @"SELECT c.id, c.firstname, c.lastname, r.number, r.pricepernight
                        FROM clients c
                        LEFT JOIN reservations res ON res.clientid = c.id
                        LEFT JOIN rooms r ON res.roomid = r.id
                        WHERE c.isactive = @isactive AND r.isactive = @isactive";
            var parameters = new Dictionary<string, object> { { "@isactive", isActive } };
            return ExecuteQueryAsync(sql, parameters);
        }

        public Task<List<Dictionary<string, object>>> ReadRoomsWithReservationCountAsync()
        {
            var sql = @"SELECT r.id, r.number, r.capacity, COUNT(res.id) AS reservationcount
                        FROM rooms r
                        LEFT JOIN reservations res ON res.roomid = r.id
                        GROUP BY r.id, r.number, r.capacity
                        HAVING COUNT(res.id) > 0";
            return ExecuteQueryAsync(sql);
        }

        public Task<List<Dictionary<string, object>>> ReadServicesUsageAsync()
        {
            var sql = @"SELECT s.name AS servicename, s.price, COUNT(rs.reservationid) AS usagecount
                        FROM services s
                        LEFT JOIN reservationsservices rs ON s.id = rs.serviceid
                        GROUP BY s.name, s.price
                        ORDER BY usagecount DESC";
            return ExecuteQueryAsync(sql);
        }

        public Task<List<Dictionary<string, object>>> ReadPaymentsAboveAsync(int minSum)
        {
            var sql = @"SELECT p.id, p.sum, p.creationdate, c.firstname AS clientname, r.number AS roomnumber
                        FROM payments p
                        LEFT JOIN reservations res ON res.id = p.reservationid
                        LEFT JOIN clients c ON res.clientid = c.id
                        LEFT JOIN rooms r ON res.roomid = r.id
                        WHERE p.sum > @minsum";
            var parameters = new Dictionary<string, object> { { "@minsum", minSum } };
            return ExecuteQueryAsync(sql, parameters);
        }

        public Task<List<Dictionary<string, object>>> ReadReservationsWithServicesAsync(bool clientActive, bool serviceActive)
        {
            var sql = @"SELECT res.id AS reservationid, c.lastname, s.name AS servicename, s.price, res.checkindate, res.checkoutdate
                        FROM reservations res
                        LEFT JOIN clients c ON res.clientid = c.id
                        LEFT JOIN reservationsservices rs ON rs.reservationid = res.id
                        LEFT JOIN services s ON rs.serviceid = s.id
                        WHERE c.isactive = @clientactive AND s.isactive = @serviceactive";
            var parameters = new Dictionary<string, object>
            {
                {"@clientactive", clientActive},
                {"@serviceactive", serviceActive}
            };
            return ExecuteQueryAsync(sql, parameters);
        }

        // UPDATE
        public Task<int> UpdateClientsAddressPhoneAsync(bool isActive)
        {
            var sql = @"UPDATE clients SET address = 'Cracow, ul. abc 4', phonenumber = '123456789'
                        WHERE id IN (SELECT id FROM clients WHERE isactive = @isactive LIMIT 200)";
            var parameters = new Dictionary<string, object> { { "@isactive", isActive } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateRoomsPriceJoinReservationsAsync(int minCapacity)
        {
            var sql = @"UPDATE rooms r
                        SET pricepernight = pricepernight + 150
                        FROM reservations res
                        WHERE r.id = res.roomid AND r.capacity >= @mincapacity";
            var parameters = new Dictionary<string, object> { { "@mincapacity", minCapacity } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateServicesPriceAsync(bool isActive)
        {
            var sql = "UPDATE services SET price = price + 25 WHERE isactive = @isactive";
            var parameters = new Dictionary<string, object> { { "@isactive", isActive } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateRoomsPriceInactiveAsync()
        {
            var sql = "UPDATE rooms SET pricepernight = pricepernight * 0.8 WHERE isactive = @isactive";
            var parameters = new Dictionary<string, object> { { "@isactive", false } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> UpdateRoomsPriceFutureReservationsAsync()
        {
            var sql = "UPDATE rooms SET pricepernight = pricepernight - 15 WHERE id IN (SELECT roomid FROM reservations WHERE checkindate > NOW())";
            return ExecuteNonQueryAsync(sql);
        }

        // DELETE
        public Task<int> DeleteReservationsSmallRoomsAsync(int capacityThreshold)
        {
            var sql = "DELETE FROM reservations WHERE roomid IN (SELECT id FROM rooms WHERE capacity < @capacitythreshold AND checkindate > NOW())";
            var parameters = new Dictionary<string, object> { { "@capacitythreshold", capacityThreshold } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> DeleteReservationsServicesFutureAsync(int limitRows)
        {
            var sql = "DELETE FROM reservationsservices WHERE reservationid IN (SELECT id FROM reservations WHERE checkindate > NOW() LIMIT @limitrows)";
            var parameters = new Dictionary<string, object> { { "@limitrows", limitRows } };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> DeleteReservationsWithoutPaymentsAsync()
        {
            var sql = "DELETE FROM reservations WHERE id NOT IN (SELECT DISTINCT reservationid FROM payments)";
            return ExecuteNonQueryAsync(sql);
        }

        public Task<int> DeleteInactiveClientsWithoutReservationsAsync()
        {
            var sql = "DELETE FROM clients WHERE isactive = @isactive AND id NOT IN (SELECT DISTINCT clientid FROM reservations)";
            var parameters = new Dictionary<string, object>
            {
                { "@isactive", false }
            };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> DeleteRoomsWithoutReservationsAsync()
        {
            var sql = "DELETE FROM rooms WHERE id NOT IN (SELECT DISTINCT roomid FROM reservations) AND isactive = @isactive";
            var parameters = new Dictionary<string, object>
            {
                { "@isactive", false }
            };
            return ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<int> DeleteAllClientsAsync() => ExecuteNonQueryAsync("DELETE FROM clients");
        public Task<int> DeleteAllRoomsAsync() => ExecuteNonQueryAsync("DELETE FROM rooms");
        public Task<int> DeleteAllReservationsAsync() => ExecuteNonQueryAsync("DELETE FROM reservations");
        public Task<int> DeleteAllReservationsServicesAsync() => ExecuteNonQueryAsync("DELETE FROM reservationsservices");
        public Task<int> DeleteAllPaymentsAsync() => ExecuteNonQueryAsync("DELETE FROM payments");
        public Task<int> DeleteAllServicesAsync() => ExecuteNonQueryAsync("DELETE FROM services");
    }
}