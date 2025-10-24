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



        // READ



        // UPDATE



        // DELETE



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