using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TailoringShop.Core.Interfaces.Repositories;
using TailoringShop.Core.Interfaces.Services;
using TailoringShop.Infrastructure.Data;
using TailoringShop.Infrastructure.Repositories;
using TailoringShop.Infrastructure.Services;
using TailoringShop.WPF.Services;
using TailoringShop.WPF.ViewModels;
using TailoringShop.WPF.Views.Windows;

namespace TailoringShop.WPF;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    public static Core.Entities.User? CurrentUser { get; set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // ═══════════════════════════════════════════════════════════════
        // إعداد التقاط الأخطاء العامة (Global Exception Handling)
        // ═══════════════════════════════════════════════════════════════
        SetupExceptionHandling();
        
        // تنظيف السجلات القديمة (أقدم من 30 يوم)
        ErrorLogService.Instance.CleanOldLogs(30);
        ErrorLogService.Instance.LogInfo("Application started");

        // ═══════════════════════════════════════════════════════════════
        // التحقق من الترخيص
        // ═══════════════════════════════════════════════════════════════
        if (!CheckLicense())
        {
            ErrorLogService.Instance.LogInfo("License check failed - Application shutdown");
            Environment.Exit(0);
            return;
        }
        
        ErrorLogService.Instance.LogInfo("License verified successfully");

        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        // Initialize Database with seed data
        // يستخدم Migrate() داخلياً لتطبيق التحديثات تلقائياً
        try
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await DbInitializer.InitializeAsync(context);
            }
            ErrorLogService.Instance.LogInfo("Database initialized successfully");
        }
        catch (Exception ex)
        {
            ErrorLogService.Instance.LogError(ex, "Database initialization failed");
            MessageBox.Show(
                $"خطأ في الاتصال بقاعدة البيانات:\n{ex.Message}\n\nتحقق من تثبيت SQL Server LocalDB",
                "خطأ في قاعدة البيانات",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        // Initialize AutoBackupService
        InitializeAutoBackup();
    }

    /// <summary>
    /// التحقق من الترخيص
    /// </summary>
    private bool CheckLicense()
    {
        var result = LicenseService.CheckLicense();
        
        if (result.IsValid)
        {
            // عرض رسالة إذا كانت الفترة التجريبية قاربت على الانتهاء
            if (result.Type == LicenseType.Trial && result.DaysRemaining <= 2)
            {
                MessageBox.Show(
                    $"⚠️ تنبيه: متبقي {result.DaysRemaining} يوم فقط على انتهاء الفترة التجريبية!\n\n" +
                    "يرجى الحصول على ترخيص دائمي للاستمرار في استخدام البرنامج.",
                    "تنبيه - الترخيص",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            }
            return true;
        }
        
        // فتح نافذة التفعيل
        var activationWindow = new ActivationWindow();
        var dialogResult = activationWindow.ShowDialog();
        
        return dialogResult == true && activationWindow.IsActivated;
    }

    /// <summary>
    /// إعداد التقاط الأخطاء العامة
    /// </summary>
    private void SetupExceptionHandling()
    {
        // أخطاء UI Thread
        DispatcherUnhandledException += (sender, e) =>
        {
            ErrorLogService.Instance.LogError(e.Exception, "Unhandled UI Exception");
            
            MessageBox.Show(
                $"حدث خطأ غير متوقع:\n{e.Exception.Message}\n\nتم تسجيل الخطأ في ملف السجل.",
                "خطأ",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            e.Handled = true; // منع إغلاق التطبيق
        };

        // أخطاء Background Threads
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                ErrorLogService.Instance.LogError(exception, "Unhandled Domain Exception");
            }
        };

        // أخطاء Tasks
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            ErrorLogService.Instance.LogError(e.Exception, "Unobserved Task Exception");
            e.SetObserved(); // منع إغلاق التطبيق
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ErrorLogService.Instance.LogInfo("Application closed");
        base.OnExit(e);
    }

    /// <summary>
    /// تهيئة خدمة النسخ الاحتياطي التلقائي
    /// </summary>
    private void InitializeAutoBackup()
    {
        try
        {
            // مجلد النسخ الاحتياطية داخل مجلد التطبيق
            var appFolder = AppDomain.CurrentDomain.BaseDirectory;
            var backupFolder = Path.Combine(appFolder, "Backup");

            // تفعيل النسخ الاحتياطي التلقائي
            AutoBackupService.Instance.Initialize(backupFolder, enableAutoBackup: true);
            
            ErrorLogService.Instance.LogInfo($"AutoBackup initialized: {backupFolder}");
        }
        catch (Exception ex)
        {
            // لا نوقف التطبيق بسبب خطأ في النسخ الاحتياطي
            ErrorLogService.Instance.LogError(ex, "AutoBackup initialization error");
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Database Context
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=TailoringShopDB;Trusted_Connection=True;MultipleActiveResultSets=true",
                b => b.MigrationsAssembly("TailoringShop.Infrastructure"));
        });

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICashboxRepository, CashboxRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<ITailoringOrderRepository, TailoringOrderRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<ISettingRepository, SettingRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICashboxService, CashboxService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<CustomersViewModel>();
        services.AddTransient<ProductsViewModel>();
        services.AddTransient<TailoringOrdersViewModel>();
        services.AddTransient<InvoicesViewModel>();
        services.AddTransient<ExpensesViewModel>();
        services.AddTransient<CashboxesViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<UsersViewModel>();
        services.AddTransient<InvoiceSettingsViewModel>();
    }

    public static T GetService<T>() where T : class
    {
        return ServiceProvider.GetRequiredService<T>();
    }
}
