using DBSystemComparator_API.Models.Collections;
using MongoDB.Driver;

namespace DBSystemComparator_API.Database
{
    public static class MongoDbSeeder
    {
        public static async Task CreateCollectionsAndIndexesAsync(IMongoDatabase database)
        {
            // Collections
            var requiredCollections = new[] { "Clients", "Rooms", "Reservations", "Payments", "Services", "ReservationServices" };
            var existingCollections = database.ListCollectionNames().ToList();

            foreach (var collection in requiredCollections)
            {
                if (!existingCollections.Contains(collection))
                {
                    await database.CreateCollectionAsync(collection);
                }
            }

            // Indexes
            var reservations = database.GetCollection<ReservationCollection>("Reservations");
            await reservations.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<ReservationCollection>(Builders<ReservationCollection>.IndexKeys.Ascending(r => r.ClientId)),
                new CreateIndexModel<ReservationCollection>(Builders<ReservationCollection>.IndexKeys.Ascending(r => r.RoomId))
            });

            var payments = database.GetCollection<PaymentCollection>("Payments");
            await payments.Indexes.CreateOneAsync(
                new CreateIndexModel<PaymentCollection>(Builders<PaymentCollection>.IndexKeys.Ascending(p => p.ReservationId))
            );

            var reservationServices = database.GetCollection<ReservationServiceCollection>("ReservationServices");
            await reservationServices.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<ReservationServiceCollection>(Builders<ReservationServiceCollection>.IndexKeys.Ascending(rs => rs.ReservationId)),
                new CreateIndexModel<ReservationServiceCollection>(Builders<ReservationServiceCollection>.IndexKeys.Ascending(rs => rs.ServiceId))
            });
        }
    }
}