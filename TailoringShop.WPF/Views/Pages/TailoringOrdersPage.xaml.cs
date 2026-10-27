using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TailoringShop.Core.Entities;
using TailoringShop.WPF.Services;
using TailoringShop.WPF.ViewModels;

namespace TailoringShop.WPF.Views.Pages;

public partial class TailoringOrdersPage : UserControl
{
    private TailoringOrdersViewModel? ViewModel => DataContext as TailoringOrdersViewModel;
    private object? _lastClickedItem = null;

    public TailoringOrdersPage()
    {
        InitializeComponent();
    }

    private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGrid dataGrid)
        {
            // التحقق إذا كان الضغط على زر - لا نفتح Detail Panel
            var button = FindParent<Button>(e.OriginalSource as DependencyObject);
            if (button != null)
            {
                return; // الزر سيتعامل مع الحدث بنفسه
            }

            var row = FindParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row != null)
            {
                var clickedItem = row.Item;
                if (clickedItem == dataGrid.SelectedItem && clickedItem == _lastClickedItem)
                {
                    dataGrid.SelectedItem = null;
                    _lastClickedItem = null;
                    e.Handled = true;
                }
                else
                {
                    _lastClickedItem = clickedItem;
                }
            }
        }
    }

    private static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child != null)
        {
            if (child is T parent)
                return parent;
            child = System.Windows.Media.VisualTreeHelper.GetParent(child);
        }
        return null;
    }

    private void AddOrder_Click(object sender, MouseButtonEventArgs e)
    {
        if (!PermissionService.Instance.CanCreate("tailoring_orders"))
        {
            ShowNoPermissionMessage("إضافة");
            return;
        }
        ViewModel?.AddOrderCommand.Execute(null);
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // التحقق إذا كان الضغط على زر
        var button = FindParent<Button>(e.OriginalSource as DependencyObject);
        if (button != null)
        {
            e.Handled = true;
            return;
        }

        if (!PermissionService.Instance.CanUpdate("tailoring_orders"))
        {
            ShowNoPermissionMessage("تعديل");
            return;
        }
        ViewModel?.EditOrderCommand.Execute(null);
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true; // منع انتشار الحدث للصف
        
        if (!PermissionService.Instance.CanUpdate("tailoring_orders"))
        {
            ShowNoPermissionMessage("تعديل");
            return;
        }

        if (sender is Button button && button.DataContext is TailoringOrder order)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedOrder = order;
                ViewModel.EditOrderCommand.Execute(null);
            }
        }
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true; // منع انتشار الحدث للصف
        
        if (!PermissionService.Instance.CanUpdate("tailoring_orders"))
        {
            ShowNoPermissionMessage("تحديث حالة");
            return;
        }

        if (sender is Button button && button.DataContext is TailoringOrder order)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedOrder = order;
                ViewModel.ChangeStatusCommand.Execute("قيد التنفيذ");
            }
        }
    }

    private void ReadyButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true; // منع انتشار الحدث للصف
        
        if (!PermissionService.Instance.CanUpdate("tailoring_orders"))
        {
            ShowNoPermissionMessage("تحديث حالة");
            return;
        }

        if (sender is Button button && button.DataContext is TailoringOrder order)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedOrder = order;
                ViewModel.ChangeStatusCommand.Execute("جاهز");
            }
        }
    }

    private void DeliverButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true; // منع انتشار الحدث للصف
        
        if (!PermissionService.Instance.CanUpdate("tailoring_orders"))
        {
            ShowNoPermissionMessage("تسليم");
            return;
        }

        if (sender is Button button && button.DataContext is TailoringOrder order)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedOrder = order;
                ViewModel.ChangeStatusCommand.Execute("تم التسليم");
            }
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true; // منع انتشار الحدث للصف
        
        if (!PermissionService.Instance.CanDelete("tailoring_orders"))
        {
            ShowNoPermissionMessage("إلغاء");
            return;
        }

        if (sender is Button button && button.DataContext is TailoringOrder order)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedOrder = order;
                ViewModel.ChangeStatusCommand.Execute("ملغي");
            }
        }
    }

    private void ShowNoPermissionMessage(string action)
    {
        MessageBox.Show($"ليس لديك صلاحية {action} الطلبات", "غير مصرح",
            MessageBoxButton.OK, MessageBoxImage.Warning,
            MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
    }
}
