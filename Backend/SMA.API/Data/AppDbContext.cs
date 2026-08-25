using Microsoft.EntityFrameworkCore;
using SMA.API.Entities;
namespace SMA.API.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // These DbSets represent your tables
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<StripeWebhookEvent> StripeWebhookEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Currency Precision (CRITICAL for financial data)
            modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Transaction>().Property(t => t.Amount).HasColumnType("decimal(10,2)");

            // 2. Unique Constraints
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Product>().HasIndex(p => p.Sku).IsUnique();
            modelBuilder.Entity<Transaction>().HasIndex(t => t.GatewayTransactionId).IsUnique();
            modelBuilder.Entity<Order>().HasIndex(o => o.StripeCheckoutSessionId).IsUnique();
            modelBuilder.Entity<Order>().HasIndex(o => o.StripePaymentIntentId).IsUnique();

            // 3. One-to-One: Product & Inventory
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Inventory)
                .WithOne(i => i.Product)
                .HasForeignKey<Inventory>(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. One-to-Many: Order & OrderItems
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 5. Restrict Deletion on Financial Records
            // If an order has a transaction, prevent the order from being deleted.
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Order)
                .WithMany(o => o.Transactions)
                .HasForeignKey(t => t.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshToken>().HasIndex(rt => rt.TokenHash).IsUnique(false);
        }
    }
}   