using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using TailoringShop.WPF.Services;
using TailoringShop.WPF.ViewModels;

namespace TailoringShop.WPF.Views.Windows;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.GetService<MainViewModel>();
        
        // تحميل إعدادات المحل
        LoadShopSettings();
        
        // الاشتراك في تغييرات الإعدادات
        ShopSettingsService.SettingsChanged += LoadShopSettings;
    }

    private void LoadShopSettings()
    {
        try
        {
            var settings = ShopSettingsService.LoadSettings();
            
            // تحديث اسم المحل
            ShopNameText.Text = settings.ShopName;
            
            // تحديث الحرفين الأولين للوغو الافتراضي
            if (!string.IsNullOrEmpty(settings.ShopName) && settings.ShopName.Length >= 2)
            {
                DefaultLogoText.Text = settings.ShopName.Substring(0, 2);
            }
            else if (!string.IsNullOrEmpty(settings.ShopName))
            {
                DefaultLogoText.Text = settings.ShopName;
            }
            
            // تحميل اللوغو
            var logoPath = ShopSettingsService.GetLogoPath();
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(logoPath, UriKind.Absolute);
                bitmap.EndInit();
                
                ShopLogoImage.Source = bitmap;
                ShopLogoBorder.Visibility = Visibility.Visible;
                DefaultLogoBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShopLogoBorder.Visibility = Visibility.Collapsed;
                DefaultLogoBorder.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading shop settings: {ex.Message}");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // إلغاء الاشتراك
        ShopSettingsService.SettingsChanged -= LoadShopSettings;
        base.OnClosed(e);
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "هل تريد تسجيل الخروج؟",
            "تأكيد",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

        if (result == MessageBoxResult.Yes)
        {
            App.CurrentUser = null;
            PermissionService.Instance.Clear();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
