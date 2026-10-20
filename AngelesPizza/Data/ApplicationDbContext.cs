using AngelesPizza.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AngelesPizza.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductModifier> ProductModifiers { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<RestaurantTable> RestaurantTables { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<OrderDetailModifier> OrderDetailModifiers { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<ProductModifierProduct> ProductModifierProducts { get; set; }

        public DbSet<CashRegister> CashRegisters { get; set; }

        public DbSet<Bank> Banks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // RELACIONES

            // Category -> Products
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            // Customer -> Orders
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            // RestaurantTable -> Orders
            modelBuilder.Entity<Order>()
                .HasOne(o => o.RestaurantTable)
                .WithMany(t => t.Orders)
                .HasForeignKey(o => o.RestaurantTableId)
                .OnDelete(DeleteBehavior.NoAction);

            // Order -> OrderDetails
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            // Product -> OrderDetails
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // OrderDetail -> OrderDetailModifiers
            modelBuilder.Entity<OrderDetailModifier>()
                .HasOne(odm => odm.OrderDetail)
                .WithMany(od => od.OrderDetailModifiers)
                .HasForeignKey(odm => odm.OrderDetailId)
                .OnDelete(DeleteBehavior.NoAction);

            // ProductModifier -> OrderDetailModifiers
            modelBuilder.Entity<OrderDetailModifier>()
                .HasOne(odm => odm.ProductModifier)
                .WithMany(pm => pm.OrderDetailModifiers)
                .HasForeignKey(odm => odm.ProductModifierId)
                .OnDelete(DeleteBehavior.NoAction);

            // Order -> Payments
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            // Order -> StatusHistory
            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(sh => sh.Order)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(sh => sh.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.CashRegister)
                .WithMany(cr => cr.Payments)
                .HasForeignKey(p => p.CashRegisterId)
                .OnDelete(DeleteBehavior.NoAction);

            // Bank -> Payments
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Bank)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BankId)
                .OnDelete(DeleteBehavior.NoAction);

            // ÍNDICES

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name);

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Phone);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CreatedAt);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Status);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderType);

            modelBuilder.Entity<Bank>()
                .HasIndex(b => b.Name);

            modelBuilder.Entity<Bank>().HasData(
                new Bank { Id = 1, Name = "Bancolombia", SwiftCode = "COLCCO22", IsActive = true, CreatedAt = new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc) },
                new Bank { Id = 2, Name = "Banco de Bogotá", SwiftCode = "BOGOCO22", IsActive = true, CreatedAt = new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc) },
                new Bank { Id = 3, Name = "Banco Santander", SwiftCode = "BSANESCO", IsActive = true, CreatedAt = new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc) },
                new Bank { Id = 4, Name = "BBVA", SwiftCode = "BBVAESCO", IsActive = true, CreatedAt = new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc) },
                new Bank { Id = 5, Name = "Davivienda", SwiftCode = "DAVICO22", IsActive = true, CreatedAt = new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc) },
                new Bank { Id = 6, Name = "Banco Caja Social", SwiftCode = "CAJACO22", IsActive = true, CreatedAt = new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc) },
                new Bank { Id = 7, Name = "Banco Occidente", SwiftCode = "OCCOCO22", IsActive = true, CreatedAt = new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc) },
                new Bank { Id = 8, Name = "Banco Popular", SwiftCode = "BOPOCO22", IsActive = true, CreatedAt = new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc) },
                new Bank { Id = 9, Name = "Banco Falabella", SwiftCode = "FALACO22", IsActive = true, CreatedAt = new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc) },
                new Bank { Id = 10, Name = "Scotiabank", SwiftCode = "SCBLESCO", IsActive = true, CreatedAt = new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }

}