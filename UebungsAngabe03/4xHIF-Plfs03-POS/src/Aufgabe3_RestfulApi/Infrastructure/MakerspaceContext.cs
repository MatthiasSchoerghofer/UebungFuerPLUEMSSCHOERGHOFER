using Aufgabe3_RestfulApi.Model;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe3_RestfulApi.Infrastructure;

public class MakerspaceContext(DbContextOptions<MakerspaceContext> options) : DbContext(options)
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<EquipmentTag> EquipmentTags => Set<EquipmentTag>();
    public DbSet<MaintenanceTicket> MaintenanceTickets => Set<MaintenanceTicket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>().OwnsOne(x => x.Address);

        modelBuilder.Entity<Device>()
            .HasIndex(x => x.InventoryNumber)
            .IsUnique();

        modelBuilder.Entity<Member>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Room>()
            .HasIndex(x => x.RoomNumber)
            .IsUnique();

        modelBuilder.Entity<Device>()
            .Property(x => x.ReplacementValue)
            .HasPrecision(10, 2);

        modelBuilder.Entity<MaintenanceTicket>()
            .Ignore(x => x.IsOpen);
    }
}
