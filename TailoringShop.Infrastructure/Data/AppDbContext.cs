using Microsoft.EntityFrameworkCore;
using TailoringShop.Core.Entities;
using TailoringShop.Core.Enums;

namespace TailoringShop.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Users & Permissions
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    // Finance
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Cashbox> Cashboxes => Set<Cashbox>();
    public DbSet<CashboxTransaction> CashboxTransactions => Set<CashboxTransaction>();

    // Customers
    public DbSet<Customer> Customers => Set<Customer>();

    // Products & Inventory
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();

    // Invoices
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<InvoicePayment> InvoicePayments => Set<InvoicePayment>();

    // Tailoring
    public DbSet<TailoringOrder> TailoringOrders => Set<TailoringOrder>();
    public DbSet<TailoringOrderItem> TailoringOrderItems => Set<TailoringOrderItem>();
    public DbSet<TailoringOrderStatusHistory> TailoringOrderStatusHistory => Set<TailoringOrderStatusHistory>();
    public DbSet<TailoringOrderPayment> TailoringOrderPayments => Set<TailoringOrderPayment>();

    // Expenses
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();

    // System
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Backup> Backups => Set<Backup>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.UUID).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasOne(e => e.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // Role Configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Permission Configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasIndex(e => new { e.Module, e.Name }).IsUnique();
        });

        // RolePermission Configuration
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId });
            entity.HasOne(e => e.Role)
                  .WithMany(r => r.RolePermissions)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(e => e.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // UserRole Configuration
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.AssignedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.AssignedBy)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // Currency Configuration
        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.ExchangeRate).HasPrecision(15, 6);
        });

        // Cashbox Configuration
        modelBuilder.Entity<Cashbox>(entity =>
        {
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.CurrentBalance).HasPrecision(15, 3);
            
            entity.HasOne(e => e.Currency)
                  .WithMany(c => c.Cashboxes)
                  .HasForeignKey(e => e.CurrencyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // CashboxTransaction Configuration
        modelBuilder.Entity<CashboxTransaction>(entity =>
        {
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Amount).HasPrecision(15, 3);
            entity.Property(e => e.BalanceBefore).HasPrecision(15, 3);
            entity.Property(e => e.BalanceAfter).HasPrecision(15, 3);
            
            entity.HasOne(e => e.Cashbox)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(e => e.CashboxId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Customer Configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(e => e.UUID).IsUnique();
            entity.Property(e => e.TotalPurchases).HasPrecision(15, 3);
        });

        // Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.UUID).IsUnique();
            entity.HasIndex(e => e.SKU).IsUnique();
            entity.HasIndex(e => e.Barcode).IsUnique();
            entity.Property(e => e.CostPrice).HasPrecision(15, 3);
            entity.Property(e => e.SellingPrice).HasPrecision(15, 3);
            
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.Currency)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CurrencyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Inventory Configuration
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasIndex(e => new { e.ProductId, e.StoreId }).IsUnique();
            entity.Property(e => e.Quantity).HasPrecision(15, 3);
            entity.Property(e => e.ReservedQuantity).HasPrecision(15, 3);
            entity.Ignore(e => e.AvailableQuantity);
            
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.Inventories)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Store)
                  .WithMany(s => s.Inventories)
                  .HasForeignKey(e => e.StoreId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // InventoryMovement Configuration
        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Quantity).HasPrecision(15, 3);
            entity.Property(e => e.QuantityBefore).HasPrecision(15, 3);
            entity.Property(e => e.QuantityAfter).HasPrecision(15, 3);
            entity.Property(e => e.UnitCost).HasPrecision(15, 3);
            
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Store)
                  .WithMany()
                  .HasForeignKey(e => e.StoreId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Invoice Configuration
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(e => e.UUID).IsUnique();
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.DiscountType).HasConversion<string>();
            entity.Property(e => e.PaymentStatus).HasConversion<string>();
            entity.Property(e => e.Subtotal).HasPrecision(15, 3);
            entity.Property(e => e.DiscountValue).HasPrecision(15, 3);
            entity.Property(e => e.DiscountAmount).HasPrecision(15, 3);
            entity.Property(e => e.Total).HasPrecision(15, 3);
            entity.Property(e => e.PaidAmount).HasPrecision(15, 3);
            entity.Property(e => e.RemainingAmount).HasPrecision(15, 3);
            
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Invoices)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.Cashbox)
                  .WithMany()
                  .HasForeignKey(e => e.CashboxId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Store)
                  .WithMany()
                  .HasForeignKey(e => e.StoreId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Currency)
                  .WithMany()
                  .HasForeignKey(e => e.CurrencyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // InvoiceItem Configuration
        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.Property(e => e.DiscountType).HasConversion<string>();
            entity.Property(e => e.Quantity).HasPrecision(15, 3);
            entity.Property(e => e.UnitPrice).HasPrecision(15, 3);
            entity.Property(e => e.CostPrice).HasPrecision(15, 3);
            entity.Property(e => e.DiscountValue).HasPrecision(15, 3);
            entity.Property(e => e.DiscountAmount).HasPrecision(15, 3);
            entity.Property(e => e.Subtotal).HasPrecision(15, 3);
            entity.Property(e => e.Total).HasPrecision(15, 3);
            entity.Property(e => e.Profit).HasPrecision(15, 3);
            
            entity.HasOne(e => e.Invoice)
                  .WithMany(i => i.Items)
                  .HasForeignKey(e => e.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.InvoiceItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // InvoicePayment Configuration
        modelBuilder.Entity<InvoicePayment>(entity =>
        {
            entity.Property(e => e.PaymentMethod).HasConversion<string>();
            entity.Property(e => e.Amount).HasPrecision(15, 3);
            
            entity.HasOne(e => e.Invoice)
                  .WithMany(i => i.Payments)
                  .HasForeignKey(e => e.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Cashbox)
                  .WithMany()
                  .HasForeignKey(e => e.CashboxId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // TailoringOrder Configuration
        modelBuilder.Entity<TailoringOrder>(entity =>
        {
            entity.HasIndex(e => e.UUID).IsUnique();
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.PaymentStatus).HasConversion<string>();
            entity.Property(e => e.TotalAmount).HasPrecision(15, 3);
            entity.Property(e => e.DepositAmount).HasPrecision(15, 3);
            entity.Property(e => e.RemainingAmount).HasPrecision(15, 3);
            
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.TailoringOrders)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Cashbox)
                  .WithMany()
                  .HasForeignKey(e => e.CashboxId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Currency)
                  .WithMany()
                  .HasForeignKey(e => e.CurrencyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // TailoringOrderItem Configuration
        modelBuilder.Entity<TailoringOrderItem>(entity =>
        {
            entity.Property(e => e.UnitPrice).HasPrecision(15, 3);
            entity.Property(e => e.Total).HasPrecision(15, 3);
            
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.Items)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TailoringOrderStatusHistory Configuration
        modelBuilder.Entity<TailoringOrderStatusHistory>(entity =>
        {
            entity.Property(e => e.FromStatus).HasConversion<string>();
            entity.Property(e => e.ToStatus).HasConversion<string>();
            
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.StatusHistory)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TailoringOrderPayment Configuration
        modelBuilder.Entity<TailoringOrderPayment>(entity =>
        {
            entity.Property(e => e.PaymentType).HasConversion<string>();
            entity.Property(e => e.PaymentMethod).HasConversion<string>();
            entity.Property(e => e.Amount).HasPrecision(15, 3);
            
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.Payments)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Cashbox)
                  .WithMany()
                  .HasForeignKey(e => e.CashboxId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Expense Configuration
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasIndex(e => e.UUID).IsUnique();
            entity.HasIndex(e => e.ExpenseNumber).IsUnique();
            entity.Property(e => e.Amount).HasPrecision(15, 3);
            
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Expenses)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Cashbox)
                  .WithMany()
                  .HasForeignKey(e => e.CashboxId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Currency)
                  .WithMany()
                  .HasForeignKey(e => e.CurrencyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Setting Configuration
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasIndex(e => new { e.Group, e.Key }).IsUnique();
        });

        // Backup Configuration
        modelBuilder.Entity<Backup>(entity =>
        {
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            entity.UpdatedAt = DateTime.Now;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.Now;
            }
        }
    }
}
