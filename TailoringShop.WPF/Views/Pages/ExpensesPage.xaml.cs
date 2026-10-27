using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TailoringShop.Core.Entities;
using TailoringShop.WPF.Services;
using TailoringShop.WPF.ViewModels;

namespace TailoringShop.WPF.Views.Pages;

public partial class ExpensesPage : UserControl
{
    private ExpensesViewModel? ViewModel => DataContext as ExpensesViewModel;
    private object? _lastClickedItem = null;

    public ExpensesPage()
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

    private void AddExpense_Click(object sender, MouseButtonEventArgs e)
    {
        if (!PermissionService.Instance.CanCreate("expenses"))
        {
            ShowNoPermissionMessage("إضافة");
            return;
        }
        ViewModel?.AddExpenseCommand.Execute(null);
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

        if (!PermissionService.Instance.CanUpdate("expenses"))
        {
            ShowNoPermissionMessage("تعديل");
            return;
        }
        ViewModel?.EditExpenseCommand.Execute(null);
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true; // منع انتشار الحدث للصف
        
        if (!PermissionService.Instance.CanUpdate("expenses"))
        {
            ShowNoPermissionMessage("تعديل");
            return;
        }

        if (sender is Button button && button.DataContext is Expense expense)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedExpense = expense;
                ViewModel.EditExpenseCommand.Execute(null);
            }
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true; // منع انتشار الحدث للصف
        
        if (!PermissionService.Instance.CanDelete("expenses"))
        {
            ShowNoPermissionMessage("حذف");
            return;
        }

        if (sender is Button button && button.DataContext is Expense expense)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedExpense = expense;
                ViewModel.DeleteExpenseCommand.Execute(null);
            }
        }
    }

    private void ShowNoPermissionMessage(string action)
    {
        MessageBox.Show($"ليس لديك صلاحية {action} المصروفات", "غير مصرح",
            MessageBoxButton.OK, MessageBoxImage.Warning,
            MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
    }
}
