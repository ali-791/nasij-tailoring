using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using TailoringShop.Core.Entities;
using TailoringShop.Core.Enums;
using TailoringShop.Infrastructure.Data;
using TailoringShop.WPF.Services;
using TailoringShop.WPF.Views.Pages;

namespace TailoringShop.WPF.Views.Dialogs;

public partial class TailoringOrderDialog : Window
{
    public TailoringOrder? Order { get; private set; }
    private readonly AppDbContext _context;
    private readonly bool _isEditMode;
    private Currency? _defaultCurrency;
    private ObservableCollection<TailoringOrderItem> _items = new();

    public TailoringOrderDialog(AppDbContext context, TailoringOrder? order = null)
    {
        InitializeComponent();
        
        _context = context;
        _isEditMode = order != null;
        
        LoadData();
        
        if (_isEditMode && order != null)
        {
            TitleText.Text = "تعديل طلب الفصال";
            Order = order;
            LoadOrderData();
        }
        else
        {
            TitleText.Text = "طلب فصال جديد";
            Order = new TailoringOrder();
            DeliveryDatePicker.SelectedDate = DateTime.Today.AddDays(7);
        }

        ItemsListBox.ItemsSource = _items;

        // Allow window dragging
        MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };
    }

    private void LoadData()
    {
        // Get default currency
        _defaultCurrency = _context.Currencies.FirstOrDefault(c => c.IsDefault);
    }

    private void LoadOrderData()
    {
        if (Order == null) return;
        
        // Load customer info
        var customer = _context.Customers.FirstOrDefault(c => c.Id == Order.CustomerId);
        if (customer != null)
        {
            CustomerNameTextBox.Text = customer.FullName;
            CustomerPhoneTextBox.Text = customer.Phone;
        }

        DeliveryDatePicker.SelectedDate = Order.DeliveryDate;
        DepositTextBox.Text = Order.DepositAmount.ToString("N0");
        NotesTextBox.Text = Order.Notes;

        // Load items
        if (Order.Items != null)
        {
            foreach (var item in Order.Items)
            {
                _items.Add(item);
            }
        }

        UpdateTotals();
    }

    private void AddItemButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new TailoringItemDialog();
        if (dialog.ShowDialog() == true && dialog.Item != null)
        {
            _items.Add(dialog.Item);
            UpdateTotals();
        }
    }

    private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is TailoringOrderItem item)
        {
            _items.Remove(item);
            UpdateTotals();
        }
    }

    private void DepositTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateTotals();
    }

    private void UpdateTotals()
    {
        // Check if UI elements are loaded
        if (TotalText == null || DepositText == null || RemainingText == null)
            return;

        decimal total = _items.Sum(i => i.Total);
        decimal.TryParse(DepositTextBox?.Text?.Replace(",", "") ?? "0", out decimal deposit);
        decimal remaining = total - deposit;

        TotalText.Text = $"{total:N0} د.ع";
        DepositText.Text = $"{deposit:N0} د.ع";
        RemainingText.Text = $"{remaining:N0} د.ع";
    }

    private bool ValidateForm()
    {
        bool isValid = true;

        // Validate Customer Name
        if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
        {
            CustomerNameError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            CustomerNameError.Visibility = Visibility.Collapsed;
        }

        // Validate Items
        if (_items.Count == 0)
        {
            MessageBox.Show("يرجى إضافة بند واحد على الأقل", "تنبيه", 
                MessageBoxButton.OK, MessageBoxImage.Warning,
                MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// البحث عن العميل أو إنشاء عميل جديد
    /// </summary>
    private Customer GetOrCreateCustomer(string name, string? phone)
    {
        name = name.Trim();
        phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();

        // البحث عن العميل بالاسم أو رقم الهاتف
        Customer? existingCustomer = null;

        if (!string.IsNullOrEmpty(phone))
        {
            // البحث بالهاتف أولاً (أكثر دقة)
            existingCustomer = _context.Customers
                .FirstOrDefault(c => c.Phone == phone);
        }

        if (existingCustomer == null)
        {
            // البحث بالاسم
            existingCustomer = _context.Customers
                .FirstOrDefault(c => c.FullName.ToLower() == name.ToLower());
        }

        if (existingCustomer != null)
        {
            // تحديث رقم الهاتف إذا كان فارغاً وتم إدخال رقم جديد
            if (string.IsNullOrEmpty(existingCustomer.Phone) && !string.IsNullOrEmpty(phone))
            {
                existingCustomer.Phone = phone;
            }
            return existingCustomer;
        }

        // إنشاء عميل جديد
        var newCustomer = new Customer
        {
            FullName = name,
            Phone = phone,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _context.Customers.Add(newCustomer);
        _context.SaveChanges();

        return newCustomer;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm()) return;

        if (Order == null) Order = new TailoringOrder();

        // الحصول على العميل أو إنشاء جديد
        var customerName = CustomerNameTextBox.Text.Trim();
        var customerPhone = CustomerPhoneTextBox.Text?.Trim();
        var customer = GetOrCreateCustomer(customerName, customerPhone);
        
        Order.CustomerId = customer.Id;
        Order.DeliveryDate = DeliveryDatePicker.SelectedDate ?? DateTime.Today.AddDays(7);
        Order.Notes = NotesTextBox.Text;

        // Calculate totals
        decimal total = _items.Sum(i => i.Total);
        decimal.TryParse(DepositTextBox.Text.Replace(",", ""), out decimal deposit);

        Order.TotalAmount = total;
        Order.DepositAmount = deposit;
        Order.RemainingAmount = total - deposit;

        // Set payment status
        if (deposit >= total)
            Order.PaymentStatus = PaymentStatus.Paid;
        else if (deposit > 0)
            Order.PaymentStatus = PaymentStatus.Partial;
        else
            Order.PaymentStatus = PaymentStatus.Unpaid;

        // Set order status
        if (!_isEditMode)
        {
            Order.Status = OrderStatus.New;
            Order.OrderNumber = GenerateOrderNumber();
        }

        // Set currency
        if (_defaultCurrency != null)
        {
            Order.CurrencyId = _defaultCurrency.Id;
        }

        // Get default cashbox
        var defaultCashbox = _context.Cashboxes.FirstOrDefault(c => c.Type == CashboxType.Tailoring);
        if (defaultCashbox != null)
        {
            Order.CashboxId = defaultCashbox.Id;
        }

        // Set items
        Order.Items = _items.ToList();

        DialogResult = true;
        Close();
    }

    private string GenerateOrderNumber()
    {
        var today = DateTime.Today;
        var prefix = $"TLR-{today:yyyyMMdd}-";
        
        var lastOrder = _context.TailoringOrders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefault();

        int sequence = 1;
        if (lastOrder != null && lastOrder.OrderNumber.Length > prefix.Length)
        {
            var lastSequence = lastOrder.OrderNumber.Substring(prefix.Length);
            if (int.TryParse(lastSequence, out int lastNum))
            {
                sequence = lastNum + 1;
            }
        }

        return $"{prefix}{sequence:D3}";
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    /// <summary>
    /// حفظ الطلب وطباعته
    /// </summary>
    private void SaveAndPrintButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm()) return;

        // التحقق من وجود طابعة
        if (!PrintService.HasPrinter())
        {
            var result = MessageBox.Show(
                "لا توجد طابعة متصلة. هل تريد حفظ الطلب فقط؟",
                "تنبيه",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.Yes,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            if (result == MessageBoxResult.Yes)
            {
                SaveButton_Click(sender, e);
            }
            return;
        }

        if (Order == null) Order = new TailoringOrder();

        // الحصول على العميل أو إنشاء جديد
        var customerName = CustomerNameTextBox.Text.Trim();
        var customerPhone = CustomerPhoneTextBox.Text?.Trim();
        var customer = GetOrCreateCustomer(customerName, customerPhone);
        
        Order.CustomerId = customer.Id;
        Order.Customer = customer; // للطباعة
        Order.DeliveryDate = DeliveryDatePicker.SelectedDate ?? DateTime.Today.AddDays(7);
        Order.ExpectedDeliveryDate = Order.DeliveryDate;
        Order.OrderDate = DateTime.Now;
        Order.Notes = NotesTextBox.Text;

        // Calculate totals
        decimal total = _items.Sum(i => i.Total);
        decimal.TryParse(DepositTextBox.Text.Replace(",", ""), out decimal deposit);

        Order.TotalAmount = total;
        Order.DepositAmount = deposit;
        Order.RemainingAmount = total - deposit;

        // Set payment status
        if (deposit >= total)
            Order.PaymentStatus = PaymentStatus.Paid;
        else if (deposit > 0)
            Order.PaymentStatus = PaymentStatus.Partial;
        else
            Order.PaymentStatus = PaymentStatus.Unpaid;

        // Set order status
        if (!_isEditMode)
        {
            Order.Status = OrderStatus.New;
            Order.OrderNumber = GenerateOrderNumber();
        }

        // Set currency
        if (_defaultCurrency != null)
        {
            Order.CurrencyId = _defaultCurrency.Id;
        }

        // Get default cashbox
        var defaultCashbox = _context.Cashboxes.FirstOrDefault(c => c.Type == CashboxType.Tailoring);
        if (defaultCashbox != null)
        {
            Order.CashboxId = defaultCashbox.Id;
        }

        // Set items
        Order.Items = _items.ToList();

        // الطباعة
        try
        {
            var settings = InvoiceSettingsPage.GetCurrentSettings();
            var printService = new PrintService(settings);
            
            var printed = printService.PrintTailoringOrder(Order, showDialog: false);
            
            if (printed)
            {
                MessageBox.Show(
                    "تم حفظ الطلب وطباعته بنجاح ✅",
                    "تم",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    MessageBoxResult.OK,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"تم حفظ الطلب لكن حدث خطأ في الطباعة:\n{ex.Message}",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }

        DialogResult = true;
        Close();
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            WindowState = WindowState.Normal;
        else
            WindowState = WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
