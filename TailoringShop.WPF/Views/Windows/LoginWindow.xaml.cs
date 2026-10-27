using System.Windows;
using TailoringShop.Infrastructure.Data;
using TailoringShop.WPF.Services;
using TailoringShop.WPF.ViewModels;

namespace TailoringShop.WPF.Views.Windows;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow()
    {
        InitializeComponent();
        _viewModel = App.GetService<LoginViewModel>();
        DataContext = _viewModel;
        _viewModel.LoginSuccessful += OnLoginSuccessful;
    }

    private async void OnLoginSuccessful(Core.Entities.User user)
    {
        try
        {
            // تحميل صلاحيات المستخدم - مهم جداً!
            var context = App.GetService<AppDbContext>();
            await PermissionService.Instance.LoadPermissionsAsync(context, user.Id);
            
            // طباعة الصلاحيات للتأكد (يمكن إزالتها لاحقاً)
            PermissionService.Instance.PrintAllPermissions();
            
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل الصلاحيات: {ex.Message}", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Password = PasswordBox.Password;
    }
}
