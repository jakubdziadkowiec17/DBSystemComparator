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
            var statement = new SimpleStatement(@"INSERT INTO clients (id, firstname, secondname, lastname, email, dateofbirth, address, phonenumber, isactive) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)", id, firstName, secondName, lastName, email, dob, address, phone, isActive);
            await _session.ExecuteAsync(statement);

            var statementActive = new SimpleStatement(@"INSERT INTO active_clients_by_status (isactive, id, firstname, lastname, email, dateofbirth, address, phonenumber) VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                isActive, id, firstName, lastName, email, dob, address, phone);
            await _session.ExecuteAsync(statementActive);
            return id;
        }

        public async Task<Guid> CreateRoomAsync(int number, int capacity, double pricePerNight, bool isActive)
        {
            var id = Guid.NewGuid();
            var statement = new SimpleStatement(@"INSERT INTO rooms (id, number, capacity, pricepernight, isactive) VALUES (?, ?, ?, ?, ?)", id, number, capacity, pricePerNight, isActive);
            await _session.ExecuteAsync(statement);

            var statementActive = new SimpleStatement(@"INSERT INTO active_rooms_by_status (isactive, id, number, capacity, pricepernight) VALUES (?, ?, ?, ?, ?)",
                isActive, id, number, capacity, pricePerNight);
            await _session.ExecuteAsync(statementActive);
            return id;
        }

        public async Task<Guid> CreateServiceAsync(string name, int price, bool isActive)
        {
            var id = Guid.NewGuid();
            var statement = new SimpleStatement(@"INSERT INTO services (id, name, price, isactive) VALUES (?, ?, ?, ?)", id, name, price, isActive);
            await _session.ExecuteAsync(statement);

            var statementActive = new SimpleStatement(@"INSERT INTO active_services_by_price (isactive, price, id, name) VALUES (?, ?, ?, ?)",
                isActive, price, id, name);
            await _session.ExecuteAsync(statementActive);
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
                statements.Add(new SimpleStatement(@"INSERT INTO active_clients_by_status (isactive, id, firstname, lastname, email, dateofbirth, address, phonenumber) VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                    isActive, id, firstName, lastName, email, dob, address, phone));
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
                statements.Add(new SimpleStatement(@"INSERT INTO active_rooms_by_status (isactive, id, number, capacity, pricepernight) VALUES (?, ?, ?, ?, ?)",
                    isActive, id, number, capacity, pricePerNight));
            }
            await ExecuteInChunksAsync(statements);
            return ids;
        }

        // READ
        public async Task<List<Dictionary<string, object>>> ReadReservationsAfterSecondHalf2025Async()
        {
            var result = new List<Dictionary<string, object>>();
            var statement = new SimpleStatement(@"SELECT reservationid, checkindate, checkoutdate, clientid, roomid FROM reservations_by_client WHERE checkindate > ? ALLOW FILTERING", new DateTime(2025, 6, 30));
            var rs = await _session.ExecuteAsync(statement);
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
            var statement = new SimpleStatement(@"SELECT reservationid, sum, creationdate FROM payments_by_reservation WHERE sum > ? ALLOW FILTERING", minSum);
            var rs = await _session.ExecuteAsync(statement);
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
            var statement = new SimpleStatement(@"SELECT id, firstname, lastname, isactive FROM active_clients_by_status WHERE isactive = true");
            var rs = await _session.ExecuteAsync(statement);
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
            var statement = new SimpleStatement(@"SELECT id, name, price, isactive FROM active_services_by_price WHERE isactive = true AND price > ?", minSum);
            var rs = await _session.ExecuteAsync(statement);
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

            var roomsStmt = new SimpleStatement(@"SELECT id, number, capacity FROM rooms WHERE capacity > ? ALLOW FILTERING", capacityThreshold);
            var roomsRs = await _session.ExecuteAsync(roomsStmt);
            var rooms = roomsRs
                .Select(r => new
                {
                    Id = r.GetValue<Guid>("id"),
                    Number = r.GetValue<int>("number"),
                    Capacity = r.GetValue<int>("capacity")
                })
                .ToList();

            if (rooms.Count == 0)
                return result;

            var roomMeta = rooms.ToDictionary(x => x.Id, x => (x.Number, x.Capacity));

            var reservations = new List<(Guid ReservationId, Guid RoomId, Guid ClientId, DateTime CheckIn, DateTime CheckOut)>();
            foreach (var roomChunk in Chunk(rooms, ChunkSize))
            {
                var roomIds = roomChunk.Select(r => (object)r.Id).ToList();
                var placeholders = string.Join(", ", Enumerable.Repeat("?", roomIds.Count));
                var stmt = new SimpleStatement($"SELECT roomid, reservationid, clientid, checkindate, checkoutdate FROM reservations_by_room WHERE roomid IN ({placeholders})", roomIds.ToArray());
                var rs = await _session.ExecuteAsync(stmt);
                foreach (var row in rs)
                {
                    reservations.Add((
                        row.GetValue<Guid>("reservationid"),
                        row.GetValue<Guid>("roomid"),
                        row.GetValue<Guid>("clientid"),
                        row.GetValue<DateTime>("checkindate"),
                        row.GetValue<DateTime>("checkoutdate")
                    ));
                }
            }

            if (reservations.Count == 0)
                return result;

            var uniqueClientIds = reservations.Select(r => r.ClientId).Distinct().ToList();
            var clientMap = new Dictionary<Guid, (string FirstName, string LastName)>(uniqueClientIds.Count);

            const int inChunkSize = 500;
            for (int i = 0; i < uniqueClientIds.Count; i += inChunkSize)
            {
                var chunk = uniqueClientIds.Skip(i).Take(inChunkSize).ToList();
                var placeholders = string.Join(", ", Enumerable.Repeat("?", chunk.Count));
                var stmt = new SimpleStatement($"SELECT id, firstname, lastname FROM clients WHERE id IN ({placeholders})", chunk.Cast<object>().ToArray());
                var rs = await _session.ExecuteAsync(stmt);
                foreach (var row in rs)
                {
                    var id = row.GetValue<Guid>("id");
                    var first = row.GetValue<string>("firstname");
                    var last = row.GetValue<string>("lastname");
                    clientMap[id] = (first, last);
                }
            }

            foreach (var r in reservations)
            {
                clientMap.TryGetValue(r.ClientId, out var name);
                var (roomNumber, roomCapacity) = roomMeta.TryGetValue(r.RoomId, out var meta) ? meta : (0, 0);

                result.Add(new Dictionary<string, object>
                {
                    ["ReservationId"] = r.ReservationId,
                    ["CheckInDate"] = r.CheckIn,
                    ["CheckOutDate"] = r.CheckOut,
                    ["FirstName"] = name.FirstName,
                    ["LastName"] = name.LastName,
                    ["RoomNumber"] = roomNumber,
                    ["Capacity"] = roomCapacity
                });
            }

            return result;
        }

        // UPDATE
        public async Task<int> UpdateClientsAddressAndPhoneAsync(bool isActive, DateTime dateThreshold)
        {
            var selectStmt = new SimpleStatement(@"SELECT id FROM active_clients_by_status WHERE isactive = ? AND dateofbirth > ? ALLOW FILTERING", isActive, dateThreshold);
            var rs = await _session.ExecuteAsync(selectStmt);

            var statements = new List<SimpleStatement>();
            int updated = 0;

            foreach (var row in rs)
            {
                var id = row.GetValue<Guid>("id");

                statements.Add(new SimpleStatement(@"UPDATE clients SET address = ?, phonenumber = ? WHERE id = ?",
                    "Cracow, ul. abc 4", "123456789", id));
                statements.Add(new SimpleStatement(@"UPDATE active_clients_by_status SET address = ?, phonenumber = ? WHERE isactive = ? AND id = ?",
                    "Cracow, ul. abc 4", "123456789", isActive, id));

                updated++;
            }

            await ExecuteInChunksAsync(statements);
            return updated;
        }

        public async Task<int> UpdateRoomsPriceForReservationsAsync(int minCapacity, int priceIncrement)
        {
            var statement = new SimpleStatement(@"SELECT id, pricepernight, isactive FROM rooms WHERE capacity > ? ALLOW FILTERING", minCapacity);
            var rs = await _session.ExecuteAsync(statement);
            int updated = 0;
            foreach (var row in rs)
            {
                var id = row.GetValue<Guid>("id");
                var isActive = row.GetValue<bool>("isactive");
                var price = row.GetValue<int>("pricepernight") + priceIncrement;
                var updateStmt = new SimpleStatement(@"UPDATE rooms SET pricepernight = ? WHERE id = ?", price, id);
                await _session.ExecuteAsync(updateStmt);
                var updateActive = new SimpleStatement(@"UPDATE active_rooms_by_status SET pricepernight = ? WHERE isactive = ? AND id = ?", price, isActive, id);
                await _session.ExecuteAsync(updateActive);
                updated++;
            }
            return updated;
        }

        public async Task<int> UpdateServicesPriceAsync(int priceIncrement, bool isActive, int price)
        {
            var statement = new SimpleStatement(@"SELECT id, price, name FROM services WHERE isactive = ? AND price = ? ALLOW FILTERING", isActive, price);
            var rs = await _session.ExecuteAsync(statement);
            int updated = 0;
            foreach (var row in rs)
            {
                var id = row.GetValue<Guid>("id");
                var newPrice = row.GetValue<int>("price") + priceIncrement;
                var name = row.GetValue<string>("name");
                var updateStmt = new SimpleStatement(@"UPDATE services SET price = ? WHERE id = ?", newPrice, id);
                await _session.ExecuteAsync(updateStmt);
                var deleteOld = new SimpleStatement(@"DELETE FROM active_services_by_price WHERE isactive = ? AND price = ? AND id = ?", isActive, price, id);
                var insertNew = new SimpleStatement(@"INSERT INTO active_services_by_price (isactive, price, id, name) VALUES (?, ?, ?, ?)", isActive, newPrice, id, name);
                await _session.ExecuteAsync(deleteOld);
                await _session.ExecuteAsync(insertNew);
                updated++;
            }
            return updated;
        }

        public async Task<int> UpdatePriceForInactiveRoomsAsync(double discountMultiplier, double pricePerNight)
        {
            var statement = new SimpleStatement(@"SELECT id, pricepernight FROM rooms WHERE isactive = false AND pricepernight = ? ALLOW FILTERING", pricePerNight);
            var rs = await _session.ExecuteAsync(statement);
            int updated = 0;
            foreach (var row in rs)
            {
                var id = row.GetValue<Guid>("id");
                var oldPrice = row.GetValue<double>("pricepernight");
                var newPrice = oldPrice * discountMultiplier;
                var updateStmt = new SimpleStatement(@"UPDATE rooms SET pricepernight = ? WHERE id = ?", newPrice, id);
                await _session.ExecuteAsync(updateStmt);
                var updateActive = new SimpleStatement(@"UPDATE active_rooms_by_status SET pricepernight = ? WHERE isactive = false AND id = ?", newPrice, id);
                await _session.ExecuteAsync(updateActive);
                updated++;
            }
            return updated;
        }

        public async Task<int> UpdateRoomsPriceForReservationsToApril2024Async(int priceDecrement)
        {
            var statement = new SimpleStatement(@"SELECT roomid FROM reservations_by_room WHERE checkindate < ? ALLOW FILTERING", new DateTime(2023, 4, 1));
            var rs = await _session.ExecuteAsync(statement);
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
                var placeholders = string.Join(", ", Enumerable.Repeat("?", chunk.Count));
                var getRoomsStmt = new SimpleStatement($"SELECT id, pricepernight, isactive FROM rooms WHERE id IN ({placeholders})", chunk.Cast<object>().ToArray());
                var roomsRs = await _session.ExecuteAsync(getRoomsStmt);
                foreach (var roomRow in roomsRs)
                {
                    var id = roomRow.GetValue<Guid>("id");
                    var price = roomRow.GetValue<double>("pricepernight");
                    var newPrice = price - priceDecrement;
                    statements.Add(new SimpleStatement(@"UPDATE rooms SET pricepernight = ? WHERE id = ?", newPrice, id));
                    var isActive = roomRow.GetValue<bool>("isactive");
                    statements.Add(new SimpleStatement(@"UPDATE active_rooms_by_status SET pricepernight = ? WHERE isactive = ? AND id = ?", newPrice, isActive, id));
                    updated++;
                }
            }
            await ExecuteInChunksAsync(statements);
            return updated;
        }

        // DELETE
        public async Task<int> DeletePaymentsOlderThanMarch2024Async()
        {
            var threshold = new DateTime(2023, 3, 1);
            var roomsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT id FROM rooms"));
            var roomIds = roomsRs.Select(r => r.GetValue<Guid>("id")).ToList();
            if (roomIds.Count == 0)
                return 0;

            var reservationIds = new HashSet<Guid>();
            foreach (var roomChunk in Chunk(roomIds, ChunkSize))
            {
                var chunkList = roomChunk.ToList();
                var placeholders = string.Join(", ", Enumerable.Repeat("?", chunkList.Count));
                var args = new List<object>(chunkList.Cast<object>()) { threshold };
                var resRs = await _session.ExecuteAsync(new SimpleStatement(
                    $"SELECT reservationid FROM reservations_by_room WHERE roomid IN ({placeholders}) AND checkindate < ? ALLOW FILTERING",
                    args.ToArray()));
                foreach (var row in resRs)
                    reservationIds.Add(row.GetValue<Guid>("reservationid"));
            }

            if (reservationIds.Count == 0)
                return 0;

            var deleteStatements = new List<SimpleStatement>(reservationIds.Count);
            foreach (var reservationId in reservationIds)
            {
                deleteStatements.Add(new SimpleStatement(
                    "DELETE FROM payments_by_reservation WHERE reservationid = ?",
                    reservationId));
            }

            await ExecuteInChunksAsync(deleteStatements);

            return deleteStatements.Count;
        }

        public async Task<int> DeletePaymentsToSumAsync(int sum)
        {
            var select = new SimpleStatement(
                @"SELECT reservationid, creationdate, paymentid FROM payments_by_reservation WHERE sum < ? ALLOW FILTERING",
                sum);

            select.SetPageSize(5000);

            var rs = await _session.ExecuteAsync(select);

            int deleted = 0;
            var deleteBuffer = new List<SimpleStatement>(1000);

            foreach (var row in rs)
            {
                var reservationId = row.GetValue<Guid>("reservationid");
                var creationDate = row.GetValue<DateTime>("creationdate");
                var paymentId = row.GetValue<Guid>("paymentid");

                deleteBuffer.Add(new SimpleStatement(
                    @"DELETE FROM payments_by_reservation WHERE reservationid = ? AND creationdate = ? AND paymentid = ?",
                    reservationId, creationDate, paymentId));

                if (deleteBuffer.Count >= 1000)
                {
                    await ExecuteInChunksAsync(deleteBuffer);
                    deleted += deleteBuffer.Count;
                    deleteBuffer.Clear();
                }
            }

            if (deleteBuffer.Count > 0)
            {
                await ExecuteInChunksAsync(deleteBuffer);
                deleted += deleteBuffer.Count;
                deleteBuffer.Clear();
            }

            return deleted;
        }

        public async Task<int> DeleteReservationsServicesOlderThanMarch2023Async()
        {
            var threshold = new DateTime(2023, 3, 1);

            var roomsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT id FROM rooms"));
            var roomIds = roomsRs.Select(r => r.GetValue<Guid>("id")).ToList();
            if (roomIds.Count == 0)
                return 0;

            var reservationIds = new HashSet<Guid>();
            foreach (var roomChunk in Chunk(roomIds, ChunkSize))
            {
                var chunkList = roomChunk.ToList();
                var placeholders = string.Join(", ", Enumerable.Repeat("?", chunkList.Count));
                var args = new List<object>(chunkList.Cast<object>()) { threshold };
                var resStmt = new SimpleStatement(
                    $"SELECT reservationid FROM reservations_by_room WHERE roomid IN ({placeholders}) AND checkindate < ? ALLOW FILTERING",
                    args.ToArray());
                resStmt.SetPageSize(5000);
                var resRs = await _session.ExecuteAsync(resStmt);
                foreach (var row in resRs)
                    reservationIds.Add(row.GetValue<Guid>("reservationid"));
            }

            if (reservationIds.Count == 0)
                return 0;

            int deleted = 0;
            var deleteStatements = new List<SimpleStatement>();

            const int resChunkSize = 500;
            var reservationsList = reservationIds.ToList();
            for (int i = 0; i < reservationsList.Count; i += resChunkSize)
            {
                var chunk = reservationsList.Skip(i).Take(resChunkSize).ToList();

                var placeholders = string.Join(", ", Enumerable.Repeat("?", chunk.Count));
                var selectRsStmt = new SimpleStatement(
                    $"SELECT reservationid, serviceid FROM reservations_services_by_reservation WHERE reservationid IN ({placeholders})",
                    chunk.Cast<object>().ToArray());
                selectRsStmt.SetPageSize(5000);
                var mapRows = await _session.ExecuteAsync(selectRsStmt);

                var seenReservations = new HashSet<Guid>();
                foreach (var row in mapRows)
                {
                    var resId = row.GetValue<Guid>("reservationid");
                    var serviceId = row.GetValue<Guid>("serviceid");

                    deleteStatements.Add(new SimpleStatement(
                        "DELETE FROM reservations_services_by_service WHERE serviceid = ? AND reservationid = ?",
                        serviceId, resId));

                    deleted++;

                    if (deleteStatements.Count >= 1000)
                    {
                        await ExecuteInChunksAsync(deleteStatements);
                        deleteStatements.Clear();
                    }

                    if (seenReservations.Add(resId))
                    {
                        deleteStatements.Add(new SimpleStatement(
                            "DELETE FROM reservations_services_by_reservation WHERE reservationid = ?",
                            resId));
                    }
                }

                if (deleteStatements.Count > 0)
                {
                    await ExecuteInChunksAsync(deleteStatements);
                    deleteStatements.Clear();
                }
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

            if (serviceIds.Count == 0)
                return 0;

            int deleted = 0;
            var deleteStatements = new List<SimpleStatement>();

            const int serviceChunk = 200;
            for (int i = 0; i < serviceIds.Count; i += serviceChunk)
            {
                var chunk = serviceIds.Skip(i).Take(serviceChunk).ToList();

                var tasks = chunk.Select(sid =>
                    _session.ExecuteAsync(new SimpleStatement(
                        "SELECT reservationid FROM reservations_services_by_service WHERE serviceid = ?",
                        sid))).ToList();

                var results = await Task.WhenAll(tasks);

                for (int j = 0; j < results.Length; j++)
                {
                    var sid = chunk[j];
                    var rs = results[j];
                    foreach (var row in rs)
                    {
                        var reservationId = row.GetValue<Guid>("reservationid");
                        deleteStatements.Add(new SimpleStatement(
                            "DELETE FROM reservations_services_by_service WHERE serviceid = ? AND reservationid = ?",
                            sid, reservationId));
                        deleteStatements.Add(new SimpleStatement(
                            "DELETE FROM reservations_services_by_reservation WHERE reservationid = ? AND serviceid = ?",
                            reservationId, sid));
                        deleted++;
                    }
                }

                if (deleteStatements.Count > 0)
                {
                    await ExecuteInChunksAsync(deleteStatements);
                    deleteStatements.Clear();
                }
            }

            return deleted;
        }

        public async Task<int> DeleteUnusedServicesPriceBelowAsync(int price)
        {
            var candidates = new List<(Guid Id, bool IsActive, int Price)>();

            foreach (var active in new[] { true, false })
            {
                var stmt = new SimpleStatement(
                    "SELECT id, price FROM active_services_by_price WHERE isactive = ? AND price < ?",
                    active, price);
                var rs = await _session.ExecuteAsync(stmt);
                foreach (var row in rs)
                {
                    candidates.Add((row.GetValue<Guid>("id"), active, row.GetValue<int>("price")));
                }
            }

            if (candidates.Count == 0)
                return 0;

            int deleted = 0;
            var deleteStatements = new List<SimpleStatement>();

            const int serviceChunk = 200;
            for (int i = 0; i < candidates.Count; i += serviceChunk)
            {
                var chunk = candidates.Skip(i).Take(serviceChunk).ToList();

                var fetchTasks = chunk.Select(c =>
                    _session.ExecuteAsync(new SimpleStatement(
                        "SELECT reservationid FROM reservations_services_by_service WHERE serviceid = ? LIMIT 1",
                        c.Id))).ToList();

                var results = await Task.WhenAll(fetchTasks);

                for (int j = 0; j < results.Length; j++)
                {
                    var rs = results[j];
                    if (!rs.Any())
                    {
                        var (serviceId, isActive, currPrice) = chunk[j];

                        deleteStatements.Add(new SimpleStatement(
                            "DELETE FROM active_services_by_price WHERE isactive = ? AND price = ? AND id = ?",
                            isActive, currPrice, serviceId));
                        deleteStatements.Add(new SimpleStatement(
                            "DELETE FROM services WHERE id = ?",
                            serviceId));
                        deleted++;
                    }
                }

                if (deleteStatements.Count > 0)
                {
                    await ExecuteInChunksAsync(deleteStatements);
                    deleteStatements.Clear();
                }
            }

            return deleted;
        }

        // HELPERS

        public async Task InsertClientsBatchAsync(IEnumerable<CassandraClientDTO> clients)
        {
            var statements = new List<SimpleStatement>();
            foreach (var c in clients)
            {
                statements.Add(new SimpleStatement(@"
                    INSERT INTO clients (id, firstname, secondname, lastname, email, dateofbirth, address, phonenumber, isactive)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                    c.Id, c.FirstName, c.SecondName, c.LastName, c.Email, c.DateOfBirth, c.Address, c.PhoneNumber, c.IsActive));
            }
            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertActiveClientsBatchAsync(IEnumerable<CassandraActiveClientDTO> clients)
        {
            var statements = new List<SimpleStatement>();
            foreach (var c in clients)
            {
                statements.Add(new SimpleStatement(@"
                    INSERT INTO active_clients_by_status (isactive, id, firstname, lastname, email, dateofbirth, address, phonenumber)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                    c.IsActive, c.Id, c.FirstName, c.LastName, c.Email, c.DateOfBirth, c.Address, c.PhoneNumber));
            }
            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertRoomsBatchAsync(IEnumerable<CassandraRoomDTO> rooms)
        {
            var statements = new List<SimpleStatement>();
            foreach (var r in rooms)
            {
                statements.Add(new SimpleStatement(@"
                    INSERT INTO rooms (id, number, capacity, pricepernight, isactive)
                    VALUES (?, ?, ?, ?, ?)",
                    r.Id, r.Number, r.Capacity, r.PricePerNight, r.IsActive));
            }
            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertActiveRoomsBatchAsync(IEnumerable<CassandraActiveRoomDTO> rooms)
        {
            var statements = new List<SimpleStatement>();
            foreach (var r in rooms)
            {
                statements.Add(new SimpleStatement(@"
                    INSERT INTO active_rooms_by_status (isactive, id, number, capacity, pricepernight)
                    VALUES (?, ?, ?, ?, ?)",
                    r.IsActive, r.Id, r.Number, r.Capacity, r.PricePerNight));
            }
            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertServicesBatchAsync(IEnumerable<CassandraServiceDTO> services)
        {
            var statements = new List<SimpleStatement>();
            foreach (var s in services)
            {
                statements.Add(new SimpleStatement(@"
                    INSERT INTO services (id, name, price, isactive)
                    VALUES (?, ?, ?, ?)",
                    s.Id, s.Name, s.Price, s.IsActive));
            }
            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertActiveServicesBatchAsync(IEnumerable<CassandraActiveServiceDTO> services)
        {
            var statements = new List<SimpleStatement>();
            foreach (var s in services)
            {
                statements.Add(new SimpleStatement(@"
                    INSERT INTO active_services_by_price (isactive, price, id, name)
                    VALUES (?, ?, ?, ?)",
                    s.IsActive, s.Price, s.Id, s.Name));
            }
            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertReservationsByClientBatchAsync(IEnumerable<CassandraReservationByClientDTO> reservations)
        {
            var statements = new List<SimpleStatement>();
            foreach (var r in reservations)
            {
                statements.Add(new SimpleStatement(@"
                    INSERT INTO reservations_by_client (clientid, creationdate, reservationid, roomid, checkindate, checkoutdate)
                    VALUES (?, ?, ?, ?, ?, ?)",
                    r.ClientId, r.CreationDate, r.ReservationId, r.RoomId, r.CheckInDate, r.CheckOutDate));
            }
            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertReservationsByRoomBatchAsync(IEnumerable<CassandraReservationByRoomDTO> reservations)
        {
            var statements = new List<SimpleStatement>();
            foreach (var r in reservations)
            {
                statements.Add(new SimpleStatement(@"
                    INSERT INTO reservations_by_room (roomid, creationdate, reservationid, clientid, checkindate, checkoutdate)
                    VALUES (?, ?, ?, ?, ?, ?)",
                    r.RoomId, r.CreationDate, r.ReservationId, r.ClientId, r.CheckInDate, r.CheckOutDate));
            }
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
            var statements = new List<SimpleStatement>();
            foreach (var rs in items)
            {
                statements.Add(new SimpleStatement(@"
                    INSERT INTO reservations_services_by_reservation (reservationid, serviceid, creationdate)
                    VALUES (?, ?, ?)",
                    rs.ReservationId, rs.ServiceId, rs.CreationDate));
            }
            await ExecuteInChunksAsync(statements);
        }

        public async Task InsertReservationServicesByServiceBatchAsync(IEnumerable<CassandraReservationServiceByServiceDTO> items)
        {
            var statements = new List<SimpleStatement>();
            foreach (var rs in items)
            {
                statements.Add(new SimpleStatement(@"
                    INSERT INTO reservations_services_by_service (serviceid, reservationid, creationdate)
                    VALUES (?, ?, ?)",
                    rs.ServiceId, rs.ReservationId, rs.CreationDate));
            }
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
                foreach (var statement in chunk)
                    batch.Add(statement);

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