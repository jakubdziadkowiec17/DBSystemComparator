using DBSystemComparator_API.Models.Collections;
using MongoDB.Driver;

namespace DBSystemComparator_API.Database
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _mongoDatabase = client.GetDatabase(databaseName);
        }

        public IMongoDatabase Database => _mongoDatabase;

        public IMongoCollection<ClientCollection> Clients => _mongoDatabase.GetCollection<ClientCollection>("Clients");
        public IMongoCollection<RoomCollection> Rooms => _mongoDatabase.GetCollection<RoomCollection>("Rooms");
        public IMongoCollection<Models.Collections.ServiceCollection> Services => _mongoDatabase.GetCollection<Models.Collections.ServiceCollection>("Services");
        public IMongoCollection<ReservationCollection> Reservations => _mongoDatabase.GetCollection<ReservationCollection>("Reservations");
    }
}