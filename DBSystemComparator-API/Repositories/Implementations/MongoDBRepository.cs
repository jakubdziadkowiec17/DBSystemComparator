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

        public async Task<string> CreateRoomAsync(int number, int capacity, double pricePerNight, bool isActive)
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

        public async Task<List<string>> CreateRoomsAsync(int number, int capacity, double pricePerNight, bool isActive, int count)
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

        public async Task<List<Dictionary<string, object>>> ReadReservationsAfterSecondHalf2025Async()
        {
            var filter = Builders<ReservationCollection>.Filter.Gt(r => r.CheckInDate, new DateTime(2025, 06, 30));
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

        public async Task<List<Dictionary<string, object>>> ReadActiveServicesUsedInReservationsAsync(int minSum)
        {
            var pipeline = new[]
            {
                new BsonDocument("$unwind", "$services"),
                new BsonDocument("$match", new BsonDocument
                {
                    { "services.isActive", true },
                    { "services.price", new BsonDocument("$gt", minSum) }
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$services._id" },
                    { "Name", new BsonDocument("$first", "$services.name") },
                    { "Price", new BsonDocument("$first", "$services.price") }
                })
            };

            var result = await _context.Reservations.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return result.Select(s => new Dictionary<string, object>
            {
                ["Id"] = s["_id"].ToString(),
                ["Name"] = s["Name"].AsString,
                ["Price"] = s["Price"].ToInt32()
            }).ToList();
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

        public async Task<long> UpdateClientsAddressAndPhoneAsync(bool isActive, DateTime dateThreshold)
        {
            var filterClients = Builders<ClientCollection>.Filter.And(
                Builders<ClientCollection>.Filter.Eq(c => c.IsActive, isActive),
                Builders<ClientCollection>.Filter.Gt(c => c.DateOfBirth, dateThreshold)
            );

            var updateClients = Builders<ClientCollection>.Update
                .Set(c => c.Address, "Cracow, ul. abc 4")
                .Set(c => c.PhoneNumber, "123456789");

            var clientsResult = await _context.Clients.UpdateManyAsync(filterClients, updateClients);

            var filterReservations = Builders<ReservationCollection>.Filter.And(
                Builders<ReservationCollection>.Filter.Eq(r => r.Client.IsActive, isActive),
                Builders<ReservationCollection>.Filter.Gt(r => r.Client.DateOfBirth, dateThreshold)
            );

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

        public async Task<long> UpdateServicesPriceAsync(int priceIncrement, bool isActive, int price)
        {
            var filterServices = Builders<Models.Collections.ServiceCollection>.Filter.And(
                Builders<Models.Collections.ServiceCollection>.Filter.Eq(s => s.IsActive, isActive),
                Builders<Models.Collections.ServiceCollection>.Filter.Gt(s => s.Price, price)
            );

            var updateServices = Builders<Models.Collections.ServiceCollection>.Update.Inc(s => s.Price, priceIncrement);
            var svcResult = await _context.Services.UpdateManyAsync(filterServices, updateServices);

            var filterReservations = Builders<ReservationCollection>.Filter.ElemMatch(
                r => r.Services,
                s => s.IsActive == isActive && s.Price > price
            );

            var updateReservations = Builders<ReservationCollection>.Update.Inc("services.$[s].price", priceIncrement);

            var options = new UpdateOptions
            {
                ArrayFilters = new List<ArrayFilterDefinition>
                {
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(
                        new BsonDocument
                        {
                            { "s.isActive", isActive },
                            { "s.price", new BsonDocument("$gt", price) }
                        })
                }
            };

            await _context.Reservations.UpdateManyAsync(filterReservations, updateReservations, options);

            return svcResult.ModifiedCount;
        }

        public async Task<long> UpdatePriceForInactiveRoomsAsync(double discountMultiplier, double pricePerNight)
        {
            var filterRooms = Builders<RoomCollection>.Filter.And(
                Builders<RoomCollection>.Filter.Eq(r => r.IsActive, false),
                Builders<RoomCollection>.Filter.Gt(r => r.PricePerNight, pricePerNight)
            );

            var updateRooms = Builders<RoomCollection>.Update.Mul(r => r.PricePerNight, discountMultiplier);
            var roomsResult = await _context.Rooms.UpdateManyAsync(filterRooms, updateRooms);

            var filterReservations = Builders<ReservationCollection>.Filter.And(
                Builders<ReservationCollection>.Filter.Eq(r => r.Room.IsActive, false),
                Builders<ReservationCollection>.Filter.Gt(r => r.Room.PricePerNight, pricePerNight)
            );

            var updateReservations = Builders<ReservationCollection>.Update.Mul("room.pricePerNight", discountMultiplier);
            await _context.Reservations.UpdateManyAsync(filterReservations, updateReservations);

            return roomsResult.ModifiedCount;
        }

        public async Task<long> UpdateRoomsPriceForReservationsToApril2024Async(int priceDecrement)
        {
            var filterReservations = Builders<ReservationCollection>.Filter.Lt(r => r.CheckInDate, new DateTime(2023, 4, 1));
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

        public async Task<long> DeletePaymentsOlderThanMarch2024Async()
        {
            var filterReservations = Builders<ReservationCollection>.Filter.Lt(r => r.CheckInDate, new DateTime(2023, 3, 1));
            var update = Builders<ReservationCollection>.Update.Set(r => r.Payments, new List<PaymentEmbedded>());

            var result = await _context.Reservations.UpdateManyAsync(filterReservations, update);
            return result.ModifiedCount;
        }

        public async Task<long> DeletePaymentsToSumAsync(int sum)
        {
            var filter = Builders<ReservationCollection>.Filter.Empty;
            var update = Builders<ReservationCollection>.Update.PullFilter(r => r.Payments, p => p.Sum < sum);

            var result = await _context.Reservations.UpdateManyAsync(filter, update);
            return result.ModifiedCount;
        }

        public async Task<long> DeleteReservationsServicesOlderThanMarch2023Async()
        {
            var filterReservations = Builders<ReservationCollection>.Filter.Lt(r => r.CheckInDate, new DateTime(2023, 3, 1));
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

        public async Task<long> DeleteUnusedServicesPriceBelowAsync(int price)
        {
            var usedServiceIds = await _context.Reservations
                .Aggregate()
                .Unwind(r => r.Services)
                .Group(new BsonDocument { { "_id", "$services._id" } })
                .Project(new BsonDocument("_id", 1))
                .ToListAsync();

            var usedIds = usedServiceIds.Select(x => x["_id"].AsObjectId).ToList();

            var filter = Builders<Models.Collections.ServiceCollection>.Filter.And(
                Builders<Models.Collections.ServiceCollection>.Filter.Lt(s => s.Price, price),
                Builders<Models.Collections.ServiceCollection>.Filter.Nin(s => s.Id, usedIds)
            );

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