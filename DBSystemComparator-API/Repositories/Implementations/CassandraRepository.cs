using Cassandra;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using MongoDB.Driver;

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

        public async Task<TablesCountDTO> GetTablesCountAsync()
        {
            var clientsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT COUNT(*) FROM clients;"));
            var clientsCount = (long)clientsRs.FirstOrDefault()?["count"]!;

            var roomsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT COUNT(*) FROM rooms;"));
            var roomsCount = (long)roomsRs.FirstOrDefault()?["count"]!;

            var reservationsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT COUNT(*) FROM reservations;"));
            var reservationsCount = (long)reservationsRs.FirstOrDefault()?["count"]!;

            var paymentsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT COUNT(*) FROM payments;"));
            var paymentsCount = (long)paymentsRs.FirstOrDefault()?["count"]!;

            var servicesRs = await _session.ExecuteAsync(new SimpleStatement("SELECT COUNT(*) FROM services;"));
            var servicesCount = (long)servicesRs.FirstOrDefault()?["count"]!;

            var resServRs = await _session.ExecuteAsync(new SimpleStatement("SELECT COUNT(*) FROM reservationsservices;"));
            var resServCount = (long)resServRs.FirstOrDefault()?["count"]!;

            return new TablesCountDTO
            {
                ClientsCount = (int)clientsCount,
                RoomsCount = (int)roomsCount,
                ReservationsCount = (int)reservationsCount,
                PaymentsCount = (int)paymentsCount,
                ServicesCount = (int)servicesCount,
                ReservationsServicesCount = (int)resServCount
            };
        }

        // CREATE METHODS
        public async Task<Guid> CreateClientAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive)
        {
            var id = Guid.NewGuid();
            var stmt = new SimpleStatement(@"
                INSERT INTO clients (id, firstname, secondname, lastname, email, dateofbirth, address, phonenumber, isactive)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                id, firstName, secondName, lastName, email, dob, address, phone, isActive
            );
            await _session.ExecuteAsync(stmt);
            return id;
        }

        public async Task<Guid> CreateRoomAsync(int number, int capacity, int pricePerNight, bool isActive)
        {
            var id = Guid.NewGuid();
            var stmt = new SimpleStatement(@"
                INSERT INTO rooms (id, number, capacity, pricepernight, isactive)
                VALUES (?, ?, ?, ?, ?)",
                id, number, capacity, pricePerNight, isActive
            );
            await _session.ExecuteAsync(stmt);
            return id;
        }

        public async Task<Guid> CreateServiceAsync(string name, int price, bool isActive)
        {
            var id = Guid.NewGuid();
            var stmt = new SimpleStatement(@"
                INSERT INTO services (id, name, price, isactive)
                VALUES (?, ?, ?, ?)",
                id, name, price, isActive
            );
            await _session.ExecuteAsync(stmt);
            return id;
        }

        public async Task<Guid> CreateReservationAsync(Guid clientId, Guid roomId, DateTime checkIn, DateTime checkOut, DateTime creationDate)
        {
            var id = Guid.NewGuid();
            var stmt = new SimpleStatement(@"
                INSERT INTO reservations (id, clientid, roomid, checkindate, checkoutdate, creationdate)
                VALUES (?, ?, ?, ?, ?, ?)",
                id, clientId, roomId, checkIn, checkOut, creationDate
            );
            await _session.ExecuteAsync(stmt);
            return id;
        }

        public Task CreateReservationServiceAsync(Guid reservationId, Guid serviceId, DateTime creationDate)
        {
            var stmt = new SimpleStatement(@"
                INSERT INTO reservationsservices (reservationid, serviceid, creationdate)
                VALUES (?, ?, ?)",
                reservationId, serviceId, creationDate
            );
            return _session.ExecuteAsync(stmt);
        }

        public async Task<Guid> CreatePaymentAsync(Guid reservationId, string description, int sum, DateTime creationDate)
        {
            var id = Guid.NewGuid();
            var stmt = new SimpleStatement(@"
                INSERT INTO payments (id, reservationid, description, sum, creationdate)
                VALUES (?, ?, ?, ?, ?)",
                id, reservationId, description, sum, creationDate
            );
            await _session.ExecuteAsync(stmt);
            return id;
        }

        // READ
        public async Task<List<Dictionary<string, object>>> ReadClientsWithRoomsAsync(bool isActive)
        {
            var clientsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM clients WHERE isactive = ?", isActive));
            var reservationsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM reservations"));
            var roomsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM rooms"));

            var clients = clientsRs.ToList();
            var reservations = reservationsRs.ToList();
            var rooms = roomsRs.ToDictionary(r => r["id"]);

            var result = new List<Dictionary<string, object>>();

            foreach (var client in clients)
            {
                var clientReservations = reservations.Where(r => r["clientid"].Equals(client["id"]));
                foreach (var res in clientReservations)
                {
                    if (rooms.TryGetValue(res["roomid"], out var room))
                    {
                        result.Add(new Dictionary<string, object>
                        {
                            {"Id", client["id"]},
                            {"FirstName", client["firstname"]},
                            {"LastName", client["lastname"]},
                            {"Number", room["number"]},
                            {"PricePerNight", room["pricepernight"]}
                        });
                    }
                }
            }

            return result;
        }

        public async Task<List<Dictionary<string, object>>> ReadRoomsWithReservationCountAsync()
        {
            var roomsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM rooms"));
            var reservationsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM reservations"));

            var rooms = roomsRs.ToList();
            var reservations = reservationsRs.ToList();

            var result = new List<Dictionary<string, object>>();

            foreach (var room in rooms)
            {
                var count = reservations.Count(r => r["roomid"].Equals(room["id"]));
                if (count > 0)
                {
                    result.Add(new Dictionary<string, object>
                    {
                        {"Id", room["id"]},
                        {"Number", room["number"]},
                        {"Capacity", room["capacity"]},
                        {"ReservationCount", count}
                    });
                }
            }

            return result;
        }

        public async Task<List<Dictionary<string, object>>> ReadServicesUsageAsync()
        {
            var servicesRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM services"));
            var resServicesRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM reservationsservices"));

            var services = servicesRs.ToList();
            var resServices = resServicesRs.ToList();

            var result = services.Select(s =>
            {
                var usageCount = resServices.Count(rs => rs["serviceid"].Equals(s["id"]));
                return new Dictionary<string, object>
                {
                    {"ServiceName", s["name"]},
                    {"Price", s["price"]},
                    {"UsageCount", usageCount}
                };
            }).OrderByDescending(x => (int)x["UsageCount"]).ToList();

            return result;
        }

        public async Task<List<Dictionary<string, object>>> ReadPaymentsAboveAsync(int minSum)
        {
            var paymentsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM payments"));
            var reservationsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM reservations"));
            var clientsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM clients"));
            var roomsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM rooms"));

            var payments = paymentsRs.ToList().Where(p => (int)p["sum"] > minSum).ToList();
            var reservations = reservationsRs.ToList();
            var clients = clientsRs.ToDictionary(c => c["id"]);
            var rooms = roomsRs.ToDictionary(r => r["id"]);

            var result = new List<Dictionary<string, object>>();

            foreach (var p in payments)
            {
                var res = reservations.FirstOrDefault(r => r["id"].Equals(p["reservationid"]));
                if (res == null) continue;

                clients.TryGetValue(res["clientid"], out var client);
                rooms.TryGetValue(res["roomid"], out var room);

                result.Add(new Dictionary<string, object>
                {
                    {"Id", p["id"]},
                    {"Sum", p["sum"]},
                    {"CreationDate", p["creationdate"]},
                    {"ClientName", client?["firstname"]},
                    {"RoomNumber", room?["number"]}
                });
            }

            return result;
        }

        public async Task<List<Dictionary<string, object>>> ReadReservationsWithServicesAsync(bool clientActive, bool serviceActive)
        {
            var reservationsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM reservations"));
            var clientsRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM clients WHERE isactive = ?", clientActive));
            var resServicesRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM reservationsservices"));
            var servicesRs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM services WHERE isactive = ?", serviceActive));

            var reservations = reservationsRs.ToList();
            var clients = clientsRs.ToDictionary(c => c["id"]);
            var resServices = resServicesRs.ToList();
            var services = servicesRs.ToDictionary(s => s["id"]);

            var result = new List<Dictionary<string, object>>();

            foreach (var res in reservations)
            {
                if (!clients.ContainsKey(res["clientid"])) continue;

                var client = clients[res["clientid"]];
                var linkedServices = resServices.Where(rs => rs["reservationid"].Equals(res["id"]));

                foreach (var rs in linkedServices)
                {
                    if (!services.ContainsKey(rs["serviceid"])) continue;

                    var svc = services[rs["serviceid"]];
                    result.Add(new Dictionary<string, object>
                    {
                        {"ReservationId", res["id"]},
                        {"LastName", client["lastname"]},
                        {"ServiceName", svc["name"]},
                        {"ServicePrice", svc["price"]},
                        {"CheckInDate", res["checkindate"]},
                        {"CheckOutDate", res["checkoutdate"]}
                    });
                }
            }

            return result;
        }

        // UPDATE
        public async Task UpdateClientsAddressPhoneAsync(bool isActive)
        {
            var clients = (await _session.ExecuteAsync(
                new SimpleStatement("SELECT id FROM clients WHERE isactive = ? LIMIT 200", isActive)
            )).Select(r => r["id"]);

            foreach (var clientId in clients)
            {
                await _session.ExecuteAsync(
                    new SimpleStatement(
                        "UPDATE clients SET address = ?, phonenumber = ? WHERE id = ?",
                        "Cracow, ul. abc 4", "123456789", clientId
                    )
                );
            }
        }

        public async Task UpdateRoomsPriceJoinReservationsAsync(int minCapacity)
        {
            var reservations = await _session.ExecuteAsync(new SimpleStatement("SELECT roomid FROM reservations"));
            var reservedRoomIds = reservations.Select(r => r["roomid"]).ToHashSet();

            var rooms = await _session.ExecuteAsync(new SimpleStatement("SELECT id, capacity FROM rooms"));
            var targetRoomIds = rooms
                .Where(r => reservedRoomIds.Contains(r["id"]) && (int)r["capacity"] >= minCapacity)
                .Select(r => r["id"]);

            foreach (var roomId in targetRoomIds)
            {
                await _session.ExecuteAsync(new SimpleStatement(
                    "UPDATE rooms SET pricepernight = pricepernight + 150 WHERE id = ?",
                    roomId
                ));
            }
        }

        public async Task UpdateServicesPriceAsync(bool isActive)
        {
            var services = await _session.ExecuteAsync(
                new SimpleStatement("SELECT id, price FROM services WHERE isactive = ?", isActive)
            );

            foreach (var service in services)
            {
                var serviceId = service["id"];
                var currentPrice = (int)service["price"];
                var newPrice = currentPrice + 25;

                await _session.ExecuteAsync(
                    new SimpleStatement(
                        "UPDATE services SET price = ? WHERE id = ?",
                        newPrice,
                        serviceId
                    )
                );
            }
        }

        public async Task UpdateRoomsPriceInactiveAsync()
        {
            var rooms = await _session.ExecuteAsync(
                new SimpleStatement("SELECT id, pricepernight FROM rooms WHERE isactive = false")
            );

            foreach (var room in rooms)
            {
                var roomId = room["id"];
                var currentPrice = (int)room["pricepernight"];
                var newPrice = (int)(currentPrice * 0.8);

                await _session.ExecuteAsync(
                    new SimpleStatement(
                        "UPDATE rooms SET pricepernight = ? WHERE id = ?",
                        newPrice,
                        roomId
                    )
                );
            }
        }

        public async Task UpdateRoomsPriceFutureReservationsAsync()
        {
            var reservations = await _session.ExecuteAsync(
                new SimpleStatement("SELECT roomid, checkindate FROM reservations")
            );

            var futureRoomIds = reservations
                .Where(r => ((DateTime)r["checkindate"]) > DateTime.Now)
                .Select(r => r["roomid"])
                .Distinct();

            foreach (var roomId in futureRoomIds)
            {
                var room = await _session.ExecuteAsync(
                    new SimpleStatement("SELECT pricepernight FROM rooms WHERE id = ?", roomId)
                );
                var currentPrice = (int)room.First()["pricepernight"];
                var newPrice = currentPrice - 15;

                await _session.ExecuteAsync(
                    new SimpleStatement(
                        "UPDATE rooms SET pricepernight = ? WHERE id = ?",
                        newPrice,
                        roomId
                    )
                );
            }
        }

        // DELETE
        public Task DeleteClientAsync(Guid clientId)
        {
            return _session.ExecuteAsync(new SimpleStatement("DELETE FROM clients WHERE id = ?", clientId));
        }

        public Task DeleteRoomAsync(Guid roomId)
        {
            return _session.ExecuteAsync(new SimpleStatement("DELETE FROM rooms WHERE id = ?", roomId));
        }

        public Task DeleteReservationAsync(Guid reservationId)
        {
            return _session.ExecuteAsync(new SimpleStatement("DELETE FROM reservations WHERE id = ?", reservationId));
        }

        public async Task DeleteReservationsSmallRoomsAsync(int capacityThreshold)
        {
            var allRooms = await _session.ExecuteAsync(
                new SimpleStatement("SELECT id, capacity FROM rooms")
            );

            var smallRoomIds = allRooms
                .Where(r => (int)r["capacity"] < capacityThreshold)
                .Select(r => r["id"])
                .ToHashSet();

            if (!smallRoomIds.Any())
                return;

            var allReservations = await _session.ExecuteAsync(
                new SimpleStatement("SELECT id, roomid, checkindate FROM reservations")
            );

            var reservationIdsToDelete = allReservations
                .Where(r => smallRoomIds.Contains(r["roomid"]) && ((DateTime)r["checkindate"]) > DateTime.Now)
                .Select(r => r["id"]);

            foreach (var resId in reservationIdsToDelete)
            {
                await DeleteReservationAsync((Guid)resId);
            }
        }

        public async Task DeleteReservationsServicesFutureAsync(int topRows)
        {
            var reservationIds = (await _session.ExecuteAsync(
                new SimpleStatement("SELECT id, checkindate FROM reservations")
            )).Where(r => ((DateTime)r["checkindate"]) > DateTime.Now)
             .Take(topRows)
             .Select(r => r["id"]);

            foreach (var resId in reservationIds)
            {
                await _session.ExecuteAsync(
                    new SimpleStatement("DELETE FROM reservationsservices WHERE reservationid = ?", resId)
                );
            }
        }

        public async Task DeleteReservationsWithoutPaymentsAsync()
        {
            var paymentsRs = await _session.ExecuteAsync(
                new SimpleStatement("SELECT reservationid FROM payments")
            );
            var paymentsSet = paymentsRs.Select(p => p["reservationid"]).ToHashSet();

            var reservationsRs = await _session.ExecuteAsync(
                new SimpleStatement("SELECT id FROM reservations")
            );

            foreach (var res in reservationsRs)
            {
                var resId = (Guid)res["id"];
                if (!paymentsSet.Contains(resId))
                {
                    await DeleteReservationAsync(resId);
                }
            }
        }

        public async Task DeleteInactiveClientsWithoutReservationsAsync()
        {
            var reservations = await _session.ExecuteAsync(
                new SimpleStatement("SELECT clientid FROM reservations")
            );
            var activeClientIds = reservations.Select(r => r["clientid"]).ToHashSet();

            var clients = await _session.ExecuteAsync(
                new SimpleStatement("SELECT id FROM clients WHERE isactive = false ALLOW FILTERING")
            );

            foreach (var client in clients)
            {
                var clientId = (Guid)client["id"];
                if (!activeClientIds.Contains(clientId))
                {
                    await DeleteClientAsync(clientId);
                }
            }
        }

        public async Task DeleteRoomsWithoutReservationsAsync()
        {
            var reservations = await _session.ExecuteAsync(
                new SimpleStatement("SELECT roomid FROM reservations")
            );
            var reservedRoomIds = reservations.Select(r => r["roomid"]).ToHashSet();

            var inactiveRooms = await _session.ExecuteAsync(
                new SimpleStatement("SELECT id FROM rooms WHERE isactive = false ALLOW FILTERING")
            );

            foreach (var room in inactiveRooms)
            {
                var roomId = (Guid)room["id"];
                if (!reservedRoomIds.Contains(roomId))
                {
                    await DeleteRoomAsync(roomId);
                }
            }
        }

        public Task DeleteAllClientsAsync() => _session.ExecuteAsync(new SimpleStatement("TRUNCATE clients"));
        public Task DeleteAllRoomsAsync() => _session.ExecuteAsync(new SimpleStatement("TRUNCATE rooms"));
        public Task DeleteAllReservationsAsync() => _session.ExecuteAsync(new SimpleStatement("TRUNCATE reservations"));
        public Task DeleteAllReservationsServicesAsync() => _session.ExecuteAsync(new SimpleStatement("TRUNCATE reservationsservices"));
        public Task DeleteAllPaymentsAsync() => _session.ExecuteAsync(new SimpleStatement("TRUNCATE payments"));
        public Task DeleteAllServicesAsync() => _session.ExecuteAsync(new SimpleStatement("TRUNCATE services"));

        private IEnumerable<List<T>> Chunk<T>(IEnumerable<T> source, int chunkSize)
        {
            var list = new List<T>();
            foreach (var item in source)
            {
                list.Add(item);
                if (list.Count >= chunkSize)
                {
                    yield return list;
                    list = new List<T>();
                }
            }
            if (list.Count > 0) yield return list;
        }

        public async Task CreateClientsBatchAsync(IEnumerable<(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive)> clients)
        {
            var statements = clients.Select(c =>
            {
                var id = Guid.NewGuid();
                return new SimpleStatement(@"
                    INSERT INTO clients (id, firstname, secondname, lastname, email, dateofbirth, address, phonenumber, isactive)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                    id, c.firstName, c.secondName, c.lastName, c.email, c.dob, c.address, c.phone, c.isActive);
            });

            foreach (var chunk in Chunk(statements, ChunkSize))
            {
                var batch = new BatchStatement();
                foreach (var stmt in chunk) batch.Add(stmt);
                await _session.ExecuteAsync(batch);
            }
        }

        public async Task CreateRoomsBatchAsync(IEnumerable<(int number, int capacity, int pricePerNight, bool isActive)> rooms)
        {
            var statements = rooms.Select(r =>
            {
                var id = Guid.NewGuid();
                return new SimpleStatement(@"
                    INSERT INTO rooms (id, number, capacity, pricepernight, isactive)
                    VALUES (?, ?, ?, ?, ?)",
                    id, r.number, r.capacity, r.pricePerNight, r.isActive);
            });

            foreach (var chunk in Chunk(statements, ChunkSize))
            {
                var batch = new BatchStatement();
                foreach (var stmt in chunk) batch.Add(stmt);
                await _session.ExecuteAsync(batch);
            }
        }

        public async Task CreateServicesBatchAsync(IEnumerable<(string name, int price, bool isActive)> services)
        {
            var statements = services.Select(s =>
            {
                var id = Guid.NewGuid();
                return new SimpleStatement(@"
                    INSERT INTO services (id, name, price, isactive)
                    VALUES (?, ?, ?, ?)",
                    id, s.name, s.price, s.isActive);
            });

            foreach (var chunk in Chunk(statements, ChunkSize))
            {
                var batch = new BatchStatement();
                foreach (var stmt in chunk) batch.Add(stmt);
                await _session.ExecuteAsync(batch);
            }
        }

        public async Task CreateReservationsBatchAsync(IEnumerable<(Guid clientId, Guid roomId, DateTime checkIn, DateTime checkOut, DateTime creationDate)> reservations)
        {
            var statements = reservations.Select(r =>
            {
                var id = Guid.NewGuid();
                return new SimpleStatement(@"
                    INSERT INTO reservations (id, clientid, roomid, checkindate, checkoutdate, creationdate)
                    VALUES (?, ?, ?, ?, ?, ?)",
                    id, r.clientId, r.roomId, r.checkIn, r.checkOut, r.creationDate);
            });

            foreach (var chunk in Chunk(statements, ChunkSize))
            {
                var batch = new BatchStatement();
                foreach (var stmt in chunk) batch.Add(stmt);
                await _session.ExecuteAsync(batch);
            }
        }

        public async Task CreatePaymentsBatchAsync(IEnumerable<(Guid reservationId, string description, int sum, DateTime creationDate)> payments)
        {
            var statements = payments.Select(p =>
            {
                var id = Guid.NewGuid();
                return new SimpleStatement(@"
                    INSERT INTO payments (id, reservationid, description, sum, creationdate)
                    VALUES (?, ?, ?, ?, ?)",
                    id, p.reservationId, p.description, p.sum, p.creationDate);
            });

            foreach (var chunk in Chunk(statements, ChunkSize))
            {
                var batch = new BatchStatement();
                foreach (var stmt in chunk) batch.Add(stmt);
                await _session.ExecuteAsync(batch);
            }
        }

        public async Task CreateReservationsServicesBatchAsync(IEnumerable<(Guid reservationId, Guid serviceId, DateTime creationDate)> resServices)
        {
            var statements = resServices.Select(rs =>
                new SimpleStatement(@"
                    INSERT INTO reservationsservices (reservationid, serviceid, creationdate)
                    VALUES (?, ?, ?)",
                    rs.reservationId, rs.serviceId, rs.creationDate)
            );

            foreach (var chunk in Chunk(statements, ChunkSize))
            {
                var batch = new BatchStatement();
                foreach (var stmt in chunk) batch.Add(stmt);
                await _session.ExecuteAsync(batch);
            }
        }

        public async Task<List<Guid>> GetAllClientIdsAsync()
        {
            var result = new List<Guid>();
            var rs = await _session.ExecuteAsync(new SimpleStatement("SELECT id FROM clients"));
            foreach (var row in rs) result.Add((Guid)row["id"]);
            return result;
        }

        public async Task<List<Guid>> GetAllRoomIdsAsync()
        {
            var result = new List<Guid>();
            var rs = await _session.ExecuteAsync(new SimpleStatement("SELECT id FROM rooms"));
            foreach (var row in rs) result.Add((Guid)row["id"]);
            return result;
        }

        public async Task<List<Guid>> GetAllServiceIdsAsync()
        {
            var result = new List<Guid>();
            var rs = await _session.ExecuteAsync(new SimpleStatement("SELECT id FROM services"));
            foreach (var row in rs) result.Add((Guid)row["id"]);
            return result;
        }

        public async Task<List<Guid>> GetAllReservationIdsAsync()
        {
            var result = new List<Guid>();
            var rs = await _session.ExecuteAsync(new SimpleStatement("SELECT id FROM reservations"));
            foreach (var row in rs) result.Add((Guid)row["id"]);
            return result;
        }
    }
}