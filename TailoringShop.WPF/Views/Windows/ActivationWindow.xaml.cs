using System.Windows;
using TailoringShop.WPF.Services;

namespace TailoringShop.WPF.Views.Windows;

public partial class ActivationWindow : Window
{
    public bool IsActivated { get; private set; } = false;
    
    public ActivationWindow()
    {
        InitializeComponent();
        
        // التحقق من الترخيص الحالي
        var result = LicenseService.CheckLicense();
        if (result.IsValid)
        {
            IsActivated = true;
            DialogResult = true;
            Close();
        }
    }
    
    private void Activate_Click(object sender, RoutedEventArgs e)
    {
        var key = LicenseKeyInput.Text.Trim();
        
        if (string.IsNullOrEmpty(key))
        {
            MessageBox.Show(
                "يرجى إدخال مفتاح الترخيص",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }
        
        var result = LicenseService.ActivateLicense(key);
        
        if (result.IsValid)
        {
            MessageBox.Show(
                result.Message,
                "تم التفعيل بنجاح",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            
            IsActivated = true;
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show(
                result.Message,
                "خطأ في التفعيل",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
