using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TailoringShop.Core.Entities;
using TailoringShop.Infrastructure.Data;

namespace TailoringShop.WPF.Services;

/// <summary>
/// نموذج بيانات النسخة الاحتياطية
/// </summary>
public class BackupData
{
    public BackupInfo Info { get; set; } = new();
    public BackupTables Data { get; set; } = new();
}

public class BackupInfo
{
    public string Version { get; set; } = "1.0";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string SystemVersion { get; set; } = "47";
    public int SchemaVersion { get; set; } = 1; // إصدار بنية قاعدة البيانات
    public string? CreatedBy { get; set; }
    public string? MachineName { get; set; }
}

public class BackupTables
{
    // البيانات الأساسية
    public List<Currency> Currencies { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<ExpenseCategory> ExpenseCategories { get; set; } = new();
    public List<Store> Stores { get; set; } = new();
    
    // الصناديق
    public List<CashboxBackup> Cashboxes { get; set; } = new();
    public List<CashboxTransactionBackup> CashboxTransactions { get; set; } = new();
    
    // العملاء
    public List<Customer> Customers { get; set; } = new();
    
    // المنتجات والمخزون
    public List<ProductBackup> Products { get; set; } = new();
    public List<InventoryBackup> Inventories { get; set; } = new();
    
    // طلبات الفصال
    public List<TailoringOrderBackup> TailoringOrders { get; set; } = new();
    public List<TailoringOrderItemBackup> TailoringOrderItems { get; set; } = new();
    public List<TailoringOrderPaymentBackup> TailoringOrderPayments { get; set; } = new();
    
    // الفواتير
    public List<InvoiceBackup> Invoices { get; set; } = new();
    public List<InvoiceItemBackup> InvoiceItems { get; set; } = new();
    
    // المصروفات
    public List<ExpenseBackup> Expenses { get; set; } = new();
    
    // المستخدمين والأدوار
    public List<RoleBackup> Roles { get; set; } = new();
    public List<PermissionBackup> Permissions { get; set; } = new();
    public List<UserBackup> Users { get; set; } = new();
    public List<RolePermissionBackup> RolePermissions { get; set; } = new();
    public List<UserRoleBackup> UserRoles { get; set; } = new();
    
    // إعدادات النظام
    public List<SystemSettings> SystemSettings { get; set; } = new();
}

#region Backup Models (بدون Navigation Properties)

public class CashboxBackup
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Core.Enums.CashboxType Type { get; set; }
    public long CurrencyId { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CashboxTransactionBackup
{
    public long Id { get; set; }
    public long CashboxId { get; set; }
    public Core.Enums.TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductBackup
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public long? CategoryId { get; set; }
    public long? CashboxId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public long CurrencyId { get; set; }
    public string Unit { get; set; } = "piece";
    public int MinStockLevel { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InventoryBackup
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public long StoreId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TailoringOrderBackup
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public long CashboxId { get; set; }
    public long CurrencyId { get; set; }
    public Core.Enums.OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public Core.Enums.PaymentStatus PaymentStatus { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime DeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TailoringOrderItemBackup
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TailoringOrderPaymentBackup
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long CashboxId { get; set; }
    public decimal Amount { get; set; }
    public Core.Enums.PaymentType PaymentType { get; set; }
    public Core.Enums.PaymentMethod PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InvoiceBackup
{
    public long Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Core.Enums.InvoiceType Type { get; set; }
    public Core.Enums.InvoiceStatus Status { get; set; }
    public long? CustomerId { get; set; }
    public string? SupplierName { get; set; }
    public long CashboxId { get; set; }
    public long StoreId { get; set; }
    public long CurrencyId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public Core.Enums.PaymentStatus PaymentStatus { get; set; }
    public string? Notes { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InvoiceItemBackup
{
    public long Id { get; set; }
    public long InvoiceId { get; set; }
    public long ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ExpenseBackup
{
    public long Id { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public long CashboxId { get; set; }
    public long CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public DateTime ExpenseDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RoleBackup
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PermissionBackup
{
    public long Id { get; set; }
    public string Module { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserBackup
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RolePermissionBackup
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserRoleBackup
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public DateTime AssignedAt { get; set; }
    public long? AssignedBy { get; set; }
}

#endregion

/// <summary>
/// خدمة النسخ الاحتياطي والاستعادة
/// </summary>
public class BackupService
{
    private readonly Func<AppDbContext> _contextFactory;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public BackupService(Func<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// إنشاء نسخة احتياطية
    /// </summary>
    public async Task<string> CreateBackupAsync(string filePath)
    {
        using var context = _contextFactory();
        
        // الحصول على إصدار Schema الحالي
        var schemaVersion = await GetCurrentSchemaVersionAsync(context);
        
        var backup = new BackupData
        {
            Info = new BackupInfo
            {
                Version = "1.0",
                CreatedAt = DateTime.Now,
                SystemVersion = "47",
                SchemaVersion = schemaVersion,
                CreatedBy = App.CurrentUser?.Username,
                MachineName = Environment.MachineName
            },
            Data = new BackupTables()
        };

        // جمع البيانات
        await CollectDataAsync(context, backup.Data);

        // حفظ الملف
        var json = JsonSerializer.Serialize(backup, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);

        return filePath;
    }
    
    /// <summary>
    /// الحصول على إصدار Schema الحالي من قاعدة البيانات
    /// </summary>
    private async Task<int> GetCurrentSchemaVersionAsync(AppDbContext context)
    {
        try
        {
            var setting = await context.Settings
                .FirstOrDefaultAsync(s => s.Group == "system" && s.Key == "schema_version");
            
            if (setting != null && int.TryParse(setting.Value, out int version))
            {
                return version;
            }
        }
        catch { }
        
        return 1; // الإصدار الافتراضي
    }

    #region SQL Server Full Backup (الأفضل)

    /// <summary>
    /// إنشاء نسخة احتياطية كاملة باستخدام SQL Server BACKUP
    /// هذه الطريقة أفضل بكثير من JSON لأنها تحتوي على Schema + Data + Indexes
    /// </summary>
    public async Task<SqlBackupResult> CreateSqlBackupAsync(string backupPath)
    {
        var result = new SqlBackupResult();
        
        try
        {
            using var context = _contextFactory();
            
            // الحصول على اسم قاعدة البيانات
            var databaseName = context.Database.GetDbConnection().Database;
            
            // التأكد من وجود المجلد
            var directory = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // تنفيذ أمر BACKUP
            var sql = $@"
                BACKUP DATABASE [{databaseName}] 
                TO DISK = '{backupPath}' 
                WITH FORMAT, 
                     COMPRESSION, 
                     NAME = 'TailoringShop Full Backup',
                     DESCRIPTION = 'Full backup created at {DateTime.Now:yyyy-MM-dd HH:mm:ss}'";

            await context.Database.ExecuteSqlRawAsync(sql);

            result.Success = true;
            result.FilePath = backupPath;
            result.FileSize = new FileInfo(backupPath).Length;
            result.Message = "تم إنشاء النسخة الاحتياطية الكاملة بنجاح";
            
            ErrorLogService.Instance.LogInfo($"SQL Backup created: {backupPath}");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            
            // إذا فشل SQL Backup (مثلاً LocalDB لا يدعم BACKUP)
            // نُعلم المستخدم بالسبب
            if (ex.Message.Contains("BACKUP") || ex.Message.Contains("permission"))
            {
                result.ErrorMessage = "SQL Server LocalDB لا يدعم BACKUP DATABASE. استخدم النسخ الاحتياطي JSON بدلاً منه، أو استخدم SQL Server Express/Standard.";
            }
            
            ErrorLogService.Instance.LogError(ex, "SQL Backup failed");
        }

        return result;
    }

    /// <summary>
    /// استعادة من نسخة احتياطية SQL Server
    /// تحذير: هذا يستبدل قاعدة البيانات بالكامل!
    /// </summary>
    public async Task<SqlBackupResult> RestoreSqlBackupAsync(string backupPath)
    {
        var result = new SqlBackupResult();
        
        try
        {
            if (!File.Exists(backupPath))
            {
                result.Success = false;
                result.ErrorMessage = "ملف النسخة الاحتياطية غير موجود";
                return result;
            }

            using var context = _contextFactory();
            var databaseName = context.Database.GetDbConnection().Database;

            // إغلاق جميع الاتصالات
            await context.Database.ExecuteSqlRawAsync($@"
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE");

            try
            {
                // استعادة قاعدة البيانات
                await context.Database.ExecuteSqlRawAsync($@"
                    RESTORE DATABASE [{databaseName}] 
                    FROM DISK = '{backupPath}' 
                    WITH REPLACE");

                result.Success = true;
                result.Message = "تمت استعادة النسخة الاحتياطية بنجاح";
                
                ErrorLogService.Instance.LogInfo($"SQL Restore completed: {backupPath}");
            }
            finally
            {
                // إعادة وضع قاعدة البيانات للوضع العادي
                try
                {
                    await context.Database.ExecuteSqlRawAsync($@"
                        ALTER DATABASE [{databaseName}] SET MULTI_USER");
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            
            if (ex.Message.Contains("RESTORE") || ex.Message.Contains("permission"))
            {
                result.ErrorMessage = "SQL Server LocalDB لا يدعم RESTORE DATABASE. استخدم الاستعادة من JSON بدلاً منها.";
            }
            
            ErrorLogService.Instance.LogError(ex, "SQL Restore failed");
        }

        return result;
    }

    /// <summary>
    /// التحقق من دعم SQL Server للـ BACKUP/RESTORE
    /// LocalDB لا يدعم هذه الميزة
    /// </summary>
    public Task<bool> IsSqlBackupSupportedAsync()
    {
        try
        {
            using var context = _contextFactory();
            var connectionString = context.Database.GetConnectionString();
            
            // LocalDB لا يدعم BACKUP DATABASE
            if (connectionString?.Contains("(localdb)", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Task.FromResult(false);
            }

            // محاولة بسيطة للتحقق
            // في الإنتاج يمكنك تنفيذ استعلام بسيط للتحقق من الصلاحيات
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    #endregion

    /// <summary>
    /// جمع البيانات من قاعدة البيانات
    /// </summary>
    private async Task CollectDataAsync(AppDbContext context, BackupTables data)
    {
        // البيانات الأساسية
        data.Currencies = await context.Currencies.AsNoTracking().ToListAsync();
        data.Categories = await context.Categories.AsNoTracking().ToListAsync();
        data.ExpenseCategories = await context.ExpenseCategories.AsNoTracking().ToListAsync();
        data.Stores = await context.Stores.AsNoTracking().ToListAsync();
        data.SystemSettings = await context.SystemSettings.AsNoTracking().ToListAsync();

        // الصناديق
        data.Cashboxes = await context.Cashboxes.AsNoTracking()
            .Select(c => new CashboxBackup
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                CurrencyId = c.CurrencyId,
                CurrentBalance = c.CurrentBalance,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            }).ToListAsync();

        data.CashboxTransactions = await context.CashboxTransactions.AsNoTracking()
            .Select(t => new CashboxTransactionBackup
            {
                Id = t.Id,
                CashboxId = t.CashboxId,
                Type = t.Type,
                Amount = t.Amount,
                BalanceBefore = t.BalanceBefore,
                BalanceAfter = t.BalanceAfter,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                CreatedBy = t.CreatedBy,
                CreatedAt = t.CreatedAt
            }).ToListAsync();

        // العملاء
        data.Customers = await context.Customers.AsNoTracking().ToListAsync();

        // المنتجات
        data.Products = await context.Products.AsNoTracking()
            .Select(p => new ProductBackup
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Barcode = p.Barcode,
                CategoryId = p.CategoryId,
                CashboxId = p.CashboxId,
                CostPrice = p.CostPrice,
                SellingPrice = p.SellingPrice,
                CurrencyId = p.CurrencyId,
                Unit = p.Unit,
                MinStockLevel = p.MinStockLevel,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            }).ToListAsync();

        // المخزون
        data.Inventories = await context.Inventories.AsNoTracking()
            .Select(i => new InventoryBackup
            {
                Id = i.Id,
                ProductId = i.ProductId,
                StoreId = i.StoreId,
                Quantity = i.Quantity,
                CreatedAt = i.CreatedAt
            }).ToListAsync();

        // طلبات الفصال
        data.TailoringOrders = await context.TailoringOrders.AsNoTracking()
            .Select(o => new TailoringOrderBackup
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                CashboxId = o.CashboxId,
                CurrencyId = o.CurrencyId,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                DepositAmount = o.DepositAmount,
                RemainingAmount = o.RemainingAmount,
                PaymentStatus = o.PaymentStatus,
                Description = o.Description,
                Notes = o.Notes,
                DeliveryDate = o.DeliveryDate,
                ActualDeliveryDate = o.ActualDeliveryDate,
                OrderDate = o.OrderDate,
                CreatedAt = o.CreatedAt
            }).ToListAsync();

        data.TailoringOrderItems = await context.TailoringOrderItems.AsNoTracking()
            .Select(i => new TailoringOrderItemBackup
            {
                Id = i.Id,
                OrderId = i.OrderId,
                ItemName = i.ItemName,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Total,
                CreatedAt = i.CreatedAt
            }).ToListAsync();

        data.TailoringOrderPayments = await context.TailoringOrderPayments.AsNoTracking()
            .Select(p => new TailoringOrderPaymentBackup
            {
                Id = p.Id,
                OrderId = p.OrderId,
                CashboxId = p.CashboxId,
                Amount = p.Amount,
                PaymentType = p.PaymentType,
                PaymentMethod = p.PaymentMethod,
                PaymentDate = p.PaymentDate,
                CreatedAt = p.CreatedAt
            }).ToListAsync();

        // الفواتير
        data.Invoices = await context.Invoices.AsNoTracking()
            .Select(i => new InvoiceBackup
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                Type = i.Type,
                Status = i.Status,
                CustomerId = i.CustomerId,
                SupplierName = i.SupplierName,
                CashboxId = i.CashboxId,
                StoreId = i.StoreId,
                CurrencyId = i.CurrencyId,
                Subtotal = i.Subtotal,
                DiscountAmount = i.DiscountAmount,
                Total = i.Total,
                PaidAmount = i.PaidAmount,
                RemainingAmount = i.RemainingAmount,
                PaymentStatus = i.PaymentStatus,
                Notes = i.Notes,
                InvoiceDate = i.InvoiceDate,
                CreatedAt = i.CreatedAt
            }).ToListAsync();

        data.InvoiceItems = await context.InvoiceItems.AsNoTracking()
            .Select(i => new InvoiceItemBackup
            {
                Id = i.Id,
                InvoiceId = i.InvoiceId,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                CostPrice = i.CostPrice,
                DiscountAmount = i.DiscountAmount,
                Subtotal = i.Subtotal,
                Total = i.Total,
                CreatedAt = i.CreatedAt
            }).ToListAsync();

        // المصروفات
        data.Expenses = await context.Expenses.AsNoTracking()
            .Select(e => new ExpenseBackup
            {
                Id = e.Id,
                ExpenseNumber = e.ExpenseNumber,
                CategoryId = e.CategoryId,
                CashboxId = e.CashboxId,
                CurrencyId = e.CurrencyId,
                Amount = e.Amount,
                Description = e.Description,
                ReferenceNumber = e.ReferenceNumber,
                ExpenseDate = e.ExpenseDate,
                CreatedAt = e.CreatedAt
            }).ToListAsync();

        // المستخدمين والأدوار - استخدام نماذج Backup بدلاً من الكيانات مباشرة
        data.Roles = await context.Roles.AsNoTracking()
            .Select(r => new RoleBackup
            {
                Id = r.Id,
                Name = r.Name,
                IsSystem = r.IsSystem,
                CreatedAt = r.CreatedAt
            }).ToListAsync();

        data.Permissions = await context.Permissions.AsNoTracking()
            .Select(p => new PermissionBackup
            {
                Id = p.Id,
                Module = p.Module,
                Name = p.Name,
                DisplayName = p.DisplayName,
                CreatedAt = p.CreatedAt
            }).ToListAsync();

        data.Users = await context.Users.AsNoTracking()
            .Select(u => new UserBackup
            {
                Id = u.Id,
                Username = u.Username,
                PasswordHash = u.PasswordHash,
                FullName = u.FullName,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToListAsync();

        data.RolePermissions = await context.RolePermissions.AsNoTracking()
            .Select(rp => new RolePermissionBackup
            {
                RoleId = rp.RoleId,
                PermissionId = rp.PermissionId,
                CreatedAt = rp.CreatedAt
            }).ToListAsync();

        data.UserRoles = await context.UserRoles.AsNoTracking()
            .Select(ur => new UserRoleBackup
            {
                UserId = ur.UserId,
                RoleId = ur.RoleId,
                AssignedAt = ur.AssignedAt,
                AssignedBy = ur.AssignedBy
            }).ToListAsync();
    }

    /// <summary>
    /// قراءة معلومات النسخة الاحتياطية
    /// </summary>
    public async Task<BackupInfo?> ReadBackupInfoAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var backup = JsonSerializer.Deserialize<BackupData>(json, _jsonOptions);
            return backup?.Info;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// استعادة النسخة الاحتياطية
    /// </summary>
    public async Task RestoreBackupAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var backup = JsonSerializer.Deserialize<BackupData>(json, _jsonOptions);

        if (backup?.Data == null)
            throw new Exception("ملف النسخة الاحتياطية غير صالح");

        // استخدام DbContext جديد للاستعادة
        using var context = _contextFactory();

        // استخدام transaction للتأكد من استعادة كل شيء أو لا شيء
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // حذف البيانات الحالية بالترتيب الصحيح (بسبب العلاقات)
            await DeleteAllDataAsync(context);

            // استعادة البيانات بالترتيب الصحيح
            await RestoreDataAsync(context, backup.Data);

            await transaction.CommitAsync();
            
            // تطبيق EF Migrations بعد الاستعادة
            await ApplyMigrationsAfterRestoreAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    /// <summary>
    /// تطبيق EF Migrations بعد الاستعادة
    /// </summary>
    private async Task ApplyMigrationsAfterRestoreAsync()
    {
        try
        {
            using var context = _contextFactory();
            // تطبيق أي Migrations معلقة باستخدام EF Core
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Migration after restore error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// استعادة النسخة الاحتياطية مع إرجاع نتيجة التحديثات
    /// </summary>
    public async Task<RestoreResult> RestoreBackupWithMigrationInfoAsync(string filePath)
    {
        var result = new RestoreResult();
        
        var json = await File.ReadAllTextAsync(filePath);
        var backup = JsonSerializer.Deserialize<BackupData>(json, _jsonOptions);

        if (backup?.Data == null)
        {
            result.Success = false;
            result.ErrorMessage = "ملف النسخة الاحتياطية غير صالح";
            return result;
        }

        result.BackupInfo = backup.Info;

        using var context = _contextFactory();
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            await DeleteAllDataAsync(context);
            await RestoreDataAsync(context, backup.Data);
            await transaction.CommitAsync();
            
            // تطبيق EF Migrations إذا لزم الأمر
            await ApplyMigrationsAfterRestoreAsync();
            result.MigrationApplied = true;
            
            result.Success = true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// حذف جميع البيانات الحالية
    /// </summary>
    private async Task DeleteAllDataAsync(AppDbContext context)
    {
        // حذف بالترتيب العكسي للعلاقات
        await context.Database.ExecuteSqlRawAsync("DELETE FROM InvoicePayments");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM InvoiceItems");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Invoices");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM TailoringOrderPayments");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM TailoringOrderItems");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM TailoringOrderStatusHistory");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM TailoringOrders");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Expenses");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM CashboxTransactions");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM InventoryMovements");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Inventories");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Products");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Cashboxes");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Customers");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Categories");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM ExpenseCategories");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Stores");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM RolePermissions");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM UserRoles");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Users");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Permissions");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Roles");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Currencies");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM SystemSettings");
    }

    /// <summary>
    /// استعادة البيانات
    /// </summary>
    private async Task RestoreDataAsync(AppDbContext context, BackupTables data)
    {
        // العملات
        if (data.Currencies.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Currencies ON");
            context.Currencies.AddRange(data.Currencies);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Currencies OFF");
        }

        // الأدوار
        if (data.Roles.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Roles ON");
            foreach (var r in data.Roles)
            {
                context.Roles.Add(new Role
                {
                    Id = r.Id,
                    Name = r.Name,
                    IsSystem = r.IsSystem,
                    CreatedAt = r.CreatedAt
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Roles OFF");
        }

        // الصلاحيات
        if (data.Permissions.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Permissions ON");
            foreach (var p in data.Permissions)
            {
                context.Permissions.Add(new Permission
                {
                    Id = p.Id,
                    Module = p.Module,
                    Name = p.Name,
                    DisplayName = p.DisplayName,
                    CreatedAt = p.CreatedAt
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Permissions OFF");
        }

        // المستخدمين
        if (data.Users.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Users ON");
            foreach (var u in data.Users)
            {
                context.Users.Add(new User
                {
                    Id = u.Id,
                    Username = u.Username,
                    PasswordHash = u.PasswordHash,
                    FullName = u.FullName,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Users OFF");
        }

        // صلاحيات الأدوار (no identity - composite key)
        if (data.RolePermissions.Any())
        {
            foreach (var rp in data.RolePermissions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = rp.RoleId,
                    PermissionId = rp.PermissionId,
                    CreatedAt = rp.CreatedAt
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        // أدوار المستخدمين (no identity - composite key)
        if (data.UserRoles.Any())
        {
            foreach (var ur in data.UserRoles)
            {
                context.UserRoles.Add(new UserRole
                {
                    UserId = ur.UserId,
                    RoleId = ur.RoleId,
                    AssignedAt = ur.AssignedAt,
                    AssignedBy = ur.AssignedBy
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        // التصنيفات
        if (data.Categories.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Categories ON");
            context.Categories.AddRange(data.Categories);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Categories OFF");
        }

        // فئات المصروفات
        if (data.ExpenseCategories.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT ExpenseCategories ON");
            context.ExpenseCategories.AddRange(data.ExpenseCategories);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT ExpenseCategories OFF");
        }

        // المخازن
        if (data.Stores.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Stores ON");
            context.Stores.AddRange(data.Stores);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Stores OFF");
        }

        // العملاء
        if (data.Customers.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Customers ON");
            context.Customers.AddRange(data.Customers);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Customers OFF");
        }

        // الصناديق
        if (data.Cashboxes.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Cashboxes ON");
            foreach (var c in data.Cashboxes)
            {
                context.Cashboxes.Add(new Cashbox
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type,
                    CurrencyId = c.CurrencyId,
                    CurrentBalance = c.CurrentBalance,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Cashboxes OFF");
        }

        // المنتجات
        if (data.Products.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Products ON");
            foreach (var p in data.Products)
            {
                context.Products.Add(new Product
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Barcode = p.Barcode,
                    CategoryId = p.CategoryId,
                    CashboxId = p.CashboxId,
                    CostPrice = p.CostPrice,
                    SellingPrice = p.SellingPrice,
                    CurrencyId = p.CurrencyId,
                    Unit = p.Unit,
                    MinStockLevel = p.MinStockLevel,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Products OFF");
        }

        // المخزون
        if (data.Inventories.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Inventories ON");
            foreach (var i in data.Inventories)
            {
                context.Inventories.Add(new Inventory
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    StoreId = i.StoreId,
                    Quantity = i.Quantity,
                    CreatedAt = i.CreatedAt
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Inventories OFF");
        }

        // معاملات الصناديق
        if (data.CashboxTransactions.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT CashboxTransactions ON");
            foreach (var t in data.CashboxTransactions)
            {
                context.CashboxTransactions.Add(new CashboxTransaction
                {
                    Id = t.Id,
                    CashboxId = t.CashboxId,
                    Type = t.Type,
                    Amount = t.Amount,
                    BalanceBefore = t.BalanceBefore,
                    BalanceAfter = t.BalanceAfter,
                    ReferenceType = t.ReferenceType,
                    ReferenceId = t.ReferenceId,
                    CreatedBy = t.CreatedBy,
                    CreatedAt = t.CreatedAt
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT CashboxTransactions OFF");
        }

        // طلبات الفصال
        if (data.TailoringOrders.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT TailoringOrders ON");
            foreach (var o in data.TailoringOrders)
            {
                context.TailoringOrders.Add(new TailoringOrder
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerId = o.CustomerId,
                    CashboxId = o.CashboxId,
                    CurrencyId = o.CurrencyId,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    DepositAmount = o.DepositAmount,
                    RemainingAmount = o.RemainingAmount,
                    PaymentStatus = o.PaymentStatus,
                    Description = o.Description,
                    Notes = o.Notes,
                    DeliveryDate = o.DeliveryDate,
                    ActualDeliveryDate = o.ActualDeliveryDate,
                    OrderDate = o.OrderDate,
                    CreatedAt = o.CreatedAt
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT TailoringOrders OFF");
        }

        // بنود طلبات الفصال
        if (data.TailoringOrderItems.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT TailoringOrderItems ON");
            foreach (var i in data.TailoringOrderItems)
            {
                context.TailoringOrderItems.Add(new TailoringOrderItem
                {
                    Id = i.Id,
                    OrderId = i.OrderId,
                    ItemName = i.ItemName,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Total = i.Total,
                    CreatedAt = i.CreatedAt
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT TailoringOrderItems OFF");
        }

        // مدفوعات طلبات الفصال
        if (data.TailoringOrderPayments.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT TailoringOrderPayments ON");
            foreach (var p in data.TailoringOrderPayments)
            {
                context.TailoringOrderPayments.Add(new TailoringOrderPayment
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    CashboxId = p.CashboxId,
                    Amount = p.Amount,
                    PaymentType = p.PaymentType,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate,
                    CreatedAt = p.CreatedAt
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT TailoringOrderPayments OFF");
        }

        // الفواتير
        if (data.Invoices.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Invoices ON");
            foreach (var i in data.Invoices)
            {
                context.Invoices.Add(new Invoice
                {
                    Id = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    Type = i.Type,
                    Status = i.Status,
                    CustomerId = i.CustomerId,
                    SupplierName = i.SupplierName,
                    CashboxId = i.CashboxId,
                    StoreId = i.StoreId,
                    CurrencyId = i.CurrencyId,
                    Subtotal = i.Subtotal,
                    DiscountAmount = i.DiscountAmount,
                    Total = i.Total,
                    PaidAmount = i.PaidAmount,
                    RemainingAmount = i.RemainingAmount,
                    PaymentStatus = i.PaymentStatus,
                    Notes = i.Notes,
                    InvoiceDate = i.InvoiceDate,
                    CreatedAt = i.CreatedAt
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Invoices OFF");
        }

        // بنود الفواتير
        if (data.InvoiceItems.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT InvoiceItems ON");
            foreach (var i in data.InvoiceItems)
            {
                context.InvoiceItems.Add(new InvoiceItem
                {
                    Id = i.Id,
                    InvoiceId = i.InvoiceId,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    CostPrice = i.CostPrice,
                    DiscountAmount = i.DiscountAmount,
                    Subtotal = i.Subtotal,
                    Total = i.Total,
                    CreatedAt = i.CreatedAt
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT InvoiceItems OFF");
        }

        // المصروفات
        if (data.Expenses.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Expenses ON");
            foreach (var e in data.Expenses)
            {
                context.Expenses.Add(new Expense
                {
                    Id = e.Id,
                    ExpenseNumber = e.ExpenseNumber,
                    CategoryId = e.CategoryId,
                    CashboxId = e.CashboxId,
                    CurrencyId = e.CurrencyId,
                    Amount = e.Amount,
                    Description = e.Description,
                    ReferenceNumber = e.ReferenceNumber,
                    ExpenseDate = e.ExpenseDate,
                    CreatedAt = e.CreatedAt
                });
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Expenses OFF");
        }

        // إعدادات النظام
        if (data.SystemSettings.Any())
        {
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT SystemSettings ON");
            context.SystemSettings.AddRange(data.SystemSettings);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT SystemSettings OFF");
        }
    }

    /// <summary>
    /// الحصول على إحصائيات النسخة الاحتياطية
    /// </summary>
    public async Task<Dictionary<string, int>> GetBackupStatsAsync()
    {
        using var context = _contextFactory();
        
        return new Dictionary<string, int>
        {
            { "العملات", await context.Currencies.CountAsync() },
            { "التصنيفات", await context.Categories.CountAsync() },
            { "المنتجات", await context.Products.CountAsync() },
            { "العملاء", await context.Customers.CountAsync() },
            { "الصناديق", await context.Cashboxes.CountAsync() },
            { "طلبات الفصال", await context.TailoringOrders.CountAsync() },
            { "الفواتير", await context.Invoices.CountAsync() },
            { "المصروفات", await context.Expenses.CountAsync() },
            { "المستخدمين", await context.Users.CountAsync() }
        };
    }
}

/// <summary>
/// نتيجة عملية استعادة النسخة الاحتياطية
/// </summary>
public class RestoreResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public BackupInfo? BackupInfo { get; set; }
    public bool MigrationApplied { get; set; }
    public string? MigrationMessage { get; set; }
}

/// <summary>
/// نتيجة عملية النسخ الاحتياطي SQL Server
/// </summary>
public class SqlBackupResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// حجم الملف بصيغة مقروءة
    /// </summary>
    public string FileSizeFormatted
    {
        get
        {
            if (FileSize < 1024) return $"{FileSize} B";
            if (FileSize < 1024 * 1024) return $"{FileSize / 1024.0:F2} KB";
            if (FileSize < 1024 * 1024 * 1024) return $"{FileSize / (1024.0 * 1024):F2} MB";
            return $"{FileSize / (1024.0 * 1024 * 1024):F2} GB";
        }
    }
}
