using Microsoft.Data.SqlClient;

namespace DBSystemComparator_API.Database
{
    public static class SQLServerSeeder
    {
        public static async Task CreateDatabaseAsync(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;

            builder.InitialCatalog = "master";
            await using var masterConnection = new SqlConnection(builder.ConnectionString);
            await masterConnection.OpenAsync();

            var createDbCmd = new SqlCommand($@"
                IF DB_ID('{databaseName}') IS NULL
                BEGIN
                    CREATE DATABASE [{databaseName}];
                END
            ", masterConnection);

            await createDbCmd.ExecuteNonQueryAsync();

            builder.InitialCatalog = databaseName;
            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync();

            var commands = new[]
            {
                // Clients
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Clients' AND xtype='U')
                  CREATE TABLE Clients (
                      Id INT IDENTITY PRIMARY KEY,
                      FirstName NVARCHAR(30) NOT NULL,
                      SecondName NVARCHAR(30) NULL,
                      LastName NVARCHAR(30) NOT NULL,
                      Email NVARCHAR(1000) NOT NULL,
                      DateOfBirth DATETIME2 NOT NULL,
                      Address NVARCHAR(200) NOT NULL,
                      PhoneNumber NVARCHAR(20) NOT NULL,
                      IsActive BIT NOT NULL
                  );",

                // Rooms
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Rooms' AND xtype='U')
                  CREATE TABLE Rooms (
                      Id INT IDENTITY PRIMARY KEY,
                      Number INT NOT NULL,
                      Capacity INT NOT NULL,
                      PricePerNight DECIMAL(18,2) NOT NULL,
                      IsActive BIT NOT NULL
                  );",

                // Services
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Services' AND xtype='U')
                  CREATE TABLE Services (
                      Id INT IDENTITY PRIMARY KEY,
                      Name NVARCHAR(50) NOT NULL,
                      Price INT NOT NULL,
                      IsActive BIT NOT NULL
                  );",

                // Reservations
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reservations' AND xtype='U')
                  CREATE TABLE Reservations (
                      Id INT IDENTITY PRIMARY KEY,
                      ClientId INT NOT NULL,
                      RoomId INT NOT NULL,
                      CheckInDate DATETIME2 NOT NULL,
                      CheckOutDate DATETIME2 NOT NULL,
                      CreationDate DATETIME2 NOT NULL,
                      CONSTRAINT FK_Reservations_Clients FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE CASCADE,
                      CONSTRAINT FK_Reservations_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE CASCADE
                  );",

                // Payments
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U')
                  CREATE TABLE Payments (
                      Id INT IDENTITY PRIMARY KEY,
                      ReservationId INT NOT NULL,
                      Description NVARCHAR(MAX) NULL,
                      Sum INT NOT NULL,
                      CreationDate DATETIME2 NOT NULL,
                      CONSTRAINT FK_Payments_Reservations FOREIGN KEY (ReservationId) REFERENCES Reservations(Id) ON DELETE CASCADE
                  );",

                // ReservationsServices
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ReservationsServices' AND xtype='U')
                  CREATE TABLE ReservationsServices (
                      ReservationId INT NOT NULL,
                      ServiceId INT NOT NULL,
                      CreationDate DATETIME2 NOT NULL,
                      CONSTRAINT PK_ReservationsServices PRIMARY KEY (ReservationId, ServiceId),
                      CONSTRAINT FK_ReservationsServices_Reservations FOREIGN KEY (ReservationId) REFERENCES Reservations(Id) ON DELETE CASCADE,
                      CONSTRAINT FK_ReservationsServices_Services FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
                  );"
            };
            foreach (var cmdText in commands)
            {
                using var command = new SqlCommand(cmdText, connection);
                await command.ExecuteNonQueryAsync();
            }

            // Indexes
            var indexCommands = new[]
            {
                @"IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Reservations_ClientId' AND object_id = OBJECT_ID('Reservations'))
                    CREATE INDEX IX_Reservations_ClientId ON Reservations(ClientId);",

                @"IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Reservations_RoomId' AND object_id = OBJECT_ID('Reservations'))
                    CREATE INDEX IX_Reservations_RoomId ON Reservations(RoomId);",

                @"IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Payments_ReservationId' AND object_id = OBJECT_ID('Payments'))
                    CREATE INDEX IX_Payments_ReservationId ON Payments(ReservationId);",

                @"IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_ReservationsServices_ServiceId' AND object_id = OBJECT_ID('ReservationsServices'))
                    CREATE INDEX IX_ReservationsServices_ServiceId ON ReservationsServices(ServiceId);"
            };
            foreach (var cmdText in commands.Concat(indexCommands))
            {
                using var command = new SqlCommand(cmdText, connection);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}