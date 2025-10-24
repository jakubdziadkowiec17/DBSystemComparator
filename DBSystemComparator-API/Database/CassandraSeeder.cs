using Cassandra;

namespace DBSystemComparator_API.Database
{
    public static class CassandraSeeder
    {
        public static async Task CreateTablesAsync(Cassandra.ISession session)
        {
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

            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS active_clients_by_status (
                    isactive boolean,
                    id uuid,
                    firstname text,
                    lastname text,
                    email text,
                    dateofbirth timestamp,
                    address text,
                    phonenumber text,
                    PRIMARY KEY (isactive, id)
                );
            "));

            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS rooms (
                    id uuid PRIMARY KEY,
                    number int,
                    capacity int,
                    pricepernight int,
                    isactive boolean
                );
            "));

            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS active_rooms_by_status (
                    isactive boolean,
                    id uuid,
                    number int,
                    capacity int,
                    pricepernight int,
                    PRIMARY KEY (isactive, id)
                );
            "));

            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS services (
                    id uuid PRIMARY KEY,
                    name text,
                    price int,
                    isactive boolean
                );
            "));

            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS active_services_by_price (
                    isactive boolean,
                    price int,
                    id uuid,
                    name text,
                    PRIMARY KEY (isactive, price, id)
                ) WITH CLUSTERING ORDER BY (price DESC);
            "));

            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS reservations_by_client (
                    clientid uuid,
                    creationdate timestamp,
                    reservationid uuid,
                    roomid uuid,
                    checkindate timestamp,
                    checkoutdate timestamp,
                    PRIMARY KEY (clientid, creationdate, reservationid)
                ) WITH CLUSTERING ORDER BY (creationdate DESC);
            "));

            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS reservations_by_room (
                    roomid uuid,
                    creationdate timestamp,
                    reservationid uuid,
                    clientid uuid,
                    checkindate timestamp,
                    checkoutdate timestamp,
                    PRIMARY KEY (roomid, creationdate, reservationid)
                ) WITH CLUSTERING ORDER BY (creationdate DESC);
            "));

            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS payments_by_reservation (
                    reservationid uuid,
                    creationdate timestamp,
                    paymentid uuid,
                    description text,
                    sum int,
                    PRIMARY KEY (reservationid, creationdate, paymentid)
                ) WITH CLUSTERING ORDER BY (creationdate DESC);
            "));

            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS reservations_services_by_reservation (
                    reservationid uuid,
                    serviceid uuid,
                    creationdate timestamp,
                    PRIMARY KEY (reservationid, serviceid)
                );
            "));

            await session.ExecuteAsync(new SimpleStatement(@"
                CREATE TABLE IF NOT EXISTS reservations_services_by_service (
                    serviceid uuid,
                    reservationid uuid,
                    creationdate timestamp,
                    PRIMARY KEY (serviceid, reservationid)
                );
            "));
        }
    }
}