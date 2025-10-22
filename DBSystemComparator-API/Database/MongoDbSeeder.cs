using DBSystemComparator_API.Models.Collections;
using MongoDB.Driver;

namespace DBSystemComparator_API.Database
{
    public static class MongoDbSeeder
    {
        public static async Task CreateCollectionsAndIndexesAsync(MongoDbContext context)
        {
            await context.Clients.Indexes.CreateOneAsync(
                new CreateIndexModel<ClientCollection>(
                    Builders<ClientCollection>.IndexKeys.Ascending(c => c.Email)
                )
            );

            await context.Clients.Indexes.CreateOneAsync(
                new CreateIndexModel<ClientCollection>(
                    Builders<ClientCollection>.IndexKeys.Ascending(c => c.IsActive)
                )
            );

            await context.Rooms.Indexes.CreateOneAsync(
                new CreateIndexModel<RoomCollection>(
                    Builders<RoomCollection>.IndexKeys.Descending(r => r.Capacity)
                )
            );

            await context.Rooms.Indexes.CreateOneAsync(
                new CreateIndexModel<RoomCollection>(
                    Builders<RoomCollection>.IndexKeys.Ascending(r => r.IsActive)
                )
            );
            await context.Services.Indexes.CreateOneAsync(
                new CreateIndexModel<Models.Collections.ServiceCollection>(
                    Builders<Models.Collections.ServiceCollection>.IndexKeys.Ascending(s => s.IsActive)
                )
            );

            await context.Services.Indexes.CreateOneAsync(
                new CreateIndexModel<Models.Collections.ServiceCollection>(
                    Builders<Models.Collections.ServiceCollection>.IndexKeys.Ascending(s => s.Price)
                )
            );

            await context.Reservations.Indexes.CreateManyAsync(new[]
            {
        new CreateIndexModel<ReservationCollection>(
            Builders<ReservationCollection>.IndexKeys.Descending(r => r.CheckInDate)
        ),

        new CreateIndexModel<ReservationCollection>(
            Builders<ReservationCollection>.IndexKeys.Ascending(r => r.CheckOutDate)
        ),

        new CreateIndexModel<ReservationCollection>(
            Builders<ReservationCollection>.IndexKeys.Ascending("client._id")
        ),

        new CreateIndexModel<ReservationCollection>(
            Builders<ReservationCollection>.IndexKeys.Ascending("room._id")
        ),

        new CreateIndexModel<ReservationCollection>(
            Builders<ReservationCollection>.IndexKeys.Ascending("room.capacity")
        ),

        new CreateIndexModel<ReservationCollection>(
            Builders<ReservationCollection>.IndexKeys.Ascending("room.isActive")
        ),

        new CreateIndexModel<ReservationCollection>(
            Builders<ReservationCollection>.IndexKeys.Ascending("payments.sum")
        ),

        new CreateIndexModel<ReservationCollection>(
            Builders<ReservationCollection>.IndexKeys.Ascending("services.isActive")
        ),

        new CreateIndexModel<ReservationCollection>(
            Builders<ReservationCollection>.IndexKeys
                .Ascending(r => r.CheckInDate)
                .Ascending("room.capacity")
        )
    });
        }
    }
}