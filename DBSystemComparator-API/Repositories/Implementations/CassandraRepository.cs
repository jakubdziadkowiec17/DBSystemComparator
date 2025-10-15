using Cassandra;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using MongoDB.Driver;

namespace DBSystemComparator_API.Repositories.Implementations
{
    public class CassandraRepository : ICassandraRepository
    {
        private readonly Cassandra.ISession _session;

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

        // CREATE
        public async Task CreateClientAsync(Guid id, string firstname, string secondname, string lastname, string email, DateTime dateofbirth, string address, string phonenumber, bool isactive)
        {
            var query = "INSERT INTO clients (id, firstname, secondname, lastname, email, dateofbirth, address, phonenumber, isactive) " +
                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";
            await _session.ExecuteAsync(new SimpleStatement(query, id, firstname, secondname, lastname, email, dateofbirth, address, phonenumber, isactive));
        }

        public async Task CreateRoomAsync(Guid id, int number, int capacity, int pricepernight, bool isactive)
        {
            var query = "INSERT INTO rooms (id, number, capacity, pricepernight, isactive) VALUES (?, ?, ?, ?, ?)";
            await _session.ExecuteAsync(new SimpleStatement(query, id, number, capacity, pricepernight, isactive));
        }

        public async Task CreateServiceAsync(Guid id, string name, int price, bool isactive)
        {
            var query = "INSERT INTO services (id, name, price, isactive) VALUES (?, ?, ?, ?)";
            await _session.ExecuteAsync(new SimpleStatement(query, id, name, price, isactive));
        }

        public async Task CreateReservationAsync(Guid id, Guid clientid, Guid roomid, DateTime checkindate, DateTime checkoutdate, DateTime creationdate)
        {
            var query = "INSERT INTO reservations (id, clientid, roomid, checkindate, checkoutdate, creationdate) VALUES (?, ?, ?, ?, ?, ?)";
            await _session.ExecuteAsync(new SimpleStatement(query, id, clientid, roomid, checkindate, checkoutdate, creationdate));
        }

        public async Task CreateReservationServiceAsync(Guid id, Guid reservationid, Guid serviceid, DateTime creationdate)
        {
            var query = "INSERT INTO reservationsservices (id, reservationid, serviceid, creationdate) VALUES (?, ?, ?, ?)";
            await _session.ExecuteAsync(new SimpleStatement(query, id, reservationid, serviceid, creationdate));
        }

        public async Task CreatePaymentAsync(Guid id, Guid reservationid, string description, int sum, DateTime creationdate)
        {
            var query = "INSERT INTO payments (id, reservationid, description, sum, creationdate) VALUES (?, ?, ?, ?, ?)";
            await _session.ExecuteAsync(new SimpleStatement(query, id, reservationid, description, sum, creationdate));
        }

        // READ
        public async Task<RowSet> ReadClientsWithRoomsAsync(bool isactive)
        {
            var query = "SELECT c.id, c.firstname, c.lastname, r.number, r.pricepernight " +
                        "FROM clients c LEFT JOIN reservations res ON res.clientid = c.id " +
                        "LEFT JOIN rooms r ON res.roomid = r.id " +
                        "WHERE c.isactive = ? AND r.isactive = ?";
            return await _session.ExecuteAsync(new SimpleStatement(query, isactive, isactive));
        }

        public async Task<RowSet> ReadRoomsWithReservationCountAsync()
        {
            var query = "SELECT r.id, r.number, r.capacity, COUNT(res.id) AS reservationcount " +
                        "FROM rooms r LEFT JOIN reservations res ON res.roomid = r.id " +
                        "GROUP BY r.id, r.number, r.capacity";
            return await _session.ExecuteAsync(new SimpleStatement(query));
        }

        public async Task<RowSet> ReadServicesUsageAsync()
        {
            var query = "SELECT s.name AS servicename, s.price, COUNT(rs.reservationid) AS usagecount " +
                        "FROM services s LEFT JOIN reservationsservices rs ON s.id = rs.serviceid " +
                        "GROUP BY s.name, s.price";
            return await _session.ExecuteAsync(new SimpleStatement(query));
        }

        public async Task<RowSet> ReadPaymentsAboveAsync(int minsum)
        {
            var query = "SELECT p.id, p.sum, p.creationdate, c.firstname AS clientname, r.number AS roomnumber " +
                        "FROM payments p LEFT JOIN reservations res ON res.id = p.reservationid " +
                        "LEFT JOIN clients c ON res.clientid = c.id " +
                        "LEFT JOIN rooms r ON res.roomid = r.id WHERE p.sum > ?";
            return await _session.ExecuteAsync(new SimpleStatement(query, minsum));
        }

        public async Task<RowSet> ReadReservationsWithServicesAsync(bool clientactive, bool serviceactive)
        {
            var query = "SELECT res.id AS reservationid, c.lastname, s.name AS servicename, s.price, res.checkindate, res.checkoutdate " +
                        "FROM reservations res LEFT JOIN clients c ON res.clientid = c.id " +
                        "LEFT JOIN reservationsservices rs ON rs.reservationid = res.id " +
                        "LEFT JOIN services s ON rs.serviceid = s.id " +
                        "WHERE c.isactive = ? AND s.isactive = ?";
            return await _session.ExecuteAsync(new SimpleStatement(query, clientactive, serviceactive));
        }

        // UPDATE
        public async Task UpdateClientsAddressPhoneAsync(bool isActive)
        {
            var selectQuery = "SELECT id FROM clients WHERE isactive = ? LIMIT 200";
            var preparedSelect = await _session.PrepareAsync(selectQuery);
            var boundSelect = preparedSelect.Bind(isActive);

            var rows = await _session.ExecuteAsync(boundSelect);
            var clientIds = rows.Select(r => (Guid)r["id"]).ToList();

            var updateQuery = "UPDATE clients SET address = 'Cracow, ul. abc 4', phonenumber = '123456789' WHERE id = ?";
            var preparedUpdate = await _session.PrepareAsync(updateQuery);

            foreach (var id in clientIds)
            {
                var boundUpdate = preparedUpdate.Bind(id);
                await _session.ExecuteAsync(boundUpdate);
            }
        }

        public async Task UpdateRoomsPriceJoinReservationsAsync(int mincapacity)
        {
            var query = "UPDATE rooms SET pricepernight = pricepernight + 150 WHERE capacity >= ?";
            await _session.ExecuteAsync(new SimpleStatement(query, mincapacity));
        }

        public async Task UpdateServicesPriceAsync(bool isactive)
        {
            var query = "UPDATE services SET price = price + 25 WHERE isactive = ?";
            await _session.ExecuteAsync(new SimpleStatement(query, isactive));
        }

        public async Task UpdateRoomsPriceInactiveAsync()
        {
            var query = "UPDATE rooms SET pricepernight = pricepernight * 0.8 WHERE isactive = false";
            await _session.ExecuteAsync(new SimpleStatement(query));
        }

        public async Task UpdateRoomsPriceFutureReservationsAsync()
        {
            var query = "UPDATE rooms SET pricepernight = pricepernight - 15";
            await _session.ExecuteAsync(new SimpleStatement(query));
        }

        // DELETE
        public async Task DeleteReservationsSmallRoomsAsync(int capacityThreshold)
        {
            var roomsQuery = "SELECT id FROM rooms WHERE capacity < ? ALLOW FILTERING";
            var preparedRooms = await _session.PrepareAsync(roomsQuery);
            var roomsRows = await _session.ExecuteAsync(preparedRooms.Bind(capacityThreshold));
            var roomIds = roomsRows.Select(r => (Guid)r["id"]).ToList();

            var selectResQuery = "SELECT id FROM reservations WHERE roomid = ?";
            var preparedSelectRes = await _session.PrepareAsync(selectResQuery);

            var reservationIds = new List<Guid>();
            foreach (var roomId in roomIds)
            {
                var resRows = await _session.ExecuteAsync(preparedSelectRes.Bind(roomId));
                reservationIds.AddRange(resRows.Select(r => (Guid)r["id"]));
            }

            var deleteQuery = "DELETE FROM reservations WHERE id = ?";
            var preparedDelete = await _session.PrepareAsync(deleteQuery);

            foreach (var resId in reservationIds)
            {
                await _session.ExecuteAsync(preparedDelete.Bind(resId));
            }
        }

        public async Task DeleteReservationsServicesFutureAsync(int limitRows)
        {
            var selectQuery = "SELECT id FROM reservations WHERE checkindate > toTimestamp(now()) LIMIT ?";
            var preparedSelect = await _session.PrepareAsync(selectQuery);
            var boundSelect = preparedSelect.Bind(limitRows);

            var rows = await _session.ExecuteAsync(boundSelect);
            var reservationIds = rows.Select(r => (Guid)r["id"]).ToList();

            foreach (var reservationId in reservationIds)
            {
                var deleteQuery = "DELETE FROM reservationsservices WHERE reservationid = ?";
                var preparedDelete = await _session.PrepareAsync(deleteQuery);
                var boundDelete = preparedDelete.Bind(reservationId);
                await _session.ExecuteAsync(boundDelete);
            }
        }

        public async Task DeleteReservationsWithoutPaymentsAsync()
        {
            var query = "DELETE FROM reservations";
            await _session.ExecuteAsync(new SimpleStatement(query));
        }

        public async Task DeleteInactiveClientsWithoutReservationsAsync()
        {
            var query = "DELETE FROM clients WHERE isactive = false";
            await _session.ExecuteAsync(new SimpleStatement(query));
        }

        public async Task DeleteRoomsWithoutReservationsAsync()
        {
            var rs = await _session.ExecuteAsync(new SimpleStatement("SELECT id FROM rooms WHERE isactive = false ALLOW FILTERING"));
            foreach (var row in rs)
            {
                var id = row.GetValue<Guid>("id");
                await _session.ExecuteAsync(new SimpleStatement("DELETE FROM rooms WHERE id = ?", id));
            }
        }

        public async Task DeleteAllClientsAsync() => await _session.ExecuteAsync(new SimpleStatement("TRUNCATE clients"));
        public async Task DeleteAllRoomsAsync() => await _session.ExecuteAsync(new SimpleStatement("TRUNCATE rooms"));
        public async Task DeleteAllReservationsAsync() => await _session.ExecuteAsync(new SimpleStatement("TRUNCATE reservations"));
        public async Task DeleteAllReservationsServicesAsync() => await _session.ExecuteAsync(new SimpleStatement("TRUNCATE reservationsservices"));
        public async Task DeleteAllPaymentsAsync() => await _session.ExecuteAsync(new SimpleStatement("TRUNCATE payments"));
        public async Task DeleteAllServicesAsync() => await _session.ExecuteAsync(new SimpleStatement("TRUNCATE services"));
    }
}