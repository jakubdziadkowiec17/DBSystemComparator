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
            int batchSize = 5000;
            var random = new Random();

            await _sqlServerRepository.DeleteAllClientsAsync();
            await _sqlServerRepository.DeleteAllRoomsAsync();
            await _sqlServerRepository.DeleteAllServicesAsync();
            await _sqlServerRepository.DeleteAllReservationsAsync();
            await _sqlServerRepository.DeleteAllReservationsServicesAsync();
            await _sqlServerRepository.DeleteAllPaymentsAsync();

            await _postgreSQLRepository.DeleteAllClientsAsync();
            await _postgreSQLRepository.DeleteAllRoomsAsync();
            await _postgreSQLRepository.DeleteAllServicesAsync();
            await _postgreSQLRepository.DeleteAllReservationsAsync();
            await _postgreSQLRepository.DeleteAllReservationsServicesAsync();
            await _postgreSQLRepository.DeleteAllPaymentsAsync();

            await _mongoDBRepository.DeleteAllClientsAsync();
            await _mongoDBRepository.DeleteAllRoomsAsync();
            await _mongoDBRepository.DeleteAllServicesAsync();
            await _mongoDBRepository.DeleteAllReservationsAsync();
            await _mongoDBRepository.DeleteAllReservationsServicesAsync();
            await _mongoDBRepository.DeleteAllPaymentsAsync();

            await _cassandraRepository.DeleteAllClientsAsync();
            await _cassandraRepository.DeleteAllRoomsAsync();
            await _cassandraRepository.DeleteAllServicesAsync();
            await _cassandraRepository.DeleteAllReservationsAsync();
            await _cassandraRepository.DeleteAllReservationsServicesAsync();
            await _cassandraRepository.DeleteAllPaymentsAsync();

            var allClientIdsSQL = new List<int>();
            var allRoomIdsSQL = new List<int>();
            var allServiceIdsSQL = new List<int>();
            var allReservationIdsSQL = new List<int>();

            var allClientIdsPG = new List<int>();
            var allRoomIdsPG = new List<int>();
            var allServiceIdsPG = new List<int>();
            var allReservationIdsPG = new List<int>();

            var allClientIdsMongo = new List<string>();
            var allRoomIdsMongo = new List<string>();
            var allServiceIdsMongo = new List<string>();
            var allReservationIdsMongo = new List<string>();

            var allClientIdsCassandra = new List<Guid>();
            var allRoomIdsCassandra = new List<Guid>();
            var allServiceIdsCassandra = new List<Guid>();
            var allReservationIdsCassandra = new List<Guid>();

            for (int batchStart = 1; batchStart <= generateDataDTO.Count; batchStart += batchSize)
            {
                int batchEnd = Math.Min(batchStart + batchSize - 1, generateDataDTO.Count);

                var clientsBatch = new List<(string, string, string, string, DateTime, string, string, bool)>();
                var roomsBatch = new List<(int, int, int, bool)>();
                var servicesBatch = new List<(string, int, bool)>();

                for (int i = batchStart; i <= batchEnd; i++)
                {
                    clientsBatch.Add(($"FirstName {i}", $"SecondName {i}", $"LastName {i}", $"email{i}@email.com", DateTime.Now.AddYears(-20 - random.Next(20)), $"Address {i}", random.Next(900000000, 999999999).ToString(), random.Next(2) == 0));
                    roomsBatch.Add((100 + i, random.Next(1, 10), random.Next(50, 5000), random.Next(2) == 0));
                    servicesBatch.Add(($"Service {i}", random.Next(10, 200), random.Next(2) == 0));
                }

                await Task.WhenAll(
                    _sqlServerRepository.CreateClientsBatchAsync(clientsBatch),
                    _sqlServerRepository.CreateRoomsBatchAsync(roomsBatch),
                    _sqlServerRepository.CreateServicesBatchAsync(servicesBatch),

                    _postgreSQLRepository.CreateClientsBatchAsync(clientsBatch),
                    _postgreSQLRepository.CreateRoomsBatchAsync(roomsBatch),
                    _postgreSQLRepository.CreateServicesBatchAsync(servicesBatch),

                    _mongoDBRepository.CreateClientsBatchAsync(clientsBatch),
                    _mongoDBRepository.CreateRoomsBatchAsync(roomsBatch),
                    _mongoDBRepository.CreateServicesBatchAsync(servicesBatch),

                    _cassandraRepository.CreateClientsBatchAsync(clientsBatch),
                    _cassandraRepository.CreateRoomsBatchAsync(roomsBatch),
                    _cassandraRepository.CreateServicesBatchAsync(servicesBatch)
                );

                allClientIdsSQL = await _sqlServerRepository.GetAllClientIdsAsync();
                allRoomIdsSQL = await _sqlServerRepository.GetAllRoomIdsAsync();
                allServiceIdsSQL = await _sqlServerRepository.GetAllServiceIdsAsync();

                allClientIdsPG = await _postgreSQLRepository.GetAllClientIdsAsync();
                allRoomIdsPG = await _postgreSQLRepository.GetAllRoomIdsAsync();
                allServiceIdsPG = await _postgreSQLRepository.GetAllServiceIdsAsync();

                allClientIdsMongo = await _mongoDBRepository.GetAllClientIdsAsync();
                allRoomIdsMongo = await _mongoDBRepository.GetAllRoomIdsAsync();
                allServiceIdsMongo = await _mongoDBRepository.GetAllServiceIdsAsync();

                allClientIdsCassandra = await _cassandraRepository.GetAllClientIdsAsync();
                allRoomIdsCassandra = await _cassandraRepository.GetAllRoomIdsAsync();
                allServiceIdsCassandra = await _cassandraRepository.GetAllServiceIdsAsync();
            }

            for (int batchStart = 0; batchStart < generateDataDTO.Count; batchStart += batchSize)
            {
                int batchEnd = Math.Min(batchStart + batchSize, generateDataDTO.Count);

                var reservationsSQL = new List<(int, int, DateTime, DateTime, DateTime)>();
                var reservationsPG = new List<(int, int, DateTime, DateTime, DateTime)>();
                var reservationsMongo = new List<(string, string, DateTime, DateTime, DateTime)>();
                var reservationsCassandra = new List<(Guid, Guid, DateTime, DateTime, DateTime)>();

                for (int i = batchStart; i < batchEnd; i++)
                {
                    var checkIn = DateTime.Now.AddDays(-random.Next(1, 1000));
                    var checkOut = checkIn.AddDays(random.Next(1, 14));
                    var now = DateTime.Now;

                    reservationsSQL.Add((allClientIdsSQL[i], allRoomIdsSQL[i], checkIn, checkOut, now));
                    reservationsPG.Add((allClientIdsPG[i], allRoomIdsPG[i], checkIn, checkOut, now));
                    reservationsMongo.Add((allClientIdsMongo[i], allRoomIdsMongo[i], checkIn, checkOut, now));
                    reservationsCassandra.Add((allClientIdsCassandra[i], allRoomIdsCassandra[i], checkIn, checkOut, now));
                }

                await Task.WhenAll(
                    _sqlServerRepository.CreateReservationsBatchAsync(reservationsSQL),
                    _postgreSQLRepository.CreateReservationsBatchAsync(reservationsPG),
                    _mongoDBRepository.CreateReservationsBatchAsync(reservationsMongo),
                    _cassandraRepository.CreateReservationsBatchAsync(reservationsCassandra)
                );

                allReservationIdsSQL = await _sqlServerRepository.GetAllReservationIdsAsync();
                allReservationIdsPG = await _postgreSQLRepository.GetAllReservationIdsAsync();
                allReservationIdsMongo = await _mongoDBRepository.GetAllReservationIdsAsync();
                allReservationIdsCassandra = await _cassandraRepository.GetAllReservationIdsAsync();
            }

            for (int batchStart = 0; batchStart < generateDataDTO.Count; batchStart += batchSize)
            {
                int batchEnd = Math.Min(batchStart + batchSize, generateDataDTO.Count);

                var resServicesSQL = new List<(int, int, DateTime)>();
                var resServicesPG = new List<(int, int, DateTime)>();
                var resServicesMongo = new List<(string, string, DateTime)>();
                var resServicesCassandra = new List<(Guid, Guid, DateTime)>();

                for (int i = batchStart; i < batchEnd; i++)
                {
                    int serviceIdSQL;
                    int serviceIdPG;
                    string serviceIdMongo;
                    Guid serviceIdCassandra;
                    var now = DateTime.Now;

                    if (allServiceIdsSQL.Count == allServiceIdsPG.Count && allServiceIdsPG.Count == allServiceIdsMongo.Count && allServiceIdsMongo.Count == allServiceIdsCassandra.Count)
                    {
                        int idx = random.Next(allServiceIdsSQL.Count);
                        serviceIdSQL = allServiceIdsSQL[idx];
                        serviceIdPG = allServiceIdsPG[idx];
                        serviceIdMongo = allServiceIdsMongo[idx];
                        serviceIdCassandra = allServiceIdsCassandra[idx];
                    }
                    else
                    {
                        serviceIdSQL = allServiceIdsSQL[random.Next(allServiceIdsSQL.Count)];
                        serviceIdPG = allServiceIdsPG[random.Next(allServiceIdsPG.Count)];
                        serviceIdMongo = allServiceIdsMongo[random.Next(allServiceIdsMongo.Count)];
                        serviceIdCassandra = allServiceIdsCassandra[random.Next(allServiceIdsCassandra.Count)];
                    }

                    resServicesSQL.Add((allReservationIdsSQL[i], serviceIdSQL, now));
                    resServicesPG.Add((allReservationIdsPG[i], serviceIdPG, now));
                    resServicesMongo.Add((allReservationIdsMongo[i], serviceIdMongo, now));
                    resServicesCassandra.Add((allReservationIdsCassandra[i], serviceIdCassandra, now));
                }

                await Task.WhenAll(
                    _sqlServerRepository.CreateReservationsServicesBatchAsync(resServicesSQL),
                    _postgreSQLRepository.CreateReservationsServicesBatchAsync(resServicesPG),
                    _mongoDBRepository.CreateReservationsServicesBatchAsync(resServicesMongo),
                    _cassandraRepository.CreateReservationsServicesBatchAsync(resServicesCassandra)
                );
            }

            for (int batchStart = 0; batchStart < generateDataDTO.Count; batchStart += batchSize)
            {
                int batchEnd = Math.Min(batchStart + batchSize, generateDataDTO.Count);

                var paymentsSQL = new List<(int, string, int, DateTime)>();
                var paymentsPG = new List<(int, string, int, DateTime)>();
                var paymentsMongo = new List<(string, string, int, DateTime)>();
                var paymentsCassandra = new List<(Guid, string, int, DateTime)>();

                for (int i = batchStart; i < batchEnd; i++)
                {
                    var sum = random.Next(500, 5000);
                    var now = DateTime.Now;

                    paymentsSQL.Add((allReservationIdsSQL[i], $"Payment {i}", sum, now));
                    paymentsPG.Add((allReservationIdsPG[i], $"Payment {i}", sum, now));
                    paymentsMongo.Add((allReservationIdsMongo[i], $"Payment {i}", sum, now));
                    paymentsCassandra.Add((allReservationIdsCassandra[i], $"Payment {i}", sum, now));
                }

                await Task.WhenAll(
                    _sqlServerRepository.CreatePaymentsBatchAsync(paymentsSQL),
                    _postgreSQLRepository.CreatePaymentsBatchAsync(paymentsPG),
                    _mongoDBRepository.CreatePaymentsBatchAsync(paymentsMongo),
                    _cassandraRepository.CreatePaymentsBatchAsync(paymentsCassandra)
                );
            }

            return new ResponseDTO(SUCCESS.DATA_HAS_BEEN_GENERATED);
        }
    }
}