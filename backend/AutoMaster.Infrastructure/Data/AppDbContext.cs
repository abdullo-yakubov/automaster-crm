using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Mechanic> Mechanics => Set<Mechanic>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderTimelineEntry> OrderTimelineEntries => Set<OrderTimelineEntry>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<MysteryAudit> MysteryAudits => Set<MysteryAudit>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.HasIndex(x => x.Phone).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TotalSpent).HasPrecision(14, 2);
        });

        modelBuilder.Entity<Vehicle>(e =>
        {
            e.HasIndex(x => x.LicensePlate).IsUnique();
            e.HasOne(x => x.Customer).WithMany(x => x.Vehicles).HasForeignKey(x => x.CustomerId);
        });

        modelBuilder.Entity<Mechanic>(e =>
        {
            e.Property(x => x.Specialization).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Level).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Salary).HasPrecision(14, 2);
            e.Property(x => x.Rating).HasPrecision(4, 2);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasIndex(x => x.OrderNumber).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.LaborCost).HasPrecision(14, 2);
            e.Property(x => x.PartsCost).HasPrecision(14, 2);
            e.Property(x => x.TotalCost).HasPrecision(14, 2);
            e.HasOne(x => x.Customer).WithMany(x => x.Orders).HasForeignKey(x => x.CustomerId);
            e.HasOne(x => x.Vehicle).WithMany(x => x.Orders).HasForeignKey(x => x.VehicleId);
            e.HasOne(x => x.Mechanic).WithMany(x => x.Orders).HasForeignKey(x => x.MechanicId);
        });

        modelBuilder.Entity<OrderTimelineEntry>(e =>
        {
            e.HasOne(x => x.Order).WithMany(x => x.Timeline).HasForeignKey(x => x.OrderId);
        });

        modelBuilder.Entity<Part>(e =>
        {
            e.HasIndex(x => x.Sku);
            e.Property(x => x.Category).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.PurchasePrice).HasPrecision(14, 2);
            e.Property(x => x.SalePrice).HasPrecision(14, 2);
        });

        modelBuilder.Entity<Transaction>(e =>
        {
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Amount).HasPrecision(14, 2);
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<LoyaltyTransaction>(e =>
        {
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
        });

        modelBuilder.Entity<Branch>(e =>
        {
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<MysteryAudit>(e =>
        {
            e.Property(x => x.TotalScore).HasPrecision(4, 2);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<ChecklistItem>(e =>
        {
            e.HasIndex(x => x.Type);
        });
    }
}
