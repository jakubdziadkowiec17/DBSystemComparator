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
        private readonly MongoDbContext _mongoDbContext;

        public MongoDBRepository(MongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
        }

        public async Task<TablesCountDTO> GetTablesCountAsync()
        {
            var clientsCount = await _mongoDbContext.Clients.CountDocumentsAsync(FilterDefinition<ClientCollection>.Empty);
            var roomsCount = await _mongoDbContext.Rooms.CountDocumentsAsync(FilterDefinition<RoomCollection>.Empty);
            var reservationsCount = await _mongoDbContext.Reservations.CountDocumentsAsync(FilterDefinition<ReservationCollection>.Empty);
            var paymentsCount = await _mongoDbContext.Payments.CountDocumentsAsync(FilterDefinition<PaymentCollection>.Empty);
            var servicesCount = await _mongoDbContext.Services.CountDocumentsAsync(FilterDefinition<Models.Collections.ServiceCollection>.Empty);
            var reservationsServicesCount = await _mongoDbContext.ReservationsServices.CountDocumentsAsync(FilterDefinition<ReservationServiceCollection>.Empty);

            return new TablesCountDTO
            {
                ClientsCount = (int)clientsCount,
                RoomsCount = (int)roomsCount,
                ReservationsCount = (int)reservationsCount,
                PaymentsCount = (int)paymentsCount,
                ServicesCount = (int)servicesCount,
                ReservationsServicesCount = (int)reservationsServicesCount
            };
        }

        // CREATE
        public async Task<string> CreateClientAsync(string firstName, string secondName, string lastName, string email, DateTime dob, string address, string phone, bool isActive)
        {
            var client = new ClientCollection
            {
                Name = firstName,
                SecondName = secondName,
                LastName = lastName,
                Email = email,
                DateOfBirth = dob,
                Address = address,
                PhoneNumber = phone,
                IsActive = isActive
            };

            await _mongoDbContext.Clients.InsertOneAsync(client);
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

            await _mongoDbContext.Rooms.InsertOneAsync(room);
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

            await _mongoDbContext.Services.InsertOneAsync(service);
            return service.Id.ToString();
        }

        public async Task<string> CreateReservationAsync(string clientId, string roomId, DateTime checkIn, DateTime checkOut, DateTime creationDate)
        {
            var reservation = new ReservationCollection
            {
                ClientId = clientId,
                RoomId = roomId,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                CreationDate = creationDate
            };

            await _mongoDbContext.Reservations.InsertOneAsync(reservation);
            return reservation.Id.ToString();
        }

        public async Task CreateReservationServiceAsync(string reservationId, string serviceId, DateTime creationDate)
        {
            var rs = new ReservationServiceCollection
            {
                ReservationId = reservationId,
                ServiceId = serviceId,
                CreationDate = creationDate
            };

            await _mongoDbContext.ReservationsServices.InsertOneAsync(rs);
        }

        public async Task<string> CreatePaymentAsync(string reservationId, string description, int sum, DateTime creationDate)
        {
            var payment = new PaymentCollection
            {
                ReservationId = reservationId,
                Description = description,
                Sum = sum,
                CreationDate = creationDate
            };

            await _mongoDbContext.Payments.InsertOneAsync(payment);
            return payment.Id.ToString();
        }

        // READ
        public async Task<List<Dictionary<string, object>>> ReadClientsWithRoomsAsync(bool isActive)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("IsActive", isActive)),

                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Reservations" },
                    { "localField", "_id" },
                    { "foreignField", "ClientId" },
                    { "as", "reservations" }
                }),
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$reservations" },
                    { "preserveNullAndEmptyArrays", true }
                }),

                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Rooms" },
                    { "localField", "reservations.RoomId" },
                    { "foreignField", "_id" },
                    { "as", "room" }
                }),
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$room" },
                    { "preserveNullAndEmptyArrays", true }
                }),

                new BsonDocument("$match", new BsonDocument("room.IsActive", isActive)),

                new BsonDocument("$project", new BsonDocument
                {
                    { "ClientId", "$_id" },
                    { "FirstName", "$FirstName" },
                    { "LastName", "$LastName" },
                    { "RoomNumber", "$room.Number" },
                    { "PricePerNight", "$room.PricePerNight" }
                })
            };

            var result = await _mongoDbContext.Clients.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.Select(doc => doc.ToDictionary()).ToList();
        }

        public async Task<List<Dictionary<string, object>>> ReadRoomsWithReservationCountAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Reservations" },
                    { "localField", "_id" },
                    { "foreignField", "RoomId" },
                    { "as", "reservations" }
                }),
                new BsonDocument("$addFields", new BsonDocument("ReservationCount", new BsonDocument("$size", "$reservations"))),
                new BsonDocument("$match", new BsonDocument("ReservationCount", new BsonDocument("$gt", 0))),
                new BsonDocument("$project", new BsonDocument
                {
                    { "RoomId", "$_id" },
                    { "Number", "$Number" },
                    { "Capacity", "$Capacity" },
                    { "ReservationCount", "$ReservationCount" }
                })
            };

            var result = await _mongoDbContext.Rooms.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.Select(doc => doc.ToDictionary()).ToList();
        }

        public async Task<List<Dictionary<string, object>>> ReadServicesUsageAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "ReservationsServices" },
                    { "localField", "_id" },
                    { "foreignField", "ServiceId" },
                    { "as", "reservationsServices" }
                }),
                new BsonDocument("$addFields", new BsonDocument("UsageCount", new BsonDocument("$size", "$reservationsServices"))),
                new BsonDocument("$sort", new BsonDocument("UsageCount", -1)),
                new BsonDocument("$project", new BsonDocument
                {
                    { "ServiceName", "$Name" },
                    { "Price", "$Price" },
                    { "UsageCount", "$UsageCount" }
                })
            };

            var result = await _mongoDbContext.Services.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.Select(doc => doc.ToDictionary()).ToList();
        }

        public async Task<List<Dictionary<string, object>>> ReadPaymentsAboveAsync(int minSum)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("Sum", new BsonDocument("$gt", minSum))),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Reservations" },
                    { "localField", "ReservationId" },
                    { "foreignField", "_id" },
                    { "as", "reservation" }
                }),
                new BsonDocument("$unwind", new BsonDocument("path", "$reservation")),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Clients" },
                    { "localField", "reservation.ClientId" },
                    { "foreignField", "_id" },
                    { "as", "client" }
                }),
                new BsonDocument("$unwind", new BsonDocument("path", "$client")),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Rooms" },
                    { "localField", "reservation.RoomId" },
                    { "foreignField", "_id" },
                    { "as", "room" }
                }),
                new BsonDocument("$unwind", new BsonDocument("path", "$room")),
                new BsonDocument("$project", new BsonDocument
                {
                    { "PaymentId", "$_id" },
                    { "Sum", "$Sum" },
                    { "CreationDate", "$CreationDate" },
                    { "ClientName", "$client.FirstName" },
                    { "RoomNumber", "$room.Number" }
                })
            };

            var result = await _mongoDbContext.Payments.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.Select(doc => doc.ToDictionary()).ToList();
        }

        public async Task<List<Dictionary<string, object>>> ReadReservationsWithServicesAsync(bool clientActive, bool serviceActive)
        {
            var pipeline = new[]
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Clients" },
                    { "localField", "ClientId" },
                    { "foreignField", "_id" },
                    { "as", "client" }
                }),
                new BsonDocument("$unwind", "$client"),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "ReservationsServices" },
                    { "localField", "_id" },
                    { "foreignField", "ReservationId" },
                    { "as", "reservationsServices" }
                }),
                new BsonDocument("$unwind", "$reservationsServices"),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "Services" },
                    { "localField", "reservationsServices.ServiceId" },
                    { "foreignField", "_id" },
                    { "as", "service" }
                }),
                new BsonDocument("$unwind", "$service"),
                new BsonDocument("$match", new BsonDocument
                {
                    { "client.IsActive", clientActive },
                    { "service.IsActive", serviceActive }
                }),
                new BsonDocument("$project", new BsonDocument
                {
                    { "ReservationId", "$_id" },
                    { "LastName", "$client.LastName" },
                    { "ServiceName", "$service.Name" },
                    { "Price", "$service.Price" },
                    { "CheckInDate", "$CheckInDate" },
                    { "CheckOutDate", "$CheckOutDate" }
                })
            };

            var result = await _mongoDbContext.Reservations.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.Select(doc => doc.ToDictionary()).ToList();
        }

        // UPDATE
        public async Task<long> UpdateClientsAddressPhoneAsync(bool isActive)
        {
            var filter = Builders<ClientCollection>.Filter.Eq(c => c.IsActive, isActive);
            var update = Builders<ClientCollection>.Update
                .Set(c => c.Address, "Cracow, ul. abc 4")
                .Set(c => c.PhoneNumber, "123456789");

            var result = await _mongoDbContext.Clients.UpdateManyAsync(filter, update);
            return result.ModifiedCount;
        }

        public async Task<long> UpdateRoomsPriceJoinReservationsAsync(int minCapacity)
        {
            var reservedRoomIdsCursor = await _mongoDbContext.Reservations
                .DistinctAsync<string>("RoomId", FilterDefinition<ReservationCollection>.Empty);
            var reservedRoomIds = await reservedRoomIdsCursor.ToListAsync();

            var filter = Builders<RoomCollection>.Filter.In(r => r.Id, reservedRoomIds) &
                         Builders<RoomCollection>.Filter.Gte(r => r.Capacity, minCapacity);
            var update = Builders<RoomCollection>.Update.Inc(r => r.PricePerNight, 150);
            var result = await _mongoDbContext.Rooms.UpdateManyAsync(filter, update);
            return result.ModifiedCount;
        }

        public async Task<long> UpdateServicesPriceAsync(bool isActive)
        {
            var filter = Builders<Models.Collections.ServiceCollection>.Filter.Eq(s => s.IsActive, isActive);
            var update = Builders<Models.Collections.ServiceCollection>.Update.Inc(s => s.Price, 25);
            var result = await _mongoDbContext.Services.UpdateManyAsync(filter, update);
            return result.ModifiedCount;
        }

        public async Task<long> UpdateRoomsPriceInactiveAsync()
        {
            var filter = Builders<RoomCollection>.Filter.Eq(r => r.IsActive, false);
            var update = Builders<RoomCollection>.Update.Mul(r => r.PricePerNight, 0.8);
            var result = await _mongoDbContext.Rooms.UpdateManyAsync(filter, update);
            return result.ModifiedCount;
        }

        public async Task<long> UpdateRoomsPriceFutureReservationsAsync()
        {
            var futureRoomIds = await _mongoDbContext.Reservations
                .Find(r => r.CheckInDate > DateTime.Now)
                .Project(r => r.RoomId)
                .ToListAsync();

            var filter = Builders<RoomCollection>.Filter.In(r => r.Id, futureRoomIds);
            var update = Builders<RoomCollection>.Update.Inc(r => r.PricePerNight, -15);

            var result = await _mongoDbContext.Rooms.UpdateManyAsync(filter, update);
            return result.ModifiedCount;
        }

        // DELETE
        public async Task<long> DeleteReservationsSmallRoomsAsync(int capacityThreshold)
        {
            var smallRoomIds = await _mongoDbContext.Rooms
                .Find(r => r.Capacity < capacityThreshold)
                .Project(r => r.Id)
                .ToListAsync();

            var filter = Builders<ReservationCollection>.Filter.In(r => r.RoomId, smallRoomIds) &
                         Builders<ReservationCollection>.Filter.Gt(r => r.CheckInDate, DateTime.Now);

            var result = await _mongoDbContext.Reservations.DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        public async Task<long> DeleteReservationsServicesFutureAsync(int topRows)
        {
            var futureReservations = await _mongoDbContext.Reservations
                .Find(r => r.CheckInDate > DateTime.Now)
                .Limit(topRows)
                .Project(r => r.Id)
                .ToListAsync();

            var filter = Builders<ReservationServiceCollection>.Filter.In(rs => rs.ReservationId, futureReservations);
            var result = await _mongoDbContext.ReservationsServices.DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        public async Task<long> DeleteReservationsWithoutPaymentsAsync()
        {
            var paidReservationIds = await _mongoDbContext.Payments
                .Find(FilterDefinition<PaymentCollection>.Empty)
                .Project(p => p.ReservationId)
                .ToListAsync();

            var filter = Builders<ReservationCollection>.Filter.Nin(r => r.Id, paidReservationIds);
            var result = await _mongoDbContext.Reservations.DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        public async Task<long> DeleteInactiveClientsWithoutReservationsAsync()
        {
            var cursor = await _mongoDbContext.Reservations
                .DistinctAsync<string>("ClientId", FilterDefinition<ReservationCollection>.Empty);

            var activeClientIds = await cursor.ToListAsync();

            var filter = Builders<ClientCollection>.Filter.Eq(c => c.IsActive, false) &
                         Builders<ClientCollection>.Filter.Nin(c => c.Id, activeClientIds);

            var result = await _mongoDbContext.Clients.DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        public async Task<long> DeleteRoomsWithoutReservationsAsync()
        {
            var cursor = await _mongoDbContext.Reservations
                .DistinctAsync<string>("RoomId", FilterDefinition<ReservationCollection>.Empty);

            var reservedRoomIds = await cursor.ToListAsync();

            var filter = Builders<RoomCollection>.Filter.Eq(r => r.IsActive, false) &
                         Builders<RoomCollection>.Filter.Nin(r => r.Id, reservedRoomIds);

            var result = await _mongoDbContext.Rooms.DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        public Task DeleteAllClientsAsync() => _mongoDbContext.Clients.DeleteManyAsync(FilterDefinition<ClientCollection>.Empty);
        public Task DeleteAllRoomsAsync() => _mongoDbContext.Rooms.DeleteManyAsync(FilterDefinition<RoomCollection>.Empty);
        public Task DeleteAllReservationsAsync() => _mongoDbContext.Reservations.DeleteManyAsync(FilterDefinition<ReservationCollection>.Empty);
        public Task DeleteAllReservationsServicesAsync() => _mongoDbContext.ReservationsServices.DeleteManyAsync(FilterDefinition<ReservationServiceCollection>.Empty);
        public Task DeleteAllPaymentsAsync() => _mongoDbContext.Payments.DeleteManyAsync(FilterDefinition<PaymentCollection>.Empty);
        public Task DeleteAllServicesAsync() => _mongoDbContext.Services.DeleteManyAsync(FilterDefinition<Models.Collections.ServiceCollection>.Empty);

    }
}