using System.Windows;
using System.Windows.Input;
using TailoringShop.Core.Entities;

namespace TailoringShop.WPF.Views.Dialogs;

public partial class CustomerDialog : Window
{
    public Customer? Customer { get; private set; }
    private readonly bool _isEditMode;

    public CustomerDialog(Customer? customer = null)
    {
        InitializeComponent();
        
        _isEditMode = customer != null;
        
        if (_isEditMode && customer != null)
        {
            TitleText.Text = "تعديل بيانات العميل";
            Customer = customer;
            LoadCustomerData();
        }
        else
        {
            TitleText.Text = "إضافة عميل جديد";
            Customer = new Customer();
        }

        // Allow window dragging
        MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };
        
        // Focus on first field
        Loaded += (s, e) => FullNameTextBox.Focus();
    }

    private void LoadCustomerData()
    {
        if (Customer == null) return;
        
        FullNameTextBox.Text = Customer.FullName;
        PhoneTextBox.Text = Customer.Phone;
        IsActiveCheckBox.IsChecked = Customer.IsActive;
    }

    private bool ValidateForm()
    {
        bool isValid = true;

        // Validate Full Name
        if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
        {
            FullNameError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            FullNameError.Visibility = Visibility.Collapsed;
        }

        // Validate Phone
        if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
        {
            PhoneError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            PhoneError.Visibility = Visibility.Collapsed;
        }

        return isValid;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm()) return;

        if (Customer == null) Customer = new Customer();

        Customer.FullName = FullNameTextBox.Text.Trim();
        Customer.Phone = PhoneTextBox.Text.Trim();
        Customer.IsActive = IsActiveCheckBox.IsChecked ?? true;

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
