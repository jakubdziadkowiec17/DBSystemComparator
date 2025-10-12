using Microsoft.EntityFrameworkCore;
using DBSystemComparator_API.Models.Entities;

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

    public class SqlServerDbContext : ApplicationDbContext
    {
        public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : base(options) { }
    }

    public class PostgresDbContext : ApplicationDbContext
    {
        public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options) { }
    }
}