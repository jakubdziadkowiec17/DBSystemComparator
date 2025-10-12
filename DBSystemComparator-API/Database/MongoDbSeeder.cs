using MongoDB.Driver;

namespace DBSystemComparator_API.Database
{
    public static class MongoDbSeeder
    {
        public static void CreateCollections(IMongoDatabase database)
        {
            var existingCollections = database.ListCollectionNames().ToList();
            var requiredCollections = new[] { "Clients", "Rooms", "Reservations", "Payments", "Services", "ReservationServices" };

            foreach (var collection in requiredCollections)
            {
                if (!existingCollections.Contains(collection))
                {
                    database.CreateCollection(collection);
                }
            }
        }
    }
}