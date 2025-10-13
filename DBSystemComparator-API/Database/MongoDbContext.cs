using DBSystemComparator_API.Models.Collections;
using Microsoft.EntityFrameworkCore;
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
        public IMongoCollection<ReservationCollection> Reservations => _mongoDatabase.GetCollection<ReservationCollection>("Reservations");
        public IMongoCollection<PaymentCollection> Payments => _mongoDatabase.GetCollection<PaymentCollection>("Payments");
        public IMongoCollection<Models.Collections.ServiceCollection> Services => _mongoDatabase.GetCollection<Models.Collections.ServiceCollection>("Services");
        public IMongoCollection<ReservationServiceCollection> ReservationServices => _mongoDatabase.GetCollection<ReservationServiceCollection>("ReservationServices");
    }
}