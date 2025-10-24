using Cassandra;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;

namespace DBSystemComparator_API.Repositories.Implementations
{
    public class CassandraRepository : ICassandraRepository
    {
        private readonly Cassandra.ISession _session;
        private readonly int ChunkSize = 100;

        public CassandraRepository(Cassandra.ISession session)
        {
            _session = session;
        }

        // CREATE
        public async Task<Guid> CreateClientAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive)
        {
            var id = Guid.NewGuid();
            var stmt = new SimpleStatement(@"INSERT INTO clients (id, firstname, secondname, lastname, email, dateofbirth, address, phonenumber, isactive) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)", id, firstName, secondName, lastName, email, dob, address, phone, isActive);
            await _session.ExecuteAsync(stmt);
            return id;
        }

        public async Task<Guid> CreateRoomAsync(int number, int capacity, double pricePerNight, bool isActive)
        {
            var id = Guid.NewGuid();
            var stmt = new SimpleStatement(@"INSERT INTO rooms (id, number, capacity, pricepernight, isactive) VALUES (?, ?, ?, ?, ?)", id, number, capacity, pricePerNight, isActive);
            await _session.ExecuteAsync(stmt);
            return id;
        }

        public async Task<Guid> CreateServiceAsync(string name, int price, bool isActive)
        {
            var id = Guid.NewGuid();
            var stmt = new SimpleStatement(@"INSERT INTO services (id, name, price, isactive) VALUES (?, ?, ?, ?)", id, name, price, isActive);
            await _session.ExecuteAsync(stmt);
            return id;
        }

        public async Task<List<Guid>> CreateClientsAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive, int count)
        {
            var ids = new List<Guid>();
            var statements = new List<SimpleStatement>();
            for (int i = 0; i < count; i++)
            {
                var id = Guid.NewGuid();
                ids.Add(id);
                statements.Add(new SimpleStatement(@"INSERT INTO clients (id, firstname, secondname, lastname, email, dateofbirth, address, phonenumber, isactive) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)", id, firstName, secondName, lastName, email, dob, address, phone, isActive));
            }
            await ExecuteInChunksAsync(statements);
            return ids;
        }

        public async Task<List<Guid>> CreateRoomsAsync(int number, int capacity, double pricePerNight, bool isActive, int count)
        {
            var ids = new List<Guid>();
            var statements = new List<SimpleStatement>();
            for (int i = 0; i < count; i++)
            {
                var id = Guid.NewGuid();
                ids.Add(id);
                statements.Add(new SimpleStatement(@"INSERT INTO rooms (id, number, capacity, pricepernight, isactive) VALUES (?, ?, ?, ?, ?)", id, number, capacity, pricePerNight, isActive));
            }
            await ExecuteInChunksAsync(statements);
            return ids;
        }

        // READ
        public async Task<List<Dictionary<string, object>>> ReadReservationsAfterSecondHalf2025Async()
        {
            var result = new List<Dictionary<string, object>>();
            var stmt = new SimpleStatement(@"SELECT reservationid, checkindate, checkoutdate, clientid, roomid FROM reservations_by_client WHERE checkindate > ? ALLOW FILTERING", new DateTime(2025, 6, 30));
            var rs = await _session.ExecuteAsync(stmt);
            foreach (var row in rs)
            {
                result.Add(new Dictionary<string, object>
                {
                    ["ReservationId"] = row.GetValue<Guid>("reservationid"),
                    ["CheckInDate"] = row.GetValue<DateTime>("checkindate"),
                    ["CheckOutDate"] = row.GetValue<DateTime>("checkoutdate"),
                    ["ClientId"] = row.GetValue<Guid>("clientid"),
                    ["RoomId"] = row.GetValue<Guid>("roomid")
                });
            }
            return result;
        }

        public async Task<List<Dictionary<string, object>>> ReadReservationsWithPaymentsAboveAsync(int minSum)
        {
            var result = new List<Dictionary<string, object>>();
            var stmt = new SimpleStatement(@"SELECT reservationid, sum, creationdate FROM payments_by_reservation WHERE sum > ? ALLOW FILTERING", minSum);
            var rs = await _session.ExecuteAsync(stmt);
            foreach (var row in rs)
            {
                result.Add(new Dictionary<string, object>
                {
                    ["ReservationId"] = row.GetValue<Guid>("reservationid"),
                    ["Sum"] = row.GetValue<int>("sum"),
                    ["CreationDate"] = row.GetValue<DateTime>("creationdate")
                });
            }
            return result;
        }

        public async Task<List<Dictionary<string, object>>> ReadClientsWithActiveReservationsAsync()
        {
            var result = new List<Dictionary<string, object>>();
            var stmt = new SimpleStatement(@"SELECT id, firstname, lastname, isactive FROM clients WHERE isactive = true ALLOW FILTERING");
            var rs = await _session.ExecuteAsync(stmt);
            foreach (var row in rs)
            {
                result.Add(new Dictionary<string, object>
                {
                    ["Id"] = row.GetValue<Guid>("id"),
                    ["FirstName"] = row.GetValue<string>("firstname"),
                    ["LastName"] = row.GetValue<string>("lastname"),
                    ["IsActive"] = row.GetValue<bool>("isactive")
                });
            }
            return result;
        }

        public async Task<List<Dictionary<string, object>>> ReadActiveServicesUsedInReservationsAsync(int minSum)
        {
            var result = new List<Dictionary<string, object>>();
            var stmt = new SimpleStatement(@"SELECT id, name, price, isactive FROM services WHERE isactive = true AND price > ? ALLOW FILTERING", minSum);
            var rs = await _session.ExecuteAsync(stmt);
            foreach (var row in rs)
            {
                result.Add(new Dictionary<string, object>
                {
                    ["Id"] = row.GetValue<Guid>("id"),
                    ["Name"] = row.GetValue<string>("name"),
                    ["Price"] = row.GetValue<int>("price"),
                    ["IsActive"] = row.GetValue<bool>("isactive")
                });
            }
            return result;
        }

        public async Task<List<Dictionary<string, object>>> ReadCapacityReservationsAsync(int capacityThreshold)
        {
            var result = new List<Dictionary<string, object>>();
            var roomsStmt = new SimpleStatement(@"SELECT id FROM rooms WHERE capacity > ? ALLOW FILTERING", capacityThreshold);
            var roomsRs = await _session.ExecuteAsync(roomsStmt);
            var roomIds = roomsRs.Select(r => r.GetValue<Guid>("id")).ToList();

            foreach (var roomId in roomIds)
            {
                var resStmt = new SimpleStatement(@"SELECT reservationid, roomid FROM reservations_by_room WHERE roomid = ?", roomId);
                var resRs = await _session.ExecuteAsync(resStmt);
                foreach (var row in resRs)
                {
                    result.Add(new Dictionary<string, object>
                    {
                        ["ReservationId"] = row.GetValue<Guid>("reservationid"),
                        ["RoomId"] = row.GetValue<Guid>("roomid")
                    });
                }
            }
            return result;
        }

        // UPDATE
        public async Task<int> UpdateClientsAddressAndPhoneAsync(bool isActive, DateTime dateThreshold)
        {
            var stmt = new SimpleStatement(@"SELECT id FROM clients WHERE isactive = ? ALLOW FILTERING", isActive);
            var rs = await _session.ExecuteAsync(stmt);
            var statements = new List<SimpleStatement>();
            int updated = 0;
            foreach (var row in rs)
            {
                var id = row.GetValue<Guid>("id");
                statements.Add(new SimpleStatement(@"UPDATE clients SET address = 'Updated Address', phonenumber = 'Updated Phone' WHERE id = ?", id));
                updated++;
            }
            await ExecuteInChunksAsync(statements);
            return updated;
        }

        public async Task<int> UpdateRoomsPriceForReservationsAsync(int minCapacity, int priceIncrement)
        {
            var stmt = new SimpleStatement(@"SELECT id, pricepernight FROM rooms WHERE capacity > ? ALLOW FILTERING", minCapacity);
            var rs = await _session.ExecuteAsync(stmt);
            int updated = 0;
            foreach (var row in rs)
            {
                var id = row.GetValue<Guid>("id");
                var price = row.GetValue<int>("pricepernight") + priceIncrement;
                var updateStmt = new SimpleStatement(@"UPDATE rooms SET pricepernight = ? WHERE id = ?", price, id);
                await _session.ExecuteAsync(updateStmt);
                updated++;
            }
            return updated;
        }

        public async Task<int> UpdateServicesPriceAsync(int priceIncrement, bool isActive, int price)
        {
            var stmt = new SimpleStatement(@"SELECT id, price FROM services WHERE isactive = ? AND price = ? ALLOW FILTERING", isActive, price);
            var rs = await _session.ExecuteAsync(stmt);
            int updated = 0;
            foreach (var row in rs)
            {
                var id = row.GetValue<Guid>("id");
                var newPrice = row.GetValue<int>("price") + priceIncrement;
                var updateStmt = new SimpleStatement(@"UPDATE services SET price = ? WHERE id = ?", newPrice, id);
                await _session.ExecuteAsync(updateStmt);
                updated++;
            }
            return updated;
        }

        public async Task<int> UpdatePriceForInactiveRoomsAsync(double discountMultiplier, double pricePerNight)
        {
            var stmt = new SimpleStatement(@"SELECT id, pricepernight FROM rooms WHERE isactive = false AND pricepernight = ? ALLOW FILTERING", pricePerNight);
            var rs = await _session.ExecuteAsync(stmt);
            int updated = 0;
            foreach (var row in rs)
            {
                var id = row.GetValue<Guid>("id");
                var oldPrice = row.GetValue<double>("pricepernight");
                var newPrice = oldPrice * discountMultiplier;
                var updateStmt = new SimpleStatement(@"UPDATE rooms SET pricepernight = ? WHERE id = ?", newPrice, id);
                await _session.ExecuteAsync(updateStmt);
                updated++;
            }
            return updated;
        }

        public async Task<int> UpdateRoomsPriceForReservationsToApril2024Async(int priceDecrement)
        {
            var stmt = new SimpleStatement(@"SELECT roomid FROM reservations_by_room WHERE checkindate < ? ALLOW FILTERING", new DateTime(2024, 4, 1));
            var rs = await _session.ExecuteAsync(stmt);
            var roomIds = new HashSet<Guid>();
            foreach (var row in rs)
                roomIds.Add(row.GetValue<Guid>("roomid"));

            if (roomIds.Count == 0) return 0;

            var statements = new List<SimpleStatement>();
            int updated = 0;
            const int inChunkSize = 500;
            var roomIdList = roomIds.ToList();
            for (int i = 0; i < roomIdList.Count; i += inChunkSize)
            {
                var chunk = roomIdList.Skip(i).Take(inChunkSize).ToList();
                var inClause = string.Join(",", chunk.Select(id => $"{id}"));
                var getRoomsStmt = new SimpleStatement($"SELECT id, pricepernight FROM rooms WHERE id IN ({inClause})");
                var roomsRs = await _session.ExecuteAsync(getRoomsStmt);
                foreach (var roomRow in roomsRs)
                {
                    var id = roomRow.GetValue<Guid>("id");
                    var price = roomRow.GetValue<double>("pricepernight");
                    var newPrice = price - priceDecrement;
                    statements.Add(new SimpleStatement(@"UPDATE rooms SET pricepernight = ? WHERE id = ?", newPrice, id));
                    updated++;
                }
            }
            await ExecuteInChunksAsync(statements);
            return updated;
        }

        // DELETE
        public async Task<int> DeletePaymentsOlderThanMarch2024Async()
        {
            var stmt = new SimpleStatement(@"SELECT reservationid, paymentid, creationdate FROM payments_by_reservation WHERE creationdate < ? ALLOW FILTERING", new DateTime(2024, 3, 1));
            var rs = await _session.ExecuteAsync(stmt);
            int deleted = 0;
            foreach (var row in rs)
            {
                var reservationId = row.GetValue<Guid>("reservationid");
                var paymentId = row.GetValue<Guid>("paymentid");
                var creationDate = row.GetValue<DateTime>("creationdate");
                var delStmt = new SimpleStatement(@"DELETE FROM payments_by_reservation WHERE reservationid = ? AND creationdate = ? AND paymentid = ?", reservationId, creationDate, paymentId);
                await _session.ExecuteAsync(delStmt);
                deleted++;
            }
            return deleted;
        }

        public async Task<int> DeletePaymentsToSumAsync(int sum)
        {
            var stmt = new SimpleStatement(@"SELECT reservationid, paymentid, creationdate, sum FROM payments_by_reservation WHERE sum = ? ALLOW FILTERING", sum);
            var rs = await _session.ExecuteAsync(stmt);
            int deleted = 0;
            foreach (var row in rs)
            {
                var reservationId = row.GetValue<Guid>("reservationid");
                var paymentId = row.GetValue<Guid>("paymentid");
                var creationDate = row.GetValue<DateTime>("creationdate");
                var delStmt = new SimpleStatement(@"DELETE FROM payments_by_reservation WHERE reservationid = ? AND creationdate = ? AND paymentid = ?", reservationId, creationDate, paymentId);
                await _session.ExecuteAsync(delStmt);
                deleted++;
            }
            return deleted;
        }

        public async Task<int> DeleteReservationsServicesOlderThanMarch2023Async()
        {
            var stmt = new SimpleStatement(@"SELECT reservationid, serviceid, creationdate FROM reservations_services_by_reservation WHERE creationdate < ? ALLOW FILTERING", new DateTime(2023, 3, 1));
            var rs = await _session.ExecuteAsync(stmt);
            int deleted = 0;
            foreach (var row in rs)
            {
                var reservationId = row.GetValue<Guid>("reservationid");
                var serviceId = row.GetValue<Guid>("serviceid");
                var delStmt = new SimpleStatement(@"DELETE FROM reservations_services_by_reservation WHERE reservationid = ? AND serviceid = ?", reservationId, serviceId);
                await _session.ExecuteAsync(delStmt);
                deleted++;
            }
            return deleted;
        }

        public async Task<int> DeleteReservationsServicesWithServicePriceBelowAsync(int price)
        {
            var serviceIds = new List<Guid>();
            var serviceStmt = new SimpleStatement("SELECT id FROM services WHERE price < ? ALLOW FILTERING", price);
            var serviceRs = await _session.ExecuteAsync(serviceStmt);
            foreach (var row in serviceRs)
                serviceIds.Add(row.GetValue<Guid>("id"));

            int deleted = 0;
            foreach (var serviceId in serviceIds)
            {
                var selectStmt = new SimpleStatement("SELECT reservationid FROM reservations_services_by_service WHERE serviceid = ?", serviceId);
                var rs = await _session.ExecuteAsync(selectStmt);
                foreach (var row in rs)
                {
                    var reservationId = row.GetValue<Guid>("reservationid");
                    var delStmt = new SimpleStatement("DELETE FROM reservations_services_by_service WHERE serviceid = ? AND reservationid = ?", serviceId, reservationId);
                    await _session.ExecuteAsync(delStmt);
                    deleted++;
                }
            }
            return deleted;
        }

        public async Task<int> DeleteUnusedServicesPriceBelowAsync(int price)
        {
            var stmt = new SimpleStatement(@"SELECT id FROM services WHERE price < ? ALLOW FILTERING", price);
            var rs = await _session.ExecuteAsync(stmt);
            int deleted = 0;
            foreach (var row in rs)
            {
                var id = row.GetValue<Guid>("id");
                var delStmt = new SimpleStatement(@"DELETE FROM services WHERE id = ?", id);
                await _session.ExecuteAsync(delStmt);
                deleted++;
            }
            return deleted;
        }

        // HELPERS

        public async Task InsertClientsBatchAsync(IEnumerable<CassandraClientDTO> clients)
        {
            var statements = clients.Select(c =>
                new SimpleStatement(@"
                    INSERT INTO clients (id, firstname, secondname, lastname, email, dateofbirth, address, phonenumber, isactive)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                    c.Id, c.FirstName, c.SecondName, c.LastName, c.Email, c.DateOfBirth, c.Address, c.PhoneNumber, c.IsActive));

            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertActiveClientsBatchAsync(IEnumerable<CassandraActiveClientDTO> clients)
        {
            var statements = clients.Select(c =>
                new SimpleStatement(@"
                    INSERT INTO active_clients_by_status (isactive, id, firstname, lastname, email, dateofbirth, address, phonenumber)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                    c.IsActive, c.Id, c.FirstName, c.LastName, c.Email, c.DateOfBirth, c.Address, c.PhoneNumber));

            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertRoomsBatchAsync(IEnumerable<CassandraRoomDTO> rooms)
        {
            var statements = rooms.Select(r =>
                new SimpleStatement(@"
                    INSERT INTO rooms (id, number, capacity, pricepernight, isactive)
                    VALUES (?, ?, ?, ?, ?)",
                    r.Id, r.Number, r.Capacity, r.PricePerNight, r.IsActive));

            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertActiveRoomsBatchAsync(IEnumerable<CassandraActiveRoomDTO> rooms)
        {
            var statements = rooms.Select(r =>
                new SimpleStatement(@"
                    INSERT INTO active_rooms_by_status (isactive, id, number, capacity, pricepernight)
                    VALUES (?, ?, ?, ?, ?)",
                    r.IsActive, r.Id, r.Number, r.Capacity, r.PricePerNight));

            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertServicesBatchAsync(IEnumerable<CassandraServiceDTO> services)
        {
            var statements = services.Select(s =>
                new SimpleStatement(@"
                    INSERT INTO services (id, name, price, isactive)
                    VALUES (?, ?, ?, ?)",
                    s.Id, s.Name, s.Price, s.IsActive));

            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertActiveServicesBatchAsync(IEnumerable<CassandraActiveServiceDTO> services)
        {
            var statements = services.Select(s =>
                new SimpleStatement(@"
                    INSERT INTO active_services_by_price (isactive, price, id, name)
                    VALUES (?, ?, ?, ?)",
                    s.IsActive, s.Price, s.Id, s.Name));

            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertReservationsByClientBatchAsync(IEnumerable<CassandraReservationByClientDTO> reservations)
        {
            var statements = reservations.Select(r =>
                new SimpleStatement(@"
                    INSERT INTO reservations_by_client (clientid, creationdate, reservationid, roomid, checkindate, checkoutdate)
                    VALUES (?, ?, ?, ?, ?, ?)",
                    r.ClientId, r.CreationDate, r.ReservationId, r.RoomId, r.CheckInDate, r.CheckOutDate));

            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertReservationsByRoomBatchAsync(IEnumerable<CassandraReservationByRoomDTO> reservations)
        {
            var statements = reservations.Select(r =>
                new SimpleStatement(@"
                    INSERT INTO reservations_by_room (roomid, creationdate, reservationid, clientid, checkindate, checkoutdate)
                    VALUES (?, ?, ?, ?, ?, ?)",
                    r.RoomId, r.CreationDate, r.ReservationId, r.ClientId, r.CheckInDate, r.CheckOutDate));

            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertPaymentsByReservationBatchAsync(IEnumerable<CassandraPaymentDTO> payments)
        {
            var statements = payments.Select(p =>
                new SimpleStatement(@"
                    INSERT INTO payments_by_reservation (reservationid, creationdate, paymentid, description, sum)
                    VALUES (?, ?, ?, ?, ?)",
                    p.ReservationId, p.CreationDate, p.PaymentId, p.Description, p.Sum));

            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertReservationServicesByReservationBatchAsync(IEnumerable<CassandraReservationServiceByReservationDTO> items)
        {
            var statements = items.Select(rs =>
                new SimpleStatement(@"
                    INSERT INTO reservations_services_by_reservation (reservationid, serviceid, creationdate)
                    VALUES (?, ?, ?)",
                    rs.ReservationId, rs.ServiceId, rs.CreationDate));

            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertReservationServicesByServiceBatchAsync(IEnumerable<CassandraReservationServiceByServiceDTO> items)
        {
            var statements = items.Select(rs =>
                new SimpleStatement(@"
                    INSERT INTO reservations_services_by_service (serviceid, reservationid, creationdate)
                    VALUES (?, ?, ?)",
                    rs.ServiceId, rs.ReservationId, rs.CreationDate));

            await ExecuteInChunksAsync(statements);
        }

        public async Task<TablesCountDTO> GetTablesCountAsync()
        {
            async Task<long> CountAsync(string tableName)
            {
                var rs = await _session.ExecuteAsync(new SimpleStatement($"SELECT COUNT(*) FROM {tableName};"));
                return rs.FirstOrDefault()?.GetValue<long>("count") ?? 0;
            }

            var clients = await CountAsync("clients");
            var rooms = await CountAsync("rooms");
            var reservations = await CountAsync("reservations_by_client");
            var payments = await CountAsync("payments_by_reservation");
            var services = await CountAsync("services");
            var resServices = await CountAsync("reservations_services_by_reservation");

            return new TablesCountDTO
            {
                ClientsCount = (int)clients,
                RoomsCount = (int)rooms,
                ReservationsCount = (int)reservations,
                PaymentsCount = (int)payments,
                ServicesCount = (int)services,
                ReservationsServicesCount = (int)resServices
            };
        }

        public async Task DeleteAllAsync()
        {
            var tables = new[]
            {
                "clients",
                "active_clients_by_status",
                "rooms",
                "active_rooms_by_status",
                "services",
                "active_services_by_price",
                "reservations_by_client",
                "reservations_by_room",
                "payments_by_reservation",
                "reservations_services_by_reservation",
                "reservations_services_by_service"
            };

            foreach (var table in tables)
                await _session.ExecuteAsync(new SimpleStatement($"TRUNCATE {table};"));
        }

        private async Task ExecuteInChunksAsync(IEnumerable<SimpleStatement> statements)
        {
            foreach (var chunk in Chunk(statements, ChunkSize))
            {
                var batch = new BatchStatement();
                foreach (var stmt in chunk)
                    batch.Add(stmt);

                await _session.ExecuteAsync(batch);
            }
        }

        private static IEnumerable<IEnumerable<T>> Chunk<T>(IEnumerable<T> source, int size)
        {
            var chunk = new List<T>(size);
            foreach (var item in source)
            {
                chunk.Add(item);
                if (chunk.Count == size)
                {
                    yield return chunk.ToArray();
                    chunk.Clear();
                }
            }
            if (chunk.Count > 0)
                yield return chunk.ToArray();
        }
    }
}