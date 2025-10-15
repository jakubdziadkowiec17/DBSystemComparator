using DBSystemComparator_API.Database;
using DBSystemComparator_API.Models.Collections;
using DBSystemComparator_API.Models.DTOs;
using DBSystemComparator_API.Repositories.Interfaces;
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

        public async Task<List<int>> GetAllRoomIdsAsync()
        {
            var roomIdStrings = await _mongoDbContext.Reservations
                .Find(Builders<ReservationCollection>.Filter.Empty)
                .Project(r => r.RoomId)
                .ToListAsync();

            var roomIds = new List<int>();
            foreach (var roomIdStr in roomIdStrings)
            {
                if (int.TryParse(roomIdStr, out int roomId))
                    roomIds.Add(roomId);
            }

            return roomIds;
        }
    }
}