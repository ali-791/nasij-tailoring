using Microsoft.EntityFrameworkCore;
using TailoringShop.Core.Entities;
using TailoringShop.Core.Enums;

namespace TailoringShop.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // ═══════════════════════════════════════════════════════════════
        // التعامل مع قاعدة البيانات بشكل آمن
        // ═══════════════════════════════════════════════════════════════
        
        var canConnect = await context.Database.CanConnectAsync();
        
        if (!canConnect)
        {
            // قاعدة بيانات جديدة
            await CreateDatabaseAsync(context);
        }
        else
        {
            // قاعدة بيانات موجودة - التحقق من وجود الجداول
            var hasRolesTable = await HasTableAsync(context, "Roles");
            
            if (!hasRolesTable)
            {
                // قاعدة بيانات موجودة لكن بدون جداول - إنشاء الجداول
                await CreateDatabaseAsync(context);
            }
            else
            {
                // قاعدة بيانات موجودة مع جداول - محاولة تطبيق Migrations
                await TryApplyMigrationsAsync(context);
            }
        }

        // تحديث أسماء عرض الأدوار للقواعد البيانات القديمة
        await SafeUpdateRolesDisplayNameAsync(context);

        // Check if already seeded
        if (await context.Users.AnyAsync())
            return;

        // Seed data...
        await SeedDataAsync(context);
    }

    /// <summary>
    /// إنشاء قاعدة البيانات والجداول
    /// </summary>
    private static async Task CreateDatabaseAsync(AppDbContext context)
    {
        try
        {
            // محاولة استخدام Migrations أولاً
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            
            if (pendingMigrations.Any())
            {
                // توجد Migrations - تطبيقها
                await context.Database.MigrateAsync();
            }
            else
            {
                // لا توجد Migrations - استخدام EnsureCreated
                await context.Database.EnsureCreatedAsync();
            }
        }
        catch
        {
            // في حالة الفشل، استخدام EnsureCreated
            await context.Database.EnsureCreatedAsync();
        }
    }

    /// <summary>
    /// محاولة تطبيق Migrations على قاعدة بيانات موجودة
    /// </summary>
    private static async Task TryApplyMigrationsAsync(AppDbContext context)
    {
        try
        {
            var hasMigrationsTable = await HasTableAsync(context, "__EFMigrationsHistory");
            
            if (hasMigrationsTable)
            {
                // قاعدة بيانات تستخدم Migrations - تطبيق التحديثات
                await context.Database.MigrateAsync();
            }
            else
            {
                // قاعدة بيانات قديمة (EnsureCreated) - إنشاء جدول Migrations
                await HandleLegacyDatabaseAsync(context);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Migration error: {ex.Message}");
        }
    }

    /// <summary>
    /// التحقق من وجود جدول معين
    /// </summary>
    private static async Task<bool> HasTableAsync(AppDbContext context, string tableName)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = '{tableName}'
                ) THEN 1 ELSE 0 END";

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) == 1;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// تحديث أسماء عرض الأدوار بشكل آمن
    /// </summary>
    private static async Task SafeUpdateRolesDisplayNameAsync(AppDbContext context)
    {
        try
        {
            // التحقق من وجود جدول Roles أولاً
            if (!await HasTableAsync(context, "Roles"))
                return;

            var roleDisplayNames = new Dictionary<string, string>
            {
                { "admin", "مدير النظام" },
                { "manager", "مدير" },
                { "cashier", "كاشير" },
                { "user", "مستخدم" }
            };

            var roles = await context.Roles.ToListAsync();
            var updated = false;

            foreach (var role in roles)
            {
                if (string.IsNullOrWhiteSpace(role.DisplayName) && roleDisplayNames.ContainsKey(role.Name))
                {
                    role.DisplayName = roleDisplayNames[role.Name];
                    updated = true;
                }
            }

            if (updated)
            {
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateRolesDisplayName error: {ex.Message}");
        }
    }

    /// <summary>
    /// معالجة قاعدة البيانات القديمة (التي أُنشئت بـ EnsureCreated)
    /// </summary>
    private static async Task HandleLegacyDatabaseAsync(AppDbContext context)
    {
        try
        {
            // إنشاء جدول __EFMigrationsHistory يدوياً
            await context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory')
                BEGIN
                    CREATE TABLE [__EFMigrationsHistory] (
                        [MigrationId] nvarchar(150) NOT NULL,
                        [ProductVersion] nvarchar(32) NOT NULL,
                        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                    );
                    
                    -- إضافة سجل للـ Migration الأولية (Schema الحالي)
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES ('20260309000000_InitialCreate', '8.0.0');
                END
            ");

            // الآن يمكن تطبيق أي Migrations جديدة
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Legacy database handling error: {ex.Message}");
        }
    }

    /// <summary>
    /// إضافة البيانات الأولية
    /// </summary>
    private static async Task SeedDataAsync(AppDbContext context)
    {
        // Seed Currencies
        var currencies = new List<Currency>
        {
            new() { Code = "IQD", Name = "دينار عراقي", Symbol = "د.ع", ExchangeRate = 1, IsDefault = true, IsActive = true, DecimalPlaces = 0 },
            new() { Code = "USD", Name = "دولار أمريكي", Symbol = "$", ExchangeRate = 1480, IsDefault = false, IsActive = true, DecimalPlaces = 2 },
            new() { Code = "SAR", Name = "ريال سعودي", Symbol = "ر.س", ExchangeRate = 394, IsDefault = false, IsActive = true, DecimalPlaces = 2 }
        };
        await context.Currencies.AddRangeAsync(currencies);
        await context.SaveChangesAsync();

        var defaultCurrency = currencies.First(c => c.IsDefault);

        // Seed System Settings
        var systemSettings = new SystemSettings
        {
            ShopName = "محل الخياطة",
            ShopPhone = "",
            ShopAddress = "",
            EnableNotifications = true,
            EnableAutoBackup = false
        };
        await context.SystemSettings.AddAsync(systemSettings);
        await context.SaveChangesAsync();

        // Seed Roles
        var roles = new List<Role>
        {
            new() { Name = "admin", DisplayName = "مدير النظام", IsSystem = true, IsActive = true },
            new() { Name = "manager", DisplayName = "مدير", IsSystem = false, IsActive = true },
            new() { Name = "cashier", DisplayName = "كاشير", IsSystem = false, IsActive = true },
            new() { Name = "user", DisplayName = "مستخدم", IsSystem = false, IsActive = true }
        };
        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();

        // Seed Permissions
        var permissions = new List<Permission>
        {
            // Users
            new() { Module = "users", Name = "view", DisplayName = "عرض المستخدمين" },
            new() { Module = "users", Name = "create", DisplayName = "إنشاء مستخدم" },
            new() { Module = "users", Name = "update", DisplayName = "تعديل مستخدم" },
            new() { Module = "users", Name = "delete", DisplayName = "حذف مستخدم" },
            // Customers
            new() { Module = "customers", Name = "view", DisplayName = "عرض العملاء" },
            new() { Module = "customers", Name = "create", DisplayName = "إنشاء عميل" },
            new() { Module = "customers", Name = "update", DisplayName = "تعديل عميل" },
            new() { Module = "customers", Name = "delete", DisplayName = "حذف عميل" },
            // Products
            new() { Module = "products", Name = "view", DisplayName = "عرض المنتجات" },
            new() { Module = "products", Name = "create", DisplayName = "إنشاء منتج" },
            new() { Module = "products", Name = "update", DisplayName = "تعديل منتج" },
            new() { Module = "products", Name = "delete", DisplayName = "حذف منتج" },
            // Invoices
            new() { Module = "invoices", Name = "view", DisplayName = "عرض الفواتير" },
            new() { Module = "invoices", Name = "create", DisplayName = "إنشاء فاتورة" },
            new() { Module = "invoices", Name = "update", DisplayName = "تعديل فاتورة" },
            new() { Module = "invoices", Name = "delete", DisplayName = "حذف فاتورة" },
            // Tailoring
            new() { Module = "tailoring", Name = "view", DisplayName = "عرض طلبات الفصال" },
            new() { Module = "tailoring", Name = "create", DisplayName = "إنشاء طلب فصال" },
            new() { Module = "tailoring", Name = "update", DisplayName = "تعديل طلب فصال" },
            new() { Module = "tailoring", Name = "delete", DisplayName = "حذف طلب فصال" },
            // Expenses
            new() { Module = "expenses", Name = "view", DisplayName = "عرض المصروفات" },
            new() { Module = "expenses", Name = "create", DisplayName = "تسجيل مصروف" },
            new() { Module = "expenses", Name = "update", DisplayName = "تعديل مصروف" },
            new() { Module = "expenses", Name = "delete", DisplayName = "حذف مصروف" },
            // Cashboxes
            new() { Module = "cashboxes", Name = "view", DisplayName = "عرض القاصات" },
            new() { Module = "cashboxes", Name = "manage", DisplayName = "إدارة القاصات" },
            // Reports
            new() { Module = "reports", Name = "view", DisplayName = "عرض التقارير" },
            new() { Module = "reports", Name = "export", DisplayName = "تصدير التقارير" },
            // Settings
            new() { Module = "settings", Name = "view", DisplayName = "عرض الإعدادات" },
            new() { Module = "settings", Name = "update", DisplayName = "تعديل الإعدادات" }
        };
        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        // Assign all permissions to admin role
        var adminRole = roles.First(r => r.Name == "admin");
        foreach (var permission in permissions)
        {
            context.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = permission.Id });
        }
        await context.SaveChangesAsync();

        // Seed Admin User (password: admin123)
        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            FullName = "مدير النظام",
            IsActive = true
        };
        await context.Users.AddAsync(adminUser);
        await context.SaveChangesAsync();

        // Assign admin role to admin user
        context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
        await context.SaveChangesAsync();

        // Seed Default Store
        var store = new Store
        {
            Name = "المخزن الرئيسي",
            Location = "داخل المحل",
            IsDefault = true,
            IsActive = true
        };
        await context.Stores.AddAsync(store);
        await context.SaveChangesAsync();

        // Seed Cashboxes
        var cashboxes = new List<Cashbox>
        {
            new() { Name = "قاصة الفصال", Type = CashboxType.Tailoring, CurrencyId = defaultCurrency.Id, CurrentBalance = 0, IsActive = true },
            new() { Name = "قاصة الملابس الجاهزة", Type = CashboxType.ReadyMade, CurrencyId = defaultCurrency.Id, CurrentBalance = 0, IsActive = true },
            new() { Name = "قاصة العطور", Type = CashboxType.Perfumes, CurrencyId = defaultCurrency.Id, CurrentBalance = 0, IsActive = true },
            new() { Name = "القاصة العامة", Type = CashboxType.General, CurrencyId = defaultCurrency.Id, CurrentBalance = 0, IsActive = true }
        };
        await context.Cashboxes.AddRangeAsync(cashboxes);
        await context.SaveChangesAsync();

        // Seed Expense Categories
        var expenseCategories = new List<ExpenseCategory>
        {
            new() { Name = "إيجار", IsActive = true },
            new() { Name = "كهرباء", IsActive = true },
            new() { Name = "ماء", IsActive = true },
            new() { Name = "رواتب", IsActive = true },
            new() { Name = "مواد خام", IsActive = true },
            new() { Name = "صيانة", IsActive = true },
            new() { Name = "نقل ومواصلات", IsActive = true },
            new() { Name = "دعاية وإعلان", IsActive = true },
            new() { Name = "مصاريف بنكية", IsActive = true },
            new() { Name = "أخرى", IsActive = true }
        };
        await context.ExpenseCategories.AddRangeAsync(expenseCategories);
        await context.SaveChangesAsync();

        // Seed Categories
        var categories = new List<Category>
        {
            new() { Name = "أقمشة", IsActive = true },
            new() { Name = "ملابس رجالية", IsActive = true },
            new() { Name = "ملابس نسائية", IsActive = true },
            new() { Name = "ملابس أطفال", IsActive = true },
            new() { Name = "عطور", IsActive = true },
            new() { Name = "إكسسوارات", IsActive = true }
        };
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // Seed Settings
        var settings = new List<Setting>
        {
            new() { Group = "general", Key = "shop_name", Value = "متجر الخياطة", Type = "string", Description = "اسم المتجر" },
            new() { Group = "general", Key = "shop_phone", Value = "", Type = "string", Description = "رقم هاتف المتجر" },
            new() { Group = "general", Key = "shop_address", Value = "", Type = "string", Description = "عنوان المتجر" },
            new() { Group = "invoice", Key = "sale_prefix", Value = "INV-", Type = "string", Description = "بادئة فاتورة البيع" },
            new() { Group = "invoice", Key = "purchase_prefix", Value = "PUR-", Type = "string", Description = "بادئة فاتورة الشراء" },
            new() { Group = "invoice", Key = "tailoring_prefix", Value = "TLR-", Type = "string", Description = "بادئة طلب الفصال" },
            new() { Group = "invoice", Key = "expense_prefix", Value = "EXP-", Type = "string", Description = "بادئة المصروفات" },
            new() { Group = "tailoring", Key = "default_delivery_days", Value = "7", Type = "integer", Description = "أيام التسليم الافتراضية" },
            new() { Group = "tailoring", Key = "require_deposit", Value = "true", Type = "boolean", Description = "إلزامية العربون" },
            new() { Group = "tailoring", Key = "min_deposit_percentage", Value = "50", Type = "integer", Description = "نسبة الحد الأدنى للعربون" }
        };
        await context.Settings.AddRangeAsync(settings);
        await context.SaveChangesAsync();
    }
}
