using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TailoringShop.Core.Entities;

namespace TailoringShop.WPF.Views.Dialogs;

public partial class TailoringItemDialog : Window
{
    public TailoringOrderItem? Item { get; private set; }

    public TailoringItemDialog()
    {
        InitializeComponent();
        
        // Allow window dragging
        MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };
        
        // Focus on first field
        Loaded += (s, e) => DescriptionTextBox.Focus();
    }

    private void CalculateTotal(object sender, TextChangedEventArgs e)
    {
        // Check if UI elements are loaded
        if (QuantityTextBox == null || PriceTextBox == null || TotalText == null)
            return;

        int.TryParse(QuantityTextBox.Text, out int quantity);
        decimal.TryParse(PriceTextBox.Text?.Replace(",", "") ?? "0", out decimal price);
        
        decimal total = quantity * price;
        TotalText.Text = $"{total:N0} د.ع";
    }

    private bool ValidateForm()
    {
        bool isValid = true;

        if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
        {
            DescriptionError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            DescriptionError.Visibility = Visibility.Collapsed;
        }

        return isValid;
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm()) return;

        int.TryParse(QuantityTextBox.Text, out int quantity);
        decimal.TryParse(PriceTextBox.Text.Replace(",", ""), out decimal price);

        Item = new TailoringOrderItem
        {
            ItemName = DescriptionTextBox.Text.Trim(),
            Description = DescriptionTextBox.Text.Trim(),
            Quantity = quantity > 0 ? quantity : 1,
            UnitPrice = price,
            Total = quantity * price
        };

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            MaximizeButton.Content = "🗖";
        }
        else
        {
            WindowState = WindowState.Maximized;
            MaximizeButton.Content = "🗗";
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
