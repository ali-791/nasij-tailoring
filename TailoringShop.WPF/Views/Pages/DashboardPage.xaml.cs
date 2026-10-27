using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TailoringShop.WPF.Services;
using TailoringShop.WPF.ViewModels;
using TailoringShop.WPF.Views.Dialogs;

namespace TailoringShop.WPF.Views.Pages;

public partial class DashboardPage : UserControl
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    private MainViewModel? GetMainViewModel()
    {
        var mainWindow = Window.GetWindow(this);
        return mainWindow?.DataContext as MainViewModel;
    }

    private void NewSaleInvoice_Click(object sender, MouseButtonEventArgs e)
    {
        if (!PermissionService.Instance.CanCreate("invoices"))
        {
            ShowNoPermissionMessage("إضافة فاتورة بيع");
            return;
        }

        var mainViewModel = GetMainViewModel();
        if (mainViewModel != null)
        {
            // التنقل لصفحة الفواتير
            mainViewModel.NavigateToInvoicesCommand.Execute(null);
            
            // فتح dialog فاتورة بيع جديدة بعد تأخير بسيط للتأكد من تحميل الصفحة
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var invoicesViewModel = mainViewModel.CurrentPage as InvoicesViewModel;
                invoicesViewModel?.AddInvoiceCommand.Execute(null);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private void NewPurchaseInvoice_Click(object sender, MouseButtonEventArgs e)
    {
        if (!PermissionService.Instance.CanCreate("invoices"))
        {
            ShowNoPermissionMessage("إضافة فاتورة شراء");
            return;
        }

        var mainViewModel = GetMainViewModel();
        if (mainViewModel != null)
        {
            // التنقل لصفحة المنتجات (حيث توجد فاتورة الشراء)
            mainViewModel.NavigateToProductsCommand.Execute(null);
            
            // فتح dialog فاتورة شراء جديدة
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var productsViewModel = mainViewModel.CurrentPage as ProductsViewModel;
                productsViewModel?.AddPurchaseInvoiceCommand?.Execute(null);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private void NewTailoringOrder_Click(object sender, MouseButtonEventArgs e)
    {
        if (!PermissionService.Instance.CanCreate("tailoring_orders"))
        {
            ShowNoPermissionMessage("إضافة طلب فصال");
            return;
        }

        var mainViewModel = GetMainViewModel();
        if (mainViewModel != null)
        {
            // التنقل لصفحة طلبات الفصال
            mainViewModel.NavigateToTailoringOrdersCommand.Execute(null);
            
            // فتح dialog طلب فصال جديد
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var ordersViewModel = mainViewModel.CurrentPage as TailoringOrdersViewModel;
                ordersViewModel?.AddOrderCommand.Execute(null);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private void ShowNoPermissionMessage(string action)
    {
        MessageBox.Show($"ليس لديك صلاحية {action}", "غير مصرح",
            MessageBoxButton.OK, MessageBoxImage.Warning,
            MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
    }
}
