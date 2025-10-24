using Npgsql;

namespace DBSystemComparator_API.Database
{
    public static class PostgreSQLSeeder
    {
        public static async Task CreateDatabaseAsync(string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = builder.Database;

            builder.Database = "postgres";
            await using var sysConnection = new NpgsqlConnection(builder.ConnectionString);
            await sysConnection.OpenAsync();

            var checkDbCmd = new NpgsqlCommand(
                "SELECT 1 FROM pg_database WHERE datname = @name", sysConnection);
            checkDbCmd.Parameters.AddWithValue("name", databaseName);

            var exists = await checkDbCmd.ExecuteScalarAsync();
            if (exists == null)
            {
                var createDbCmd = new NpgsqlCommand($@"CREATE DATABASE ""{databaseName}"";", sysConnection);
                await createDbCmd.ExecuteNonQueryAsync();
            }

            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var commands = new[]
            {
                // Clients
                @"CREATE TABLE IF NOT EXISTS Clients (
                    Id SERIAL PRIMARY KEY,
                    FirstName VARCHAR(30) NOT NULL,
                    SecondName VARCHAR(30),
                    LastName VARCHAR(30) NOT NULL,
                    Email VARCHAR(1000) NOT NULL,
                    DateOfBirth TIMESTAMP NOT NULL,
                    Address VARCHAR(200) NOT NULL,
                    PhoneNumber VARCHAR(20) NOT NULL,
                    IsActive BOOLEAN NOT NULL
                );",

                // Rooms
                @"CREATE TABLE IF NOT EXISTS Rooms (
                    Id SERIAL PRIMARY KEY,
                    Number INT NOT NULL,
                    Capacity INT NOT NULL,
                    PricePerNight DOUBLE PRECISION NOT NULL,
                    IsActive BOOLEAN NOT NULL
                );",

                // Services
                @"CREATE TABLE IF NOT EXISTS Services (
                    Id SERIAL PRIMARY KEY,
                    Name VARCHAR(50) NOT NULL,
                    Price INT NOT NULL,
                    IsActive BOOLEAN NOT NULL
                );",

                // Reservations
                @"CREATE TABLE IF NOT EXISTS Reservations (
                    Id SERIAL PRIMARY KEY,
                    ClientId INT NOT NULL,
                    RoomId INT NOT NULL,
                    CheckInDate TIMESTAMP NOT NULL,
                    CheckOutDate TIMESTAMP NOT NULL,
                    CreationDate TIMESTAMP NOT NULL,
                    CONSTRAINT FK_Reservations_Clients FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE CASCADE,
                    CONSTRAINT FK_Reservations_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE CASCADE
                );",

                // Payments
                @"CREATE TABLE IF NOT EXISTS Payments (
                    Id SERIAL PRIMARY KEY,
                    ReservationId INT NOT NULL,
                    Description TEXT,
                    Sum INT NOT NULL,
                    CreationDate TIMESTAMP NOT NULL,
                    CONSTRAINT FK_Payments_Reservations FOREIGN KEY (ReservationId) REFERENCES Reservations(Id) ON DELETE CASCADE
                );",

                // ReservationsServices
                @"CREATE TABLE IF NOT EXISTS ReservationsServices (
                    ReservationId INT NOT NULL,
                    ServiceId INT NOT NULL,
                    CreationDate TIMESTAMP NOT NULL,
                    PRIMARY KEY (ReservationId, ServiceId),
                    CONSTRAINT FK_ReservationsServices_Reservations FOREIGN KEY (ReservationId) REFERENCES Reservations(Id) ON DELETE CASCADE,
                    CONSTRAINT FK_ReservationsServices_Services FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
                );",

                // Indexes
                @"CREATE INDEX IF NOT EXISTS ix_reservations_clientid ON Reservations(ClientId);",
                @"CREATE INDEX IF NOT EXISTS ix_reservations_roomid ON Reservations(RoomId);",
                @"CREATE INDEX IF NOT EXISTS ix_payments_reservationid ON Payments(ReservationId);",
                @"CREATE INDEX IF NOT EXISTS ix_reservationsservices_serviceid ON ReservationsServices(ServiceId);"
            };

            foreach (var cmdText in commands)
            {
                await using var command = new NpgsqlCommand(cmdText, connection);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}