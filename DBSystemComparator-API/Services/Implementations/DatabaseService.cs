using DBSystemComparator_API.Constants;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using DBSystemComparator_API.Services.Interfaces;

namespace DBSystemComparator_API.Services.Implementations
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IPostgreSQLRepository _postgreSQLRepository;
        private readonly ISQLServerRepository _sqlServerRepository;
        private readonly IMongoDBRepository _mongoDBRepository;
        private readonly ICassandraRepository _cassandraRepository;
        public DatabaseService(IPostgreSQLRepository postgreSQLRepository, ISQLServerRepository sqlServerRepository, IMongoDBRepository mongoDBRepository, ICassandraRepository cassandraRepository)
        {
            _postgreSQLRepository = postgreSQLRepository;
            _sqlServerRepository = sqlServerRepository;
            _mongoDBRepository = mongoDBRepository;
            _cassandraRepository = cassandraRepository;
        }

        public async Task<DataCountDTO> GetTablesCountForDatabasesAsync()
        {
            var tablesCountForPostgreSQL = await _postgreSQLRepository.GetTablesCountAsync();
            var tablesCountForSQLServer = await _sqlServerRepository.GetTablesCountAsync();
            var tablesCountForMongoDB = await _mongoDBRepository.GetTablesCountAsync();
            var tablesCountForCassandra = await _cassandraRepository.GetTablesCountAsync();

            return new DataCountDTO()
            {
                PostgreSQL = tablesCountForPostgreSQL,
                SQLServer = tablesCountForSQLServer,
                MongoDB = tablesCountForMongoDB,
                Cassandra = tablesCountForCassandra
            };
        }

        public async Task<ResponseDTO> GenerateDataAsync(GenerateDataDTO generateDataDTO)
        {
            var deleteTasks = new List<Task>
            {
                _postgreSQLRepository.DeleteAllPaymentsAsync(),
                _postgreSQLRepository.DeleteAllReservationsServicesAsync(),
                _postgreSQLRepository.DeleteAllReservationsAsync(),
                _postgreSQLRepository.DeleteAllServicesAsync(),
                _postgreSQLRepository.DeleteAllRoomsAsync(),
                _postgreSQLRepository.DeleteAllClientsAsync(),

                _sqlServerRepository.DeleteAllPaymentsAsync(),
                _sqlServerRepository.DeleteAllReservationsServicesAsync(),
                _sqlServerRepository.DeleteAllReservationsAsync(),
                _sqlServerRepository.DeleteAllServicesAsync(),
                _sqlServerRepository.DeleteAllRoomsAsync(),
                _sqlServerRepository.DeleteAllClientsAsync(),

                _mongoDBRepository.DeleteAllPaymentsAsync(),
                _mongoDBRepository.DeleteAllReservationsServicesAsync(),
                _mongoDBRepository.DeleteAllReservationsAsync(),
                _mongoDBRepository.DeleteAllServicesAsync(),
                _mongoDBRepository.DeleteAllRoomsAsync(),
                _mongoDBRepository.DeleteAllClientsAsync(),

                _cassandraRepository.DeleteAllPaymentsAsync(),
                _cassandraRepository.DeleteAllReservationsServicesAsync(),
                _cassandraRepository.DeleteAllReservationsAsync(),
                _cassandraRepository.DeleteAllServicesAsync(),
                _cassandraRepository.DeleteAllRoomsAsync(),
                _cassandraRepository.DeleteAllClientsAsync()
            };

            await Task.WhenAll(deleteTasks);

            var random = new Random();

            var clients = new List<(
                int index,
                int pgId,
                int sqlId,
                string mongoId,
                Guid cassId,
                string firstName,
                string secondName,
                string lastName,
                string email,
                DateTime dob,
                string address,
                string phone,
                bool isActive)>();

            var rooms = new List<(
                int index,
                int pgId,
                int sqlId,
                string mongoId,
                Guid cassId,
                int number,
                int capacity,
                int pricePerNight,
                bool isActive)>();

            var services = new List<(
                int index,
                int pgId,
                int sqlId,
                string mongoId,
                Guid cassId,
                string name,
                int price,
                bool isActive)>();

            var reservations = new List<(
                int index,
                int pgId,
                int sqlId,
                string mongoId,
                Guid cassId,
                int clientPgId,
                int clientSqlId,
                string clientMongoId,
                Guid clientCassId,
                int roomPgId,
                int roomSqlId,
                string roomMongoId,
                Guid roomCassId,
                DateTime checkIn,
                DateTime checkOut,
                DateTime creationDate)>();

            for (int i = 1; i <= generateDataDTO.Count; i++)
            {
                var firstName = $"First{i}";
                var secondName = $"Second{i}";
                var lastName = $"Last{i}";
                var email = $"client{i}@example.com";
                var dob = DateTime.UtcNow.AddYears(-20 - random.Next(20));
                var address = $"Address {i}";
                var phone = $"555-00{i:D3}";
                var clientIsActive = random.Next(2) == 0;

                var pgClientId = await _postgreSQLRepository.CreateClientAsync(firstName, secondName, lastName, email, dob, address, phone, clientIsActive);
                var sqlClientId = await _sqlServerRepository.CreateClientAsync(firstName, secondName, lastName, email, dob, address, phone, clientIsActive);
                var mongoClientId = await _mongoDBRepository.CreateClientAsync(firstName, secondName, lastName, email, dob, address, phone, clientIsActive);
                var cassClientId = await _cassandraRepository.CreateClientAsync(firstName, secondName, lastName, email, dob, address, phone, clientIsActive);

                clients.Add((
                    i,
                    pgClientId,
                    sqlClientId,
                    mongoClientId,
                    cassClientId,
                    firstName,
                    secondName,
                    lastName,
                    email,
                    dob,
                    address,
                    phone,
                    clientIsActive));

                var number = 100 + i;
                var capacity = random.Next(1, 5);
                var price = random.Next(100, 400);
                var roomIsActive = random.Next(2) == 0;

                var pgRoomId = await _postgreSQLRepository.CreateRoomAsync(number, capacity, price, roomIsActive);
                var sqlRoomId = await _sqlServerRepository.CreateRoomAsync(number, capacity, price, roomIsActive);
                var mongoRoomId = await _mongoDBRepository.CreateRoomAsync(number, capacity, price, roomIsActive);
                var cassRoomId = await _cassandraRepository.CreateRoomAsync(number, capacity, price, roomIsActive);

                rooms.Add((i, pgRoomId, sqlRoomId, mongoRoomId, cassRoomId, number, capacity, price, roomIsActive));

                var serviceName = $"Service {i}";
                var servicePrice = random.Next(20, 120);
                var serviceIsActive = random.Next(2) == 0;

                var pgServiceId = await _postgreSQLRepository.CreateServiceAsync(serviceName, servicePrice, serviceIsActive);
                var sqlServiceId = await _sqlServerRepository.CreateServiceAsync(serviceName, servicePrice, serviceIsActive);
                var mongoServiceId = await _mongoDBRepository.CreateServiceAsync(serviceName, servicePrice, serviceIsActive);
                var cassServiceId = await _cassandraRepository.CreateServiceAsync(serviceName, servicePrice, serviceIsActive);

                services.Add((i, pgServiceId, sqlServiceId, mongoServiceId, cassServiceId, serviceName, servicePrice, serviceIsActive));
            }

            for (int i = 1; i <= generateDataDTO.Count; i++)
            {
                var client = clients[random.Next(clients.Count)];
                var room = rooms[random.Next(rooms.Count)];

                var checkIn = DateTime.UtcNow.AddDays(random.Next(1, 30));
                var checkOut = checkIn.AddDays(random.Next(2, 10));
                var creationDate = DateTime.UtcNow.AddDays(-random.Next(1, 10));

                var pgResId = await _postgreSQLRepository.CreateReservationAsync(client.pgId, room.pgId, checkIn, checkOut, creationDate);
                var sqlResId = await _sqlServerRepository.CreateReservationAsync(client.sqlId, room.sqlId, checkIn, checkOut, creationDate);
                var mongoResId = await _mongoDBRepository.CreateReservationAsync(client.mongoId, room.mongoId, checkIn, checkOut, creationDate);
                var cassResId = await _cassandraRepository.CreateReservationAsync(client.cassId, room.cassId, checkIn, checkOut, creationDate);

                reservations.Add((
                    i,
                    pgResId,
                    sqlResId,
                    mongoResId,
                    cassResId,
                    client.pgId,
                    client.sqlId,
                    client.mongoId,
                    client.cassId,
                    room.pgId,
                    room.sqlId,
                    room.mongoId,
                    room.cassId,
                    checkIn,
                    checkOut,
                    creationDate));

                var description = $"Payment for reservation {i}";
                var sum = random.Next(100, 800);
                var payDate = DateTime.UtcNow;

                await _postgreSQLRepository.CreatePaymentAsync(pgResId, description, sum, payDate);
                await _sqlServerRepository.CreatePaymentAsync(sqlResId, description, sum, payDate);
                await _mongoDBRepository.CreatePaymentAsync(mongoResId, description, sum, payDate);
                await _cassandraRepository.CreatePaymentAsync(cassResId, description, sum, payDate);
            }

            foreach (var res in reservations)
            {
                var chosenServices = services.OrderBy(_ => random.Next()).Take(1).ToList();

                foreach (var s in chosenServices)
                {
                    await _postgreSQLRepository.CreateReservationServiceAsync(res.pgId, s.pgId, DateTime.UtcNow);
                    await _sqlServerRepository.CreateReservationServiceAsync(res.sqlId, s.sqlId, DateTime.UtcNow);
                    await _mongoDBRepository.CreateReservationServiceAsync(res.mongoId, s.mongoId, DateTime.UtcNow);
                    await _cassandraRepository.CreateReservationServiceAsync(res.cassId, s.cassId, DateTime.UtcNow);
                }
            }

            return new ResponseDTO(SUCCESS.DATA_HAS_BEEN_GENERATED);
        }
    }
}