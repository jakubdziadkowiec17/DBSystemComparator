using DBSystemComparator_API.Constants;
using DBSystemComparator_API.Models.Collections;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using DBSystemComparator_API.Services.Interfaces;
using MongoDB.Bson;
using static DBSystemComparator_API.Repositories.Implementations.PostgreSQLRepository;

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
            await GenerateDataToSQLServerAndPostgreSQLAsync(generateDataDTO);
            await GenerateDataToMongoDBAsync();
            await GenerateDataToCassandraAsync();

            return new ResponseDTO(SUCCESS.DATA_HAS_BEEN_GENERATED);
        }

        private async Task GenerateDataToSQLServerAndPostgreSQLAsync(GenerateDataDTO generateDataDTO)
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

            var allClientIdsSQL = new List<int>();
            var allRoomIdsSQL = new List<int>();
            var allServiceIdsSQL = new List<int>();
            var allReservationIdsSQL = new List<int>();

            var allClientIdsPG = new List<int>();
            var allRoomIdsPG = new List<int>();
            var allServiceIdsPG = new List<int>();
            var allReservationIdsPG = new List<int>();

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
                    _postgreSQLRepository.CreateServicesBatchAsync(servicesBatch)
                );

                allClientIdsSQL = await _sqlServerRepository.GetAllClientIdsAsync();
                allRoomIdsSQL = await _sqlServerRepository.GetAllRoomIdsAsync();
                allServiceIdsSQL = await _sqlServerRepository.GetAllServiceIdsAsync();

                allClientIdsPG = await _postgreSQLRepository.GetAllClientIdsAsync();
                allRoomIdsPG = await _postgreSQLRepository.GetAllRoomIdsAsync();
                allServiceIdsPG = await _postgreSQLRepository.GetAllServiceIdsAsync();
            }

            for (int batchStart = 0; batchStart < generateDataDTO.Count; batchStart += batchSize)
            {
                int batchEnd = Math.Min(batchStart + batchSize, generateDataDTO.Count);

                var reservationsSQL = new List<(int, int, DateTime, DateTime, DateTime)>();
                var reservationsPG = new List<(int, int, DateTime, DateTime, DateTime)>();
                var reservationsMongo = new List<(ObjectId, ObjectId, DateTime, DateTime?, DateTime, List<Models.Collections.ServiceCollection>, List<PaymentEmbedded>)>();
                var reservationsCassandra = new List<(Guid, Guid, DateTime, DateTime, DateTime)>();

                for (int i = batchStart; i < batchEnd; i++)
                {
                    var checkIn = DateTime.Now.AddDays(-random.Next(1, 1000));
                    var checkOut = checkIn.AddDays(random.Next(1, 14));
                    var now = DateTime.Now;

                    var payments = new List<PaymentEmbedded>();
                    var services = new List<Models.Collections.ServiceCollection>();

                    reservationsSQL.Add((allClientIdsSQL[i], allRoomIdsSQL[i], checkIn, checkOut, now));
                    reservationsPG.Add((allClientIdsPG[i], allRoomIdsPG[i], checkIn, checkOut, now));
                }

                await Task.WhenAll(
                    _sqlServerRepository.CreateReservationsBatchAsync(reservationsSQL),
                    _postgreSQLRepository.CreateReservationsBatchAsync(reservationsPG)
                );

                allReservationIdsSQL = await _sqlServerRepository.GetAllReservationIdsAsync();
                allReservationIdsPG = await _postgreSQLRepository.GetAllReservationIdsAsync();
            }

            for (int batchStart = 0; batchStart < generateDataDTO.Count; batchStart += batchSize)
            {
                int batchEnd = Math.Min(batchStart + batchSize, generateDataDTO.Count);

                var resServicesSQL = new List<(int, int, DateTime)>();
                var resServicesPG = new List<(int, int, DateTime)>();

                for (int i = batchStart; i < batchEnd; i++)
                {
                    int serviceIdSQL;
                    int serviceIdPG;
                    var now = DateTime.Now;

                    if (allServiceIdsSQL.Count == allServiceIdsPG.Count)
                    {
                        int idx = random.Next(allServiceIdsSQL.Count);
                        serviceIdSQL = allServiceIdsSQL[idx];
                        serviceIdPG = allServiceIdsPG[idx];
                    }
                    else
                    {
                        serviceIdSQL = allServiceIdsSQL[random.Next(allServiceIdsSQL.Count)];
                        serviceIdPG = allServiceIdsPG[random.Next(allServiceIdsPG.Count)];
                    }

                    resServicesSQL.Add((allReservationIdsSQL[i], serviceIdSQL, now));
                    resServicesPG.Add((allReservationIdsPG[i], serviceIdPG, now));
                }

                await Task.WhenAll(
                    _sqlServerRepository.CreateReservationsServicesBatchAsync(resServicesSQL),
                    _postgreSQLRepository.CreateReservationsServicesBatchAsync(resServicesPG)
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
                }

                await Task.WhenAll(
                    _sqlServerRepository.CreatePaymentsBatchAsync(paymentsSQL),
                    _postgreSQLRepository.CreatePaymentsBatchAsync(paymentsPG)
                );
            }
        }

        private async Task GenerateDataToMongoDBAsync()
        {
            await Task.WhenAll(
                _mongoDBRepository.DeleteAllReservationsAsync(),
                _mongoDBRepository.DeleteAllClientsAsync(),
                _mongoDBRepository.DeleteAllRoomsAsync(),
                _mongoDBRepository.DeleteAllServicesAsync()
            );

            var clientsTask = _postgreSQLRepository.GetAllClientsAsync();
            var roomsTask = _postgreSQLRepository.GetAllRoomsAsync();
            var servicesTask = _postgreSQLRepository.GetAllServicesAsync();
            var reservationsTask = _postgreSQLRepository.GetAllReservationsAsync();
            var reservationServicesTask = _postgreSQLRepository.GetAllReservationsServicesAsync();
            var paymentsTask = _postgreSQLRepository.GetAllPaymentsAsync();

            await Task.WhenAll(clientsTask, roomsTask, servicesTask, reservationsTask, reservationServicesTask, paymentsTask);

            var clients = clientsTask.Result;
            var rooms = roomsTask.Result;
            var services = servicesTask.Result;
            var reservations = reservationsTask.Result;
            var reservationServices = reservationServicesTask.Result;
            var payments = paymentsTask.Result;

            var clientMap = clients.ToDictionary(c => c.Id, _ => ObjectId.GenerateNewId());
            var roomMap = rooms.ToDictionary(r => r.Id, _ => ObjectId.GenerateNewId());
            var serviceMap = services.ToDictionary(s => s.Id, _ => ObjectId.GenerateNewId());

            var mongoClients = clients.Select(c => new ClientCollection
            {
                Id = clientMap[c.Id],
                FirstName = c.FirstName,
                SecondName = c.SecondName,
                LastName = c.LastName,
                Email = c.Email,
                DateOfBirth = c.BirthDate,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                IsActive = c.IsActive
            }).ToList();

            var mongoRooms = rooms.Select(r => new RoomCollection
            {
                Id = roomMap[r.Id],
                Number = r.RoomNumber,
                Capacity = r.Floor,
                PricePerNight = r.Price,
                IsActive = r.IsAvailable
            }).ToList();

            var mongoServices = services.Select(s => new Models.Collections.ServiceCollection
            {
                Id = serviceMap[s.Id],
                Name = s.Name,
                Price = s.Price,
                IsActive = s.IsAvailable
            }).ToList();

            await Task.WhenAll(
                _mongoDBRepository.CreateClientsBatchAsync(mongoClients),
                _mongoDBRepository.CreateRoomsBatchAsync(mongoRooms),
                _mongoDBRepository.CreateServicesBatchAsync(mongoServices)
            );

            var mongoClientDict = mongoClients.ToDictionary(c => c.Id);
            var mongoRoomDict = mongoRooms.ToDictionary(r => r.Id);
            var serviceDict = services.ToDictionary(s => s.Id);

            var rsLookup = reservationServices.ToLookup(x => x.ReservationId, x => x.ServiceId);
            var paymentLookup = payments.ToLookup(x => x.ReservationId, x => x);

            const int batchSize = 10000;
            int total = reservations.Count;
            int processed = 0;

            var reservationCollection = _mongoDBRepository.GetReservationCollectionRaw();

            while (processed < total)
            {
                var batch = reservations.Skip(processed).Take(batchSize);
                var bsonDocs = new List<BsonDocument>(batchSize);

                Parallel.ForEach(batch, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, res =>
                {
                    var servicesBson = new BsonArray(
                        rsLookup[res.Id].Select(sid =>
                        {
                            var s = serviceDict[sid];
                            return new BsonDocument
                            {
                                { "_id", serviceMap[sid] },
                                { "name", s.Name },
                                { "price", s.Price },
                                { "isActive", s.IsAvailable }
                            };
                        })
                    );

                    var paymentsBson = new BsonArray(
                        paymentLookup[res.Id].Select(p => new BsonDocument
                        {
                            { "_id", ObjectId.GenerateNewId() },
                            { "description", p.Description },
                            { "sum", p.Amount },
                            { "creationDate", p.PaymentDate }
                        })
                    );

                    var doc = new BsonDocument
                    {
                        { "_id", ObjectId.GenerateNewId() },
                        { "client", new BsonDocument
                            {
                                { "_id", clientMap[res.ClientId] },
                                { "firstName", mongoClientDict[clientMap[res.ClientId]].FirstName },
                                { "secondName", mongoClientDict[clientMap[res.ClientId]].SecondName },
                                { "lastName", mongoClientDict[clientMap[res.ClientId]].LastName },
                                { "email", mongoClientDict[clientMap[res.ClientId]].Email },
                                { "dateOfBirth", mongoClientDict[clientMap[res.ClientId]].DateOfBirth },
                                { "address", mongoClientDict[clientMap[res.ClientId]].Address },
                                { "phoneNumber", mongoClientDict[clientMap[res.ClientId]].PhoneNumber },
                                { "isActive", mongoClientDict[clientMap[res.ClientId]].IsActive }
                            }
                        },
                        { "room", new BsonDocument
                            {
                                { "_id", roomMap[res.RoomId] },
                                { "number", mongoRoomDict[roomMap[res.RoomId]].Number },
                                { "capacity", mongoRoomDict[roomMap[res.RoomId]].Capacity },
                                { "pricePerNight", mongoRoomDict[roomMap[res.RoomId]].PricePerNight },
                                { "isActive", mongoRoomDict[roomMap[res.RoomId]].IsActive }
                            }
                        },
                        { "checkInDate", res.CheckInDate },
                        { "checkOutDate", res.CheckOutDate },
                        { "creationDate", res.CreationDate },
                        { "services", servicesBson },
                        { "payments", paymentsBson }
                    };

                    lock (bsonDocs)
                    {
                        bsonDocs.Add(doc);
                    }
                });

                await reservationCollection.InsertManyAsync(bsonDocs, new MongoDB.Driver.InsertManyOptions { IsOrdered = false });
                processed += bsonDocs.Count;
            }
        }

        private async Task GenerateDataToCassandraAsync()
        {
            await _cassandraRepository.DeleteAllAsync();

            var clients = await _postgreSQLRepository.GetAllClientsAsync();
            var rooms = await _postgreSQLRepository.GetAllRoomsAsync();
            var services = await _postgreSQLRepository.GetAllServicesAsync();
            var reservations = await _postgreSQLRepository.GetAllReservationsAsync();
            var reservationServices = await _postgreSQLRepository.GetAllReservationsServicesAsync();
            var payments = await _postgreSQLRepository.GetAllPaymentsAsync();

            var clientIdMap = clients.ToDictionary(c => c.Id, _ => Guid.NewGuid());
            var roomIdMap = rooms.ToDictionary(r => r.Id, _ => Guid.NewGuid());
            var serviceIdMap = services.ToDictionary(s => s.Id, _ => Guid.NewGuid());
            var reservationIdMap = reservations.ToDictionary(r => r.Id, _ => Guid.NewGuid());
            var paymentIdMap = payments.ToDictionary(p => p.Id, _ => Guid.NewGuid());

            var cassandraClients = clients.Select(c => new CassandraClientDTO
            {
                Id = clientIdMap[c.Id],
                FirstName = c.FirstName,
                SecondName = c.SecondName,
                LastName = c.LastName,
                Email = c.Email,
                DateOfBirth = c.BirthDate,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                IsActive = c.IsActive
            }).ToList();

            var cassandraActiveClients = clients.Select(c => new CassandraActiveClientDTO
            {
                Id = clientIdMap[c.Id],
                IsActive = c.IsActive,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                DateOfBirth = c.BirthDate,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber
            }).ToList();

            await _cassandraRepository.InsertClientsBatchAsync(cassandraClients);
            await _cassandraRepository.InsertActiveClientsBatchAsync(cassandraActiveClients);

            var cassandraRooms = rooms.Select(r => new CassandraRoomDTO
            {
                Id = roomIdMap[r.Id],
                Number = r.RoomNumber,
                Capacity = r.Floor,
                PricePerNight = r.Price,
                IsActive = r.IsAvailable
            }).ToList();

            var cassandraActiveRooms = rooms.Select(r => new CassandraActiveRoomDTO
            {
                Id = roomIdMap[r.Id],
                IsActive = r.IsAvailable,
                Number = r.RoomNumber,
                Capacity = r.Floor,
                PricePerNight = r.Price
            }).ToList();

            await _cassandraRepository.InsertRoomsBatchAsync(cassandraRooms);
            await _cassandraRepository.InsertActiveRoomsBatchAsync(cassandraActiveRooms);

            var cassandraServices = services.Select(s => new CassandraServiceDTO
            {
                Id = serviceIdMap[s.Id],
                Name = s.Name,
                Price = s.Price,
                IsActive = s.IsAvailable
            }).ToList();

            var cassandraActiveServices = services.Select(s => new CassandraActiveServiceDTO
            {
                Id = serviceIdMap[s.Id],
                IsActive = s.IsAvailable,
                Price = s.Price,
                Name = s.Name
            }).ToList();

            await _cassandraRepository.InsertServicesBatchAsync(cassandraServices);
            await _cassandraRepository.InsertActiveServicesBatchAsync(cassandraActiveServices);

            var reservationsByClient = reservations.Select(res => new CassandraReservationByClientDTO
            {
                ClientId = clientIdMap[res.ClientId],
                CreationDate = res.CreationDate,
                ReservationId = reservationIdMap[res.Id],
                RoomId = roomIdMap[res.RoomId],
                CheckInDate = res.CheckInDate,
                CheckOutDate = res.CheckOutDate
            }).ToList();

            var reservationsByRoom = reservations.Select(res => new CassandraReservationByRoomDTO
            {
                RoomId = roomIdMap[res.RoomId],
                CreationDate = res.CreationDate,
                ReservationId = reservationIdMap[res.Id],
                ClientId = clientIdMap[res.ClientId],
                CheckInDate = res.CheckInDate,
                CheckOutDate = res.CheckOutDate
            }).ToList();

            await _cassandraRepository.InsertReservationsByClientBatchAsync(reservationsByClient);
            await _cassandraRepository.InsertReservationsByRoomBatchAsync(reservationsByRoom);

            var cassandraPayments = payments.Select(p => new CassandraPaymentDTO
            {
                ReservationId = reservationIdMap[p.ReservationId],
                CreationDate = p.PaymentDate,
                PaymentId = paymentIdMap[p.Id],
                Description = p.Description,
                Sum = p.Amount
            }).ToList();

            await _cassandraRepository.InsertPaymentsByReservationBatchAsync(cassandraPayments);

            var rsByReservation = reservationServices.Select(rs => new CassandraReservationServiceByReservationDTO
            {
                ReservationId = reservationIdMap[rs.ReservationId],
                ServiceId = serviceIdMap[rs.ServiceId],
                CreationDate = rs.CreationDate
            }).ToList();

            var rsByService = reservationServices.Select(rs => new CassandraReservationServiceByServiceDTO
            {
                ServiceId = serviceIdMap[rs.ServiceId],
                ReservationId = reservationIdMap[rs.ReservationId],
                CreationDate = rs.CreationDate
            }).ToList();

            await _cassandraRepository.InsertReservationServicesByReservationBatchAsync(rsByReservation);
            await _cassandraRepository.InsertReservationServicesByServiceBatchAsync(rsByService);
        }
    }
}