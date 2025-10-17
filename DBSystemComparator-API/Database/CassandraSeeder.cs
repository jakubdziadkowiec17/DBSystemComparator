using Cassandra;

namespace DBSystemComparator_API.Database
{
    public static class CassandraSeeder
    {
        public static async Task CreateTablesAsync(Cassandra.ISession session)
        {
            // --- Clients (mała tabela, dostęp po id) ---
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

            // --- Active clients (dla szybkich zapytań o aktywnych) ---
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS active_clients (
                    id uuid PRIMARY KEY,
                    firstname text,
                    lastname text,
                    email text,
                    dateofbirth timestamp,
                    address text,
                    phonenumber text
                );
            "));

            // --- Rooms ---
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS rooms (
                    id uuid PRIMARY KEY,
                    number int,
                    capacity int,
                    pricepernight int,
                    isactive boolean
                );
            "));

            // --- Active Rooms ---
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS active_rooms (
                    id uuid PRIMARY KEY,
                    number int,
                    capacity int,
                    pricepernight int
                );
            "));

            // --- Services ---
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS services (
                    id uuid PRIMARY KEY,
                    name text,
                    price int,
                    isactive boolean
                );
            "));

            // --- Active Services ---
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS active_services (
                    id uuid PRIMARY KEY,
                    name text,
                    price int
                );
            "));

            // --- Reservations by Client (pod zapytania: wszystkie rezerwacje klienta) ---
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS reservations_by_client (
                    clientid uuid,
                    reservationid uuid,
                    roomid uuid,
                    checkindate timestamp,
                    checkoutdate timestamp,
                    creationdate timestamp,
                    PRIMARY KEY (clientid, reservationid)
                ) WITH CLUSTERING ORDER BY (reservationid DESC);
            "));

            // --- Reservations by Room (pod zapytania: wszystkie rezerwacje pokoju) ---
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS reservations_by_room (
                    roomid uuid,
                    reservationid uuid,
                    clientid uuid,
                    checkindate timestamp,
                    checkoutdate timestamp,
                    creationdate timestamp,
                    PRIMARY KEY (roomid, reservationid)
                ) WITH CLUSTERING ORDER BY (reservationid DESC);
            "));

            // --- Payments by Reservation (pod zapytania: płatności danej rezerwacji) ---
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS payments_by_reservation (
                    reservationid uuid,
                    paymentid uuid,
                    description text,
                    sum int,
                    creationdate timestamp,
                    PRIMARY KEY (reservationid, paymentid)
                ) WITH CLUSTERING ORDER BY (paymentid DESC);
            "));

            // --- ReservationsServices by Reservation ---
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS reservationsservices_by_reservation (
                    reservationid uuid,
                    serviceid uuid,
                    creationdate timestamp,
                    PRIMARY KEY (reservationid, serviceid)
                );
            "));

            // --- ReservationsServices by Service (np. wszystkie rezerwacje z daną usługą) ---
            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS reservationsservices_by_service (
                    serviceid uuid,
                    reservationid uuid,
                    creationdate timestamp,
                    PRIMARY KEY (serviceid, reservationid)
                );
            "));
        }
    }
}