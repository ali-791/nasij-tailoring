using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TailoringShop.Core.Entities;
using TailoringShop.WPF.Services;
using TailoringShop.WPF.ViewModels;

namespace TailoringShop.WPF.Views.Pages;

public partial class CashboxesPage : UserControl
{
    private CashboxesViewModel? ViewModel => DataContext as CashboxesViewModel;

    public CashboxesPage()
    {
        InitializeComponent();
    }

    private void AddCashbox_Click(object sender, MouseButtonEventArgs e)
    {
        if (!PermissionService.Instance.CanCreate("cashboxes"))
        {
            ShowNoPermissionMessage("إضافة");
            return;
        }
        ViewModel?.AddCashboxCommand.Execute(null);
    }

    private void CashboxCard_Click(object sender, MouseButtonEventArgs e)
    {
        // View cashbox details
    }

    private void EditCashbox_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.CanUpdate("cashboxes"))
        {
            ShowNoPermissionMessage("تعديل");
            return;
        }

        if (sender is Button button && button.DataContext is Cashbox cashbox)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedCashbox = cashbox;
                ViewModel.EditCashboxCommand.Execute(null);
            }
        }
    }

    private void DeleteCashbox_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.CanDelete("cashboxes"))
        {
            ShowNoPermissionMessage("حذف");
            return;
        }

        if (sender is Button button && button.DataContext is Cashbox cashbox)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedCashbox = cashbox;
                ViewModel.DeleteCashboxCommand.Execute(null);
            }
        }
    }

    private void Deposit_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.CanCreate("cashboxes"))
        {
            ShowNoPermissionMessage("الإيداع");
            return;
        }

        if (sender is Button button && button.DataContext is Cashbox cashbox)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedCashbox = cashbox;
                ViewModel.DepositCommand.Execute(null);
            }
        }
    }

    private void Withdraw_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.CanCreate("cashboxes"))
        {
            ShowNoPermissionMessage("السحب");
            return;
        }

        if (sender is Button button && button.DataContext is Cashbox cashbox)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedCashbox = cashbox;
                ViewModel.WithdrawCommand.Execute(null);
            }
        }
    }

    private void ShowNoPermissionMessage(string action)
    {
        MessageBox.Show($"ليس لديك صلاحية {action}", "غير مصرح",
            MessageBoxButton.OK, MessageBoxImage.Warning,
            MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
    }
}
