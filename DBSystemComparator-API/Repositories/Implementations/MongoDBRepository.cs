using DBSystemComparator_API.Database;
using DBSystemComparator_API.Models.Collections;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DBSystemComparator_API.Repositories.Implementations
{
    public class MongoDBRepository : IMongoDBRepository
    {
        private readonly MongoDbContext _context;

        public MongoDBRepository(MongoDbContext context)
        {
            _context = context;
        }

        // CREATE

        public async Task<string> CreateClientAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive)
        {
            var client = new ClientCollection
            {
                FirstName = firstName,
                SecondName = secondName,
                LastName = lastName,
                Email = email,
                DateOfBirth = dob,
                Address = address,
                PhoneNumber = phone,
                IsActive = isActive
            };
            await _context.Clients.InsertOneAsync(client);
            return client.Id.ToString();
        }

        public async Task<string> CreateRoomAsync(int number, int capacity, int pricePerNight, bool isActive)
        {
            var room = new RoomCollection
            {
                Number = number,
                Capacity = capacity,
                PricePerNight = pricePerNight,
                IsActive = isActive
            };
            await _context.Rooms.InsertOneAsync(room);
            return room.Id.ToString();
        }

        public async Task<string> CreateServiceAsync(string name, int price, bool isActive)
        {
            var service = new Models.Collections.ServiceCollection
            {
                Name = name,
                Price = price,
                IsActive = isActive
            };
            await _context.Services.InsertOneAsync(service);
            return service.Id.ToString();
        }

        public async Task<List<string>> CreateClientsAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive, int count)
        {
            var clients = Enumerable.Range(0, count)
                .Select(_ => new ClientCollection
                {
                    FirstName = firstName,
                    SecondName = secondName,
                    LastName = lastName,
                    Email = email,
                    DateOfBirth = dob,
                    Address = address,
                    PhoneNumber = phone,
                    IsActive = isActive
                }).ToList();

            await _context.Clients.InsertManyAsync(clients);
            return clients.Select(c => c.Id.ToString()).ToList();
        }

        public async Task<List<string>> CreateRoomsAsync(int number, int capacity, int pricePerNight, bool isActive, int count)
        {
            var rooms = Enumerable.Range(0, count)
                .Select(_ => new RoomCollection
                {
                    Number = number,
                    Capacity = capacity,
                    PricePerNight = pricePerNight,
                    IsActive = isActive
                }).ToList();

            await _context.Rooms.InsertManyAsync(rooms);
            return rooms.Select(r => r.Id.ToString()).ToList();
        }

        // READ

        public async Task<List<Dictionary<string, object>>> ReadReservationsAfter2024Async()
        {
            var filter = Builders<ReservationCollection>.Filter.Gt(r => r.CheckInDate, new DateTime(2024, 1, 1));
            var reservations = await _context.Reservations.Find(filter).ToListAsync();

            return reservations.Select(r => new Dictionary<string, object>
            {
                ["ReservationId"] = r.Id.ToString(),
                ["CheckInDate"] = r.CheckInDate,
                ["CheckOutDate"] = r.CheckOutDate,
                ["FirstName"] = r.Client.FirstName,
                ["LastName"] = r.Client.LastName
            }).ToList();
        }

        public async Task<List<Dictionary<string, object>>> ReadReservationsWithPaymentsAboveAsync(int minSum)
        {
            var filter = Builders<ReservationCollection>.Filter.ElemMatch(r => r.Payments, p => p.Sum > minSum);
            var reservations = await _context.Reservations.Find(filter).ToListAsync();

            var result = new List<Dictionary<string, object>>();
            foreach (var r in reservations)
            {
                foreach (var p in r.Payments.Where(p => p.Sum > minSum))
                {
                    result.Add(new Dictionary<string, object>
                    {
                        ["ReservationId"] = r.Id.ToString(),
                        ["CheckInDate"] = r.CheckInDate,
                        ["CheckOutDate"] = r.CheckOutDate,
                        ["Sum"] = p.Sum,
                        ["FirstName"] = r.Client.FirstName,
                        ["LastName"] = r.Client.LastName
                    });
                }
            }

            return result;
        }

        public async Task<List<Dictionary<string, object>>> ReadClientsWithActiveReservationsAsync()
        {
            var filter = Builders<ReservationCollection>.Filter.Or(
                Builders<ReservationCollection>.Filter.Eq(r => r.CheckOutDate, null),
                Builders<ReservationCollection>.Filter.Gte(r => r.CheckOutDate, DateTime.Now)
            );

            var reservations = await _context.Reservations.Find(filter).ToListAsync();

            return reservations
                .Select(r => new Dictionary<string, object>
                {
                    ["Id"] = r.Client.Id.ToString(),
                    ["FirstName"] = r.Client.FirstName,
                    ["LastName"] = r.Client.LastName,
                    ["Email"] = r.Client.Email
                })
                .GroupBy(d => d["Id"])
                .Select(g => g.First())
                .ToList();
        }

        public async Task<List<Dictionary<string, object>>> ReadActiveServicesUsedInReservationsAsync()
        {
            var reservations = await _context.Reservations.Find(_ => true).ToListAsync();

            var services = reservations
                .SelectMany(r => r.Services)
                .Where(s => s.IsActive)
                .GroupBy(s => s.Id)
                .Select(g => g.First())
                .Select(s => new Dictionary<string, object>
                {
                    ["Id"] = s.Id.ToString(),
                    ["Name"] = s.Name,
                    ["Price"] = s.Price
                })
                .ToList();

            return services;
        }

        public async Task<List<Dictionary<string, object>>> ReadCapacityReservationsAsync(int capacityThreshold)
        {
            var filter = Builders<ReservationCollection>.Filter.Gt(r => r.Room.Capacity, capacityThreshold);
            var reservations = await _context.Reservations.Find(filter).ToListAsync();

            return reservations.Select(r => new Dictionary<string, object>
            {
                ["ReservationId"] = r.Id.ToString(),
                ["CheckInDate"] = r.CheckInDate,
                ["CheckOutDate"] = r.CheckOutDate,
                ["FirstName"] = r.Client.FirstName,
                ["LastName"] = r.Client.LastName,
                ["RoomNumber"] = r.Room.Number,
                ["Capacity"] = r.Room.Capacity
            }).ToList();
        }

        // UPDATE

        public async Task<long> UpdateClientsAddressAndPhoneAsync(bool isActive)
        {
            var filterClients = Builders<ClientCollection>.Filter.Eq(c => c.IsActive, isActive);
            var updateClients = Builders<ClientCollection>.Update
                .Set(c => c.Address, "Cracow, ul. abc 4")
                .Set(c => c.PhoneNumber, "123456789");

            var clientsResult = await _context.Clients.UpdateManyAsync(filterClients, updateClients);

            var filterReservations = Builders<ReservationCollection>.Filter.Eq(r => r.Client.IsActive, isActive);
            var updateReservations = Builders<ReservationCollection>.Update
                .Set("client.address", "Cracow, ul. abc 4")
                .Set("client.phoneNumber", "123456789");

            await _context.Reservations.UpdateManyAsync(filterReservations, updateReservations);

            return clientsResult.ModifiedCount;
        }

        public async Task<long> UpdateRoomsPriceForReservationsAsync(int minCapacity, int priceIncrement)
        {
            var filterRooms = Builders<RoomCollection>.Filter.Gte(r => r.Capacity, minCapacity);
            var updateRooms = Builders<RoomCollection>.Update.Inc(r => r.PricePerNight, priceIncrement);
            var roomsResult = await _context.Rooms.UpdateManyAsync(filterRooms, updateRooms);

            var filterReservations = Builders<ReservationCollection>.Filter.Gte(r => r.Room.Capacity, minCapacity);
            var updateReservations = Builders<ReservationCollection>.Update.Inc("room.pricePerNight", priceIncrement);

            await _context.Reservations.UpdateManyAsync(filterReservations, updateReservations);

            return roomsResult.ModifiedCount;
        }

        public async Task<long> UpdateServicesPriceAsync(int priceIncrement, bool isActive)
        {
            var filterServices = Builders<Models.Collections.ServiceCollection>.Filter.Eq(s => s.IsActive, isActive);
            var updateServices = Builders<Models.Collections.ServiceCollection>.Update.Inc(s => s.Price, priceIncrement);
            var svcResult = await _context.Services.UpdateManyAsync(filterServices, updateServices);

            var filterReservations = Builders<ReservationCollection>.Filter.ElemMatch(r => r.Services, s => s.IsActive == isActive);
            var updateReservations = Builders<ReservationCollection>.Update
                .Inc("services.$[s].price", priceIncrement);

            var options = new UpdateOptions
            {
                ArrayFilters = new List<ArrayFilterDefinition>
                {
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("s.isActive", isActive))
                }
            };

            await _context.Reservations.UpdateManyAsync(filterReservations, updateReservations, options);

            return svcResult.ModifiedCount;
        }

        public async Task<long> UpdatePriceForInactiveRoomsAsync(double discountMultiplier)
        {
            var filterRooms = Builders<RoomCollection>.Filter.Eq(r => r.IsActive, false);
            var updateRooms = Builders<RoomCollection>.Update.Mul(r => r.PricePerNight, discountMultiplier);
            var roomsResult = await _context.Rooms.UpdateManyAsync(filterRooms, updateRooms);

            var filterReservations = Builders<ReservationCollection>.Filter.Eq(r => r.Room.IsActive, false);
            var updateReservations = Builders<ReservationCollection>.Update.Mul("room.pricePerNight", discountMultiplier);

            await _context.Reservations.UpdateManyAsync(filterReservations, updateReservations);

            return roomsResult.ModifiedCount;
        }

        public async Task<long> UpdateRoomsPriceForReservationsTo2024Async(int priceDecrement)
        {
            var filterReservations = Builders<ReservationCollection>.Filter.Lt(r => r.CheckInDate, new DateTime(2024, 1, 1));
            var roomIds = await _context.Reservations.Distinct(r => r.Room.Id, filterReservations).ToListAsync();

            var filterRooms = Builders<RoomCollection>.Filter.In(r => r.Id, roomIds);
            var updateRooms = Builders<RoomCollection>.Update.Inc(r => r.PricePerNight, -priceDecrement);
            var roomsResult = await _context.Rooms.UpdateManyAsync(filterRooms, updateRooms);

            var filterRes2 = Builders<ReservationCollection>.Filter.In(r => r.Room.Id, roomIds);
            var updateRes2 = Builders<ReservationCollection>.Update.Inc("room.pricePerNight", -priceDecrement);

            await _context.Reservations.UpdateManyAsync(filterRes2, updateRes2);

            return roomsResult.ModifiedCount;
        }

        // DELETE

        public async Task<long> DeletePaymentsOlderThan2024Async()
        {
            var filterReservations = Builders<ReservationCollection>.Filter.Lt(r => r.CheckInDate, new DateTime(2024, 1, 1));
            var update = Builders<ReservationCollection>.Update.Set(r => r.Payments, new List<PaymentEmbedded>());

            var result = await _context.Reservations.UpdateManyAsync(filterReservations, update);
            return result.ModifiedCount;
        }

        public async Task<long> DeleteReservationsWithoutPaymentAsync()
        {
            var filter = Builders<ReservationCollection>.Filter.Size(r => r.Payments, 0);
            var result = await _context.Reservations.DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        public async Task<long> DeleteReservationsServicesOlderThan2024Async()
        {
            var filterReservations = Builders<ReservationCollection>.Filter.Lt(r => r.CheckInDate, new DateTime(2024, 1, 1));
            var update = Builders<ReservationCollection>.Update.Set(r => r.Services, new List<Models.Collections.ServiceCollection>());

            var result = await _context.Reservations.UpdateManyAsync(filterReservations, update);
            return result.ModifiedCount;
        }

        public async Task<long> DeleteReservationsServicesWithServicePriceBelowAsync(int price)
        {
            var serviceIds = await _context.Services.Find(s => s.Price < price).Project(s => s.Id).ToListAsync();

            if (!serviceIds.Any()) return 0;

            var filterReservations = Builders<ReservationCollection>.Filter.ElemMatch(r => r.Services, s => serviceIds.Contains(s.Id));
            var update = Builders<ReservationCollection>.Update.PullFilter(r => r.Services, s => serviceIds.Contains(s.Id));

            var result = await _context.Reservations.UpdateManyAsync(filterReservations, update);
            return result.ModifiedCount;
        }

        public async Task<long> DeleteUnusedServicesAsync()
        {
            var usedServiceIds = _context.Reservations.AsQueryable().SelectMany(r => r.Services).Select(s => s.Id).Distinct().ToList();

            var filter = Builders<Models.Collections.ServiceCollection>.Filter.Nin(s => s.Id, usedServiceIds);
            var result = await _context.Services.DeleteManyAsync(filter);

            return result.DeletedCount;
        }

        // HELPERS

        public async Task CreateClientsBatchAsync(IEnumerable<ClientCollection> clients)
        {
            if (clients == null || !clients.Any()) return;
            await _context.Clients.InsertManyAsync(clients);
        }

        public async Task CreateRoomsBatchAsync(IEnumerable<RoomCollection> rooms)
        {
            if (rooms == null || !rooms.Any()) return;
            await _context.Rooms.InsertManyAsync(rooms);
        }

        public async Task CreateServicesBatchAsync(IEnumerable<Models.Collections.ServiceCollection> services)
        {
            if (services == null || !services.Any()) return;
            await _context.Services.InsertManyAsync(services);
        }

        public async Task CreateReservationsBatchAsync(IEnumerable<ReservationCollection> reservations)
        {
            if (reservations == null || !reservations.Any()) return;
            await _context.Reservations.InsertManyAsync(reservations);
        }

        public async Task<TablesCountDTO> GetTablesCountAsync()
        {
            var clientsCount = await _context.Clients.CountDocumentsAsync(FilterDefinition<ClientCollection>.Empty);
            var roomsCount = await _context.Rooms.CountDocumentsAsync(FilterDefinition<RoomCollection>.Empty);
            var servicesCount = await _context.Services.CountDocumentsAsync(FilterDefinition<Models.Collections.ServiceCollection>.Empty);
            var reservationsCount = await _context.Reservations.CountDocumentsAsync(FilterDefinition<ReservationCollection>.Empty);

            var paymentsPipeline = new[]
            {
                new BsonDocument("$project", new BsonDocument("paymentsCount", new BsonDocument("$size", "$payments"))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "totalPayments", new BsonDocument("$sum", "$paymentsCount") }
                })
            };

            var paymentsResult = await _context.Reservations.Aggregate<BsonDocument>(paymentsPipeline).FirstOrDefaultAsync();
            var totalPaymentsCount = paymentsResult != null ? paymentsResult["totalPayments"].AsInt32 : 0;

            var servicesPipeline = new[]
            {
                new BsonDocument("$project", new BsonDocument("servicesCount", new BsonDocument("$size", "$services"))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "totalServices", new BsonDocument("$sum", "$servicesCount") }
                })
            };

            var servicesResult = await _context.Reservations.Aggregate<BsonDocument>(servicesPipeline).FirstOrDefaultAsync();
            var totalReservationServicesCount = servicesResult != null ? servicesResult["totalServices"].AsInt32 : 0;

            return new TablesCountDTO()
            {
                ClientsCount = (int)clientsCount,
                RoomsCount = (int)roomsCount,
                ServicesCount = (int)servicesCount,
                ReservationsCount = (int)reservationsCount,
                PaymentsCount = totalPaymentsCount,
                ReservationsServicesCount = totalReservationServicesCount
            };
        }

        public async Task<List<string>> GetAllClientIdsAsync()
        {
            var ids = await _context.Clients.Find(Builders<ClientCollection>.Filter.Empty)
                                           .Project(c => c.Id)
                                           .ToListAsync();
            return ids.Select(id => id.ToString()).ToList();
        }

        public async Task<List<string>> GetAllRoomIdsAsync()
        {
            var ids = await _context.Rooms.Find(Builders<RoomCollection>.Filter.Empty)
                                          .Project(r => r.Id)
                                          .ToListAsync();
            return ids.Select(id => id.ToString()).ToList();
        }

        public async Task<List<string>> GetAllServiceIdsAsync()
        {
            var ids = await _context.Services.Find(Builders<Models.Collections.ServiceCollection>.Filter.Empty)
                                             .Project(s => s.Id)
                                             .ToListAsync();
            return ids.Select(id => id.ToString()).ToList();
        }

        public async Task<List<string>> GetAllReservationIdsAsync()
        {
            var ids = await _context.Reservations.Find(Builders<ReservationCollection>.Filter.Empty)
                                                 .Project(r => r.Id)
                                                 .ToListAsync();
            return ids.Select(id => id.ToString()).ToList();
        }

        public IMongoCollection<BsonDocument> GetReservationCollectionRaw()
        {
            return _context.Database.GetCollection<BsonDocument>("Reservations");
        }

        public async Task<long> DeleteAllClientsAsync()
        {
            var result = await _context.Clients.DeleteManyAsync(FilterDefinition<ClientCollection>.Empty);
            return result.DeletedCount;
        }

        public async Task<long> DeleteAllRoomsAsync()
        {
            var result = await _context.Rooms.DeleteManyAsync(FilterDefinition<RoomCollection>.Empty);
            return result.DeletedCount;
        }

        public async Task<long> DeleteAllServicesAsync()
        {
            var result = await _context.Services.DeleteManyAsync(FilterDefinition<Models.Collections.ServiceCollection>.Empty);
            return result.DeletedCount;
        }

        public async Task<long> DeleteAllReservationsAsync()
        {
            var result = await _context.Reservations.DeleteManyAsync(FilterDefinition<ReservationCollection>.Empty);
            return result.DeletedCount;
        }
    }
}