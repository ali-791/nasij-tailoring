using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using TailoringShop.Core.Entities;
using TailoringShop.Infrastructure.Data;
using TailoringShop.WPF.Services;
using TailoringShop.WPF.ViewModels;

namespace TailoringShop.WPF.Views.Pages;

public partial class SettingsPage : UserControl
{
    private SettingsViewModel? ViewModel => DataContext as SettingsViewModel;
    private DispatcherTimer? _statusUpdateTimer;
    private string? _selectedLogoPath;

    // Factory لإنشاء DbContext جديد في كل مرة
    private Func<AppDbContext> CreateDbContext => () =>
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = "Server=.;Database=TailoringShopDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        
        // الحصول على connection string من context موجود
        var existingContext = App.ServiceProvider?.GetService(typeof(AppDbContext)) as AppDbContext;
        if (existingContext != null)
        {
            connectionString = existingContext.Database.GetConnectionString() ?? connectionString;
        }
        
        optionsBuilder.UseSqlServer(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    };

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += SettingsPage_Loaded;
        Unloaded += SettingsPage_Unloaded;
    }

    private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadBackupStatsAsync();
        UpdateAutoBackupStatus();
        LoadShopSettings();
        LoadLicenseInfo();
        
        // بدء مؤقت تحديث الحالة
        _statusUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _statusUpdateTimer.Tick += (s, args) => UpdateAutoBackupStatus();
        _statusUpdateTimer.Start();
        
        // الاشتراك في أحداث AutoBackupService
        AutoBackupService.Instance.StatusChanged += UpdateAutoBackupStatus;
        AutoBackupService.Instance.BackupCompleted += OnAutoBackupCompleted;
    }

    private void LoadShopSettings()
    {
        try
        {
            // تحميل اللوغو
            var logoPath = ShopSettingsService.GetLogoPath();
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                _selectedLogoPath = logoPath;
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(logoPath, UriKind.Absolute);
                bitmap.EndInit();
                ShopLogoPreview.Source = bitmap;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading shop settings: {ex.Message}");
        }
    }

    private void SelectShopLogo_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "اختيار شعار المحل",
                Filter = "ملفات الصور|*.png;*.jpg;*.jpeg;*.bmp;*.gif|جميع الملفات|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedLogoPath = openFileDialog.FileName;
                
                // عرض الصورة في المعاينة
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(_selectedLogoPath, UriKind.Absolute);
                bitmap.EndInit();
                ShopLogoPreview.Source = bitmap;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ أثناء اختيار الصورة:\n{ex.Message}",
                "خطأ",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }
    }

    private void RemoveShopLogo_Click(object sender, RoutedEventArgs e)
    {
        _selectedLogoPath = null;
        ShopLogoPreview.Source = null;
    }

    private void SaveShopSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var settings = ShopSettingsService.LoadSettings();
            
            // الحصول على اسم المحل من ViewModel
            if (ViewModel != null)
            {
                settings.ShopName = ViewModel.ShopName ?? "متجر الخياطة";
            }
            
            // حفظ اللوغو إذا تم اختياره
            if (!string.IsNullOrEmpty(_selectedLogoPath))
            {
                var currentLogoPath = ShopSettingsService.GetLogoPath();
                
                // إذا كان اللوغو جديد (ليس نفس المسار المحفوظ)
                if (_selectedLogoPath != currentLogoPath)
                {
                    var savedPath = ShopSettingsService.SaveLogo(_selectedLogoPath);
                    if (savedPath != null)
                    {
                        settings.LogoPath = savedPath;
                        _selectedLogoPath = savedPath;
                    }
                }
            }
            else
            {
                // حذف اللوغو
                ShopSettingsService.DeleteLogo();
                settings.LogoPath = null;
            }
            
            // حفظ الإعدادات
            ShopSettingsService.SaveSettings(settings);
            
            MessageBox.Show(
                "تم حفظ إعدادات المحل بنجاح!",
                "تم الحفظ",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ أثناء حفظ الإعدادات:\n{ex.Message}",
                "خطأ",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }
    }

    private void SettingsPage_Unloaded(object sender, RoutedEventArgs e)
    {
        _statusUpdateTimer?.Stop();
        AutoBackupService.Instance.StatusChanged -= UpdateAutoBackupStatus;
        AutoBackupService.Instance.BackupCompleted -= OnAutoBackupCompleted;
    }

    private void UpdateAutoBackupStatus()
    {
        Dispatcher.Invoke(() =>
        {
            var service = AutoBackupService.Instance;
            
            // تحديث حالة النسخ الاحتياطي
            if (service.IsEnabled)
            {
                AutoBackupStatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e8f5e9"));
                AutoBackupStatusText.Text = "✅ مفعّل";
                AutoBackupStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2e7d32"));
                ToggleAutoBackupButton.Content = "⏸️ إيقاف مؤقت";
            }
            else
            {
                AutoBackupStatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffebee"));
                AutoBackupStatusText.Text = "⏸️ متوقف";
                AutoBackupStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c62828"));
                ToggleAutoBackupButton.Content = "▶️ تفعيل";
            }
            
            // تحديث وقت آخر نسخة
            if (service.LastBackupTime > DateTime.MinValue)
            {
                LastBackupTimeText.Text = service.LastBackupTime.ToString("yyyy/MM/dd HH:mm");
            }
            else
            {
                LastBackupTimeText.Text = "لم تتم بعد";
            }
            
            // تحديث وقت النسخة القادمة
            if (service.IsEnabled)
            {
                var timeUntil = service.TimeUntilNextBackup;
                if (timeUntil.TotalHours >= 1)
                {
                    NextBackupTimeText.Text = $"خلال {timeUntil.Hours}س {timeUntil.Minutes}د";
                }
                else if (timeUntil.TotalMinutes >= 1)
                {
                    NextBackupTimeText.Text = $"خلال {timeUntil.Minutes} دقيقة";
                }
                else
                {
                    NextBackupTimeText.Text = "قريباً";
                }
            }
            else
            {
                NextBackupTimeText.Text = "--";
            }
        });
    }

    private void OnAutoBackupCompleted(bool success, string message)
    {
        Dispatcher.Invoke(() =>
        {
            if (success)
            {
                // تحديث الإحصائيات عند نجاح النسخ الاحتياطي
                _ = LoadBackupStatsAsync();
            }
        });
    }

    private void ToggleAutoBackup_Click(object sender, RoutedEventArgs e)
    {
        var service = AutoBackupService.Instance;
        service.IsEnabled = !service.IsEnabled;
        UpdateAutoBackupStatus();

        var status = service.IsEnabled ? "تفعيل" : "إيقاف";
        MessageBox.Show(
            $"تم {status} النسخ الاحتياطي التلقائي",
            "إعدادات النسخ الاحتياطي",
            MessageBoxButton.OK,
            MessageBoxImage.Information,
            MessageBoxResult.OK,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
    }

    private async void RunAutoBackupNow_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            
            var (success, message) = await AutoBackupService.Instance.PerformBackupAsync();
            
            Mouse.OverrideCursor = null;
            
            MessageBox.Show(
                message,
                success ? "تم بنجاح" : "تنبيه",
                MessageBoxButton.OK,
                success ? MessageBoxImage.Information : MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            
            UpdateAutoBackupStatus();
        }
        catch (Exception ex)
        {
            Mouse.OverrideCursor = null;
            MessageBox.Show(
                $"حدث خطأ: {ex.Message}",
                "خطأ",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }
    }

    private async Task LoadBackupStatsAsync()
    {
        try
        {
            var backupService = new BackupService(CreateDbContext);
            var stats = await backupService.GetBackupStatsAsync();

            ProductsCountText.Text = stats.GetValueOrDefault("المنتجات", 0).ToString();
            CustomersCountText.Text = stats.GetValueOrDefault("العملاء", 0).ToString();
            OrdersCountText.Text = stats.GetValueOrDefault("طلبات الفصال", 0).ToString();
            InvoicesCountText.Text = stats.GetValueOrDefault("الفواتير", 0).ToString();
            ExpensesCountText.Text = stats.GetValueOrDefault("المصروفات", 0).ToString();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading backup stats: {ex.Message}");
        }
    }

    private async void CreateBackup_Click(object sender, MouseButtonEventArgs e)
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "ملف النسخة الاحتياطية (*.json)|*.json",
                FileName = $"TailoringShop_Backup_{DateTime.Now:yyyy-MM-dd_HHmmss}.json",
                Title = "حفظ النسخة الاحتياطية",
                DefaultExt = "json"
            };

            if (saveDialog.ShowDialog() == true)
            {
                // تغيير المؤشر للانتظار
                Mouse.OverrideCursor = Cursors.Wait;

                var backupService = new BackupService(CreateDbContext);
                await backupService.CreateBackupAsync(saveDialog.FileName);

                // إعادة المؤشر للوضع الطبيعي
                Mouse.OverrideCursor = null;

                // الحصول على حجم الملف
                var fileInfo = new FileInfo(saveDialog.FileName);
                var fileSizeKB = fileInfo.Length / 1024.0;
                var fileSizeText = fileSizeKB > 1024 
                    ? $"{fileSizeKB / 1024:N2} MB" 
                    : $"{fileSizeKB:N2} KB";

                MessageBox.Show(
                    $"تم إنشاء النسخة الاحتياطية بنجاح!\n\n" +
                    $"📁 الملف: {Path.GetFileName(saveDialog.FileName)}\n" +
                    $"📊 الحجم: {fileSizeText}\n" +
                    $"📍 المسار: {Path.GetDirectoryName(saveDialog.FileName)}",
                    "تم بنجاح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    MessageBoxResult.OK,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            }
        }
        catch (Exception ex)
        {
            Mouse.OverrideCursor = null;
            MessageBox.Show(
                $"حدث خطأ أثناء إنشاء النسخة الاحتياطية:\n{ex.Message}",
                "خطأ",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }
    }

    private async void RestoreBackup_Click(object sender, MouseButtonEventArgs e)
    {
        try
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "ملف النسخة الاحتياطية (*.json)|*.json",
                Title = "اختيار ملف النسخة الاحتياطية"
            };

            if (openDialog.ShowDialog() == true)
            {
                var backupService = new BackupService(CreateDbContext);

                // قراءة معلومات النسخة الاحتياطية
                var backupInfo = await backupService.ReadBackupInfoAsync(openDialog.FileName);
                if (backupInfo == null)
                {
                    MessageBox.Show(
                        "ملف النسخة الاحتياطية غير صالح أو تالف",
                        "خطأ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK,
                        MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                    return;
                }

                // تأكيد الاستعادة
                var result = MessageBox.Show(
                    $"⚠️ تحذير مهم!\n\n" +
                    $"سيتم حذف جميع البيانات الحالية واستبدالها ببيانات النسخة الاحتياطية.\n\n" +
                    $"📅 تاريخ النسخة: {backupInfo.CreatedAt:yyyy/MM/dd - HH:mm}\n" +
                    $"💻 الجهاز: {backupInfo.MachineName}\n" +
                    $"👤 المستخدم: {backupInfo.CreatedBy}\n\n" +
                    $"هل أنت متأكد من المتابعة؟",
                    "تأكيد الاستعادة",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

                if (result != MessageBoxResult.Yes)
                    return;

                // تأكيد ثاني
                var finalConfirm = MessageBox.Show(
                    "⛔ هذا الإجراء لا يمكن التراجع عنه!\n\n" +
                    "اكتب 'نعم' في ذهنك وتأكد أنك تريد الاستمرار.\n\n" +
                    "هل تريد المتابعة؟",
                    "تأكيد نهائي",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Stop,
                    MessageBoxResult.No,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

                if (finalConfirm != MessageBoxResult.Yes)
                    return;

                // تنفيذ الاستعادة
                Mouse.OverrideCursor = Cursors.Wait;

                await backupService.RestoreBackupAsync(openDialog.FileName);

                Mouse.OverrideCursor = null;

                MessageBox.Show(
                    "✅ تمت استعادة النسخة الاحتياطية بنجاح!\n\n" +
                    "يُنصح بإعادة تشغيل البرنامج لضمان تحميل جميع البيانات بشكل صحيح.",
                    "تم بنجاح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    MessageBoxResult.OK,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

                // تحديث الإحصائيات
                await LoadBackupStatsAsync();
            }
        }
        catch (Exception ex)
        {
            Mouse.OverrideCursor = null;
            MessageBox.Show(
                $"حدث خطأ أثناء استعادة النسخة الاحتياطية:\n{ex.Message}",
                "خطأ",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }
    }

    private void DeleteProductCategory_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Category category)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedProductCategory = category;
                ViewModel.DeleteProductCategoryCommand.Execute(null);
            }
        }
    }

    private void DeleteExpenseCategory_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is ExpenseCategory category)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedExpenseCategory = category;
                ViewModel.DeleteExpenseCategoryCommand.Execute(null);
            }
        }
    }

    private void DeleteStore_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Store store)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedStore = store;
                ViewModel.DeleteStoreCommand.Execute(null);
            }
        }
    }

    private void ChangePassword_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null) return;

        var currentPassword = CurrentPasswordBox.Password;
        var newPassword = NewPasswordBox.Password;
        var confirmPassword = ConfirmPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            MessageBox.Show(
                "يرجى إدخال كلمة المرور الحالية",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            MessageBox.Show(
                "يرجى إدخال كلمة المرور الجديدة",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }

        if (newPassword != confirmPassword)
        {
            MessageBox.Show(
                "كلمة المرور الجديدة غير متطابقة",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }

        if (newPassword.Length < 4)
        {
            MessageBox.Show(
                "كلمة المرور يجب أن تكون 4 أحرف على الأقل",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }

        ViewModel.ChangePassword(currentPassword, newPassword);

        // Clear password boxes
        CurrentPasswordBox.Clear();
        NewPasswordBox.Clear();
        ConfirmPasswordBox.Clear();
    }
    
    #region License Methods
    
    private void LoadLicenseInfo()
    {
        try
        {
            var result = LicenseService.CheckLicense();
            
            if (result.IsValid)
            {
                if (result.Type == LicenseType.Permanent)
                {
                    // ترخيص دائمي
                    LicenseBorder.Background = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#f0fdf4"));
                    LicenseIcon.Text = "✅";
                    LicenseStatusText.Text = "ترخيص دائمي";
                    LicenseStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#166534"));
                    LicenseDetailsText.Text = "مفعّل - لا تنتهي صلاحيته";
                    LicenseDetailsText.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#22c55e"));
                    DaysCounterBorder.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // ترخيص تجريبي (أسبوعي)
                    LicenseBorder.Background = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#eff6ff"));
                    LicenseIcon.Text = "⏱️";
                    LicenseStatusText.Text = "استخدام تجريبي (أسبوعي)";
                    LicenseStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1e40af"));
                    
                    var license = LicenseService.GetCurrentLicense();
                    LicenseDetailsText.Text = $"ينتهي بتاريخ: {license.ExpirationDate:yyyy/MM/dd}";
                    LicenseDetailsText.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3b82f6"));
                    
                    // عرض العداد
                    DaysCounterBorder.Visibility = Visibility.Visible;
                    DaysCounterBorder.Background = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3b82f6"));
                    DaysCounterText.Text = result.DaysRemaining.ToString();
                }
            }
            else
            {
                // غير مفعّل أو منتهي
                LicenseBorder.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#fef2f2"));
                LicenseIcon.Text = "⚠️";
                LicenseStatusText.Text = "غير مفعّل";
                LicenseStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#dc2626"));
                LicenseDetailsText.Text = result.Message;
                LicenseDetailsText.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ef4444"));
                DaysCounterBorder.Visibility = Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading license info: {ex.Message}");
        }
    }
    
    #endregion
}
