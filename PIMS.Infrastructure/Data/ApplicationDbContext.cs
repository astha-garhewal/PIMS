using Microsoft.EntityFrameworkCore;
using PIMS.Domain.Entities;

namespace PIMS.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<InventoryAudit> InventoryAudits { get; set; }
        public DbSet<LowInventoryAlert> LowInventoryAlerts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          base.OnModelCreating(modelBuilder);

          modelBuilder.Entity<Product>(entity =>
          {
            entity.ToTable("Products");

            entity.HasKey(p => p.ProductID);

            entity.HasIndex(p => p.SKU)
                .IsUnique();

            entity.Property(p => p.SKU)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(p => p.Description)
                .HasMaxLength(1000);

            entity.Property(p => p.Price)
                .HasPrecision(18, 2);

            entity.Property(p => p.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
          });

          modelBuilder.Entity<Category>(entity =>
          {
            entity.ToTable("Categories");

            entity.HasKey(c => c.CategoryID);

            entity.Property(c => c.CategoryName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(c => c.Description)
                .HasMaxLength(500);
          });

          modelBuilder.Entity<ProductCategory>(entity =>
          {
            entity.ToTable("ProductCategories");

            entity.HasKey(pc => new
            {
                pc.ProductID,
                pc.CategoryID
            });

            entity.HasOne(pc => pc.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(pc => pc.ProductID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pc => pc.Category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(pc => pc.CategoryID)
                .OnDelete(DeleteBehavior.Cascade);
          });

          modelBuilder.Entity<Inventory>(entity =>
          {
            entity.ToTable("Inventory");

            entity.HasKey(i => i.InventoryID);

            entity.Property(i => i.WarehouseLocation)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductID)
                .OnDelete(DeleteBehavior.Restrict);
          });

          modelBuilder.Entity<InventoryTransaction>(entity =>
          {
            entity.ToTable("InventoryTransactions");

            entity.HasKey(t => t.TransactionID);

            entity.Property(t => t.TransactionType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(t => t.Reason)
                .HasMaxLength(500);

            entity.HasOne(t => t.Inventory)
                .WithMany(i => i.Transactions)
                .HasForeignKey(t => t.InventoryID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.User)
                .WithMany(u => u.InventoryTransactions)
                .HasForeignKey(t => t.UserID)
                .OnDelete(DeleteBehavior.Restrict);
          });

          modelBuilder.Entity<InventoryAudit>(entity =>
          {
            entity.ToTable("InventoryAudits");

            entity.HasKey(a => a.AuditID);

            entity.Property(a => a.Reason)
                .HasMaxLength(500)
                .IsRequired();

            entity.HasOne(a => a.Inventory)
                .WithMany(i => i.Audits)
                .HasForeignKey(a => a.InventoryID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.User)
                .WithMany(u => u.InventoryAudits)
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.Restrict);
          });

          modelBuilder.Entity<LowInventoryAlert>(entity =>
          {
            entity.ToTable("LowInventoryAlerts");

            entity.HasKey(a => a.AlertID);

            entity.HasOne(a => a.Inventory)
                .WithMany(i => i.LowInventoryAlerts)
                .HasForeignKey(a => a.InventoryID)
                .OnDelete(DeleteBehavior.Restrict);
          });

          modelBuilder.Entity<User>(entity =>
          {
            entity.ToTable("Users");

            entity.HasKey(u => u.UserID);

            entity.HasIndex(u => u.Username)
                .IsUnique();

            entity.Property(u => u.Username)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(u => u.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
          });

          modelBuilder.Entity<Role>(entity =>
          {
            entity.ToTable("Roles");

            entity.HasKey(r => r.RoleID);

            entity.Property(r => r.RoleName)
                .HasMaxLength(50)
                .IsRequired();
          });

          modelBuilder.Entity<UserRole>(entity =>
          {
            entity.ToTable("UserRoles");

            entity.HasKey(ur => new
            {
                ur.UserID,
                ur.RoleID
            });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleID)
                .OnDelete(DeleteBehavior.Cascade);
          });
        }
}