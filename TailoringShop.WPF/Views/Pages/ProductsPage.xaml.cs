using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TailoringShop.Core.Entities;
using TailoringShop.WPF.Services;
using TailoringShop.WPF.ViewModels;

namespace TailoringShop.WPF.Views.Pages;

public partial class ProductsPage : UserControl
{
    private ProductsViewModel? ViewModel => DataContext as ProductsViewModel;
    private object? _lastClickedItem = null;

    public ProductsPage()
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

    private void AddPurchaseInvoice_Click(object sender, MouseButtonEventArgs e)
    {
        if (!PermissionService.Instance.CanCreate("invoices"))
        {
            ShowNoPermissionMessage("إضافة فاتورة شراء");
            return;
        }
        ViewModel?.AddPurchaseInvoiceCommand?.Execute(null);
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

        if (!PermissionService.Instance.CanUpdate("products"))
        {
            ShowNoPermissionMessage("تعديل");
            return;
        }
        ViewModel?.EditProductCommand.Execute(null);
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true; // منع انتشار الحدث للصف
        
        if (!PermissionService.Instance.CanUpdate("products"))
        {
            ShowNoPermissionMessage("تعديل");
            return;
        }

        if (sender is Button button && button.DataContext is Product product)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedProduct = product;
                ViewModel.EditProductCommand.Execute(null);
            }
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true; // منع انتشار الحدث للصف
        
        if (!PermissionService.Instance.CanDelete("products"))
        {
            ShowNoPermissionMessage("حذف");
            return;
        }

        if (sender is Button button && button.DataContext is Product product)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedProduct = product;
                ViewModel.DeleteProductCommand.Execute(null);
            }
        }
    }

    private void ShowNoPermissionMessage(string action)
    {
        MessageBox.Show($"ليس لديك صلاحية {action} المنتجات", "غير مصرح",
            MessageBoxButton.OK, MessageBoxImage.Warning,
            MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
    }
}
