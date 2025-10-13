using DBSystemComparator_API.Models.Collections;
using DBSystemComparator_API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace DBSystemComparator_API.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ReservationService> ReservationServices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ReservationsServices - Many to many
            modelBuilder.Entity<ReservationService>()
                .HasKey(rs => new { rs.ReservationId, rs.ServiceId });

            // ReservationsServices for Reservation
            modelBuilder.Entity<ReservationService>()
                .HasOne(rs => rs.Reservation)
                .WithMany(r => r.ReservationServices)
                .HasForeignKey(rs => rs.ReservationId);

            // ReservationsServices for Service
            modelBuilder.Entity<ReservationService>()
                .HasOne(rs => rs.Service)
                .WithMany(s => s.ReservationServices)
                .HasForeignKey(rs => rs.ServiceId);

            // Client in Reservation
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Client)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.ClientId);

            // Room in Reservation
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Room)
                .WithMany(room => room.Reservations)
                .HasForeignKey(r => r.RoomId);

            // Payments for Reservation
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Reservation)
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.ReservationId);
        }
    }

    public class SQLServerDbContext : ApplicationDbContext
    {
        public SQLServerDbContext(DbContextOptions<SQLServerDbContext> options) : base(options) { }
    }

    public class PostgreSQLDbContext : ApplicationDbContext
    {
        public PostgreSQLDbContext(DbContextOptions<PostgreSQLDbContext> options) : base(options) { }
    }

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