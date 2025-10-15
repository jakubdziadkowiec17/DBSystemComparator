using Cassandra;

namespace DBSystemComparator_API.Database
{
    public static class CassandraSeeder
    {
        public static async Task CreateTablesAsync(Cassandra.ISession session)
        {
            // Clients
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS clients (
                    id uuid PRIMARY KEY,
                    firstname text,
                    secondname text,
                    lastname text,
                    email text,
                    dateofbirth timestamp,
                    address text,
                    phonenumber text,
                    isactive boolean
                );
            "));

            // Rooms
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS rooms (
                    id uuid PRIMARY KEY,
                    number int,
                    capacity int,
                    pricepernight int,
                    isactive boolean
                );
            "));

            // Reservations
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS reservations (
                    id uuid PRIMARY KEY,
                    clientid uuid,
                    roomid uuid,
                    checkindate timestamp,
                    checkoutdate timestamp,
                    creationdate timestamp
                );
            "));

            // Payments
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS payments (
                    id uuid PRIMARY KEY,
                    reservationid uuid,
                    description text,
                    sum int,
                    creationdate timestamp
                );
            "));

            // Services
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS services (
                    id uuid PRIMARY KEY,
                    name text,
                    price int,
                    isactive boolean
                );
            "));

            // ReservationsServices
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS reservationsservices (
                    reservationid uuid,
                    serviceid uuid,
                    creationdate timestamp,
                    PRIMARY KEY (reservationid, serviceid)
                );
            "));

            // Indexes
            await session.ExecuteAsync(new SimpleStatement(@"CREATE INDEX IF NOT EXISTS idx_clients_isactive ON clients(isactive);"));
            await session.ExecuteAsync(new SimpleStatement(@"CREATE INDEX IF NOT EXISTS idx_rooms_isactive ON rooms(isactive);"));
            await session.ExecuteAsync(new SimpleStatement(@"CREATE INDEX IF NOT EXISTS idx_reservations_clientid ON reservations(clientid);"));
            await session.ExecuteAsync(new SimpleStatement(@"CREATE INDEX IF NOT EXISTS idx_reservations_roomid ON reservations(roomid);"));
            await session.ExecuteAsync(new SimpleStatement(@"CREATE INDEX IF NOT EXISTS idx_payments_reservationid ON payments(reservationid);"));
            await session.ExecuteAsync(new SimpleStatement(@"CREATE INDEX IF NOT EXISTS idx_services_isactive ON services(isactive);"));
            await session.ExecuteAsync(new SimpleStatement(@"CREATE INDEX IF NOT EXISTS idx_reservationsservices_serviceid ON reservationsservices(serviceid);"));
        }
    }
}