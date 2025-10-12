using Cassandra;

namespace DBSystemComparator_API.Database
{
    public static class CassandraSeeder
    {
        public static void CreateTables(Cassandra.ISession session)
        {
            session.Execute(@"
            CREATE TABLE IF NOT EXISTS clients (
                id int PRIMARY KEY,
                name text,
                secondname text,
                lastname text,
                email text,
                dateofbirth timestamp,
                address text,
                phonenumber text,
                isactive boolean
            );
        ");

            session.Execute(@"
            CREATE TABLE IF NOT EXISTS rooms (
                id int PRIMARY KEY,
                number int,
                capacity int,
                pricepernight int,
                isactive boolean
            );
        ");

            session.Execute(@"
            CREATE TABLE IF NOT EXISTS reservations (
                id int PRIMARY KEY,
                clientid int,
                roomid int,
                checkindate timestamp,
                checkoutdate timestamp,
                creationdate timestamp
            );
        ");

            session.Execute(@"
            CREATE TABLE IF NOT EXISTS payments (
                id int PRIMARY KEY,
                reservationid int,
                description text,
                sum int,
                creationdate timestamp
            );
        ");

            session.Execute(@"
            CREATE TABLE IF NOT EXISTS services (
                id int PRIMARY KEY,
                name text,
                price int,
                isactive boolean
            );
        ");

            session.Execute(@"
            CREATE TABLE IF NOT EXISTS reservation_services (
                reservationid int,
                serviceid int,
                creationdate timestamp,
                PRIMARY KEY (reservationid, serviceid)
            );
        ");
        }
    }
}