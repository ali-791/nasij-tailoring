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

// Helper class for display
public class InvoiceItemDisplay
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal Total { get; set; }
    public long? ProductCashboxId { get; set; } // صندوق المنتج الأصلي
}

public partial class InvoiceDialog : Window
{
    public Invoice? Invoice { get; private set; }
    private readonly AppDbContext _context;
    private readonly bool _isEditMode;
    private Currency? _defaultCurrency;
    private Store? _defaultStore;
    private ObservableCollection<InvoiceItemDisplay> _items = new();
    private bool _isInitialized = false;
    private List<Product> _allProducts = new();

    public InvoiceDialog(AppDbContext context, Invoice? invoice = null)
    {
        InitializeComponent();
        
        _context = context;
        _isEditMode = invoice != null;

        LoadData();
        
        if (_isEditMode && invoice != null)
        {
            TitleText.Text = "تعديل فاتورة بيع";
            Invoice = invoice;
            LoadInvoiceData();
        }
        else
        {
            TitleText.Text = "فاتورة بيع جديدة";
            Invoice = new Invoice { Type = InvoiceType.Sale };
        }

        InvoiceDatePicker.SelectedDate = DateTime.Today;
        ItemsDataGrid.ItemsSource = _items;
        _isInitialized = true;

        // Allow window dragging
        MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };
        
        // Initial calculation
        Loaded += (s, e) => UpdateTotals();
    }

    private void LoadData()
    {
        // Load customers
        var customers = _context.Customers.Where(c => c.IsActive).ToList();
        CustomerComboBox.ItemsSource = customers;
        
        // تعيين العميل الافتراضي (أول عميل)
        if (customers.Any() && !_isEditMode)
        {
            CustomerComboBox.SelectedIndex = 0;
        }

        // Load cashboxes
        var cashboxes = _context.Cashboxes
            .Include(c => c.Currency)
            .Where(c => c.IsActive)
            .ToList();
        CashboxComboBox.ItemsSource = cashboxes;
        
        // اختيار الصندوق العام كافتراضي
        var defaultCashbox = cashboxes.FirstOrDefault(c => c.Type == CashboxType.General);
        if (defaultCashbox != null)
            CashboxComboBox.SelectedItem = defaultCashbox;
        else if (cashboxes.Any())
            CashboxComboBox.SelectedIndex = 0;

        // Load all products with cashbox
        _allProducts = _context.Products
            .Include(p => p.Inventories)
            .Include(p => p.Cashbox)
            .Where(p => p.IsActive)
            .ToList();
        
        // Filter products by selected cashbox initially
        FilterProductsByCashbox();

        // Get defaults
        _defaultCurrency = _context.Currencies.FirstOrDefault(c => c.IsDefault);
        _defaultStore = _context.Stores.FirstOrDefault(s => s.IsDefault);
    }

    /// <summary>
    /// عند تغيير الصندوق المختار - نعيد تصفية المنتجات
    /// </summary>
    private void CashboxComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;
        
        // مسح المنتجات المضافة إذا تم تغيير الصندوق
        if (_items.Count > 0)
        {
            var result = MessageBox.Show(
                "تغيير الصندوق سيؤدي إلى مسح المنتجات المضافة. هل تريد المتابعة؟",
                "تأكيد",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            
            if (result == MessageBoxResult.No)
            {
                // إعادة الصندوق السابق
                e.Handled = true;
                if (e.RemovedItems.Count > 0)
                    CashboxComboBox.SelectedItem = e.RemovedItems[0];
                return;
            }
            
            _items.Clear();
            UpdateTotals();
        }
        
        FilterProductsByCashbox();
    }

    /// <summary>
    /// تصفية المنتجات حسب الصندوق المختار
    /// </summary>
    private void FilterProductsByCashbox()
    {
        var selectedCashbox = CashboxComboBox.SelectedItem as Cashbox;
        
        if (selectedCashbox == null)
        {
            // إذا لم يتم اختيار صندوق، نعرض كل المنتجات
            ProductComboBox.ItemsSource = _allProducts;
        }
        else
        {
            // تصفية المنتجات التي تنتمي لهذا الصندوق أو بدون صندوق
            var filteredProducts = _allProducts
                .Where(p => p.CashboxId == null || p.CashboxId == selectedCashbox.Id)
                .ToList();
            
            ProductComboBox.ItemsSource = filteredProducts;
        }
        
        ProductComboBox.SelectedIndex = -1;
    }

    private void LoadInvoiceData()
    {
        if (Invoice == null) return;

        // Set customer
        if (Invoice.CustomerId.HasValue)
        {
            foreach (var item in CustomerComboBox.Items)
            {
                if (item is Customer customer && customer.Id == Invoice.CustomerId)
                {
                    CustomerComboBox.SelectedItem = customer;
                    break;
                }
            }
        }

        // Set cashbox
        foreach (var item in CashboxComboBox.Items)
        {
            if (item is Cashbox cashbox && cashbox.Id == Invoice.CashboxId)
            {
                CashboxComboBox.SelectedItem = cashbox;
                break;
            }
        }

        InvoiceDatePicker.SelectedDate = Invoice.InvoiceDate;
        DiscountTextBox.Text = Invoice.DiscountValue.ToString("N0");
        PaidAmountTextBox.Text = Invoice.PaidAmount.ToString("N0");
        NotesTextBox.Text = Invoice.Notes ?? "";

        // Load items
        if (Invoice.Items != null)
        {
            foreach (var item in Invoice.Items)
            {
                _items.Add(new InvoiceItemDisplay
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? "منتج محذوف",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    CostPrice = item.CostPrice,
                    Total = item.Total,
                    ProductCashboxId = item.Product?.CashboxId
                });
            }
        }

        UpdateTotals();
    }

    private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ProductComboBox.SelectedItem is Product product)
        {
            PriceTextBox.Text = product.SellingPrice.ToString("N0");
        }
    }

    private void AddProductButton_Click(object sender, RoutedEventArgs e)
    {
        // التحقق من اختيار صندوق أولاً
        var selectedCashbox = CashboxComboBox.SelectedItem as Cashbox;
        if (selectedCashbox == null)
        {
            MessageBox.Show("يرجى اختيار الصندوق أولاً", "تنبيه",
                MessageBoxButton.OK, MessageBoxImage.Warning,
                MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            CashboxComboBox.Focus();
            return;
        }

        if (ProductComboBox.SelectedItem is not Product product)
        {
            MessageBox.Show("يرجى اختيار منتج", "تنبيه",
                MessageBoxButton.OK, MessageBoxImage.Warning,
                MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }

        // التحقق من أن المنتج ينتمي للصندوق المختار
        if (product.CashboxId != null && product.CashboxId != selectedCashbox.Id)
        {
            var productCashbox = _context.Cashboxes.Find(product.CashboxId);
            MessageBox.Show(
                $"لا يمكن إضافة هذا المنتج!\n\nالمنتج \"{product.Name}\" ينتمي إلى صندوق \"{productCashbox?.Name ?? "آخر"}\".\n\nيرجى اختيار الصندوق الصحيح أو اختيار منتج آخر.",
                "خطأ - صندوق غير متطابق",
                MessageBoxButton.OK, MessageBoxImage.Error,
                MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }

        decimal.TryParse(QuantityTextBox.Text?.Replace(",", "") ?? "1", out decimal quantity);
        decimal.TryParse(PriceTextBox.Text?.Replace(",", "") ?? "0", out decimal price);

        if (quantity <= 0) quantity = 1;

        // Check if product already in list
        var existing = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing != null)
        {
            existing.Quantity += quantity;
            existing.Total = existing.Quantity * existing.UnitPrice;
            ItemsDataGrid.Items.Refresh();
        }
        else
        {
            _items.Add(new InvoiceItemDisplay
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = quantity,
                UnitPrice = price,
                CostPrice = product.CostPrice,
                Total = quantity * price,
                ProductCashboxId = product.CashboxId
            });
        }

        // Reset inputs
        ProductComboBox.SelectedIndex = -1;
        QuantityTextBox.Text = "1";
        PriceTextBox.Text = "0";

        UpdateTotals();
    }

    private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is InvoiceItemDisplay item)
        {
            _items.Remove(item);
            UpdateTotals();
        }
    }

    private void DiscountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isInitialized) UpdateTotals();
    }

    private void DiscountTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitialized) UpdateTotals();
    }

    private void PaidAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isInitialized) 
        {
            _userChangedPaidAmount = true;
            UpdateTotals();
        }
    }

    private bool _userChangedPaidAmount = false;

    private void UpdateTotals()
    {
        // Null checks
        if (SubtotalText == null || TotalText == null || RemainingText == null || ItemsCountText == null)
            return;

        decimal subtotal = _items.Sum(i => i.Total);
        
        decimal.TryParse(DiscountTextBox?.Text?.Replace(",", "") ?? "0", out decimal discountValue);
        bool isPercentage = DiscountTypeComboBox?.SelectedIndex == 1;
        
        decimal discountAmount = isPercentage ? (subtotal * discountValue / 100) : discountValue;
        decimal total = subtotal - discountAmount;
        if (total < 0) total = 0;

        // تحديث المبلغ المدفوع ليساوي الإجمالي افتراضياً (إذا لم يغيره المستخدم)
        if (!_userChangedPaidAmount && !_isEditMode && PaidAmountTextBox != null)
        {
            PaidAmountTextBox.Text = total.ToString("N0");
        }

        decimal.TryParse(PaidAmountTextBox?.Text?.Replace(",", "") ?? "0", out decimal paidAmount);
        decimal remaining = total - paidAmount;

        SubtotalText.Text = $"{subtotal:N0} د.ع";
        TotalText.Text = $"{total:N0} د.ع";
        RemainingText.Text = $"{remaining:N0} د.ع";
        ItemsCountText.Text = $"{_items.Count} منتج";
    }

    private bool ValidateForm()
    {
        // التحقق من اختيار صندوق
        if (CashboxComboBox.SelectedItem == null)
        {
            MessageBox.Show("يرجى اختيار الصندوق", "تنبيه",
                MessageBoxButton.OK, MessageBoxImage.Warning,
                MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return false;
        }

        if (_items.Count == 0)
        {
            MessageBox.Show("يرجى إضافة منتج واحد على الأقل", "تنبيه",
                MessageBoxButton.OK, MessageBoxImage.Warning,
                MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return false;
        }

        return true;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm()) return;

        if (Invoice == null) Invoice = new Invoice();

        var selectedCashbox = CashboxComboBox.SelectedItem as Cashbox;

        // Set basic info
        Invoice.Type = InvoiceType.Sale;
        Invoice.InvoiceDate = InvoiceDatePicker.SelectedDate ?? DateTime.Today;
        Invoice.Notes = NotesTextBox.Text;

        // Set customer
        var selectedCustomer = CustomerComboBox.SelectedItem as Customer;
        Invoice.CustomerId = selectedCustomer?.Id;

        // Calculate totals
        decimal subtotal = _items.Sum(i => i.Total);
        decimal.TryParse(DiscountTextBox?.Text?.Replace(",", "") ?? "0", out decimal discountValue);
        bool isPercentage = DiscountTypeComboBox?.SelectedIndex == 1;
        decimal discountAmount = isPercentage ? (subtotal * discountValue / 100) : discountValue;
        decimal total = subtotal - discountAmount;
        if (total < 0) total = 0;

        decimal.TryParse(PaidAmountTextBox?.Text?.Replace(",", "") ?? "0", out decimal paidAmount);
        decimal remaining = total - paidAmount;

        Invoice.Subtotal = subtotal;
        Invoice.DiscountType = isPercentage ? DiscountType.Percentage : DiscountType.Fixed;
        Invoice.DiscountValue = discountValue;
        Invoice.DiscountAmount = discountAmount;
        Invoice.Total = total;
        Invoice.PaidAmount = paidAmount;
        Invoice.RemainingAmount = remaining;

        // Set payment status
        if (paidAmount >= total)
            Invoice.PaymentStatus = PaymentStatus.Paid;
        else if (paidAmount > 0)
            Invoice.PaymentStatus = PaymentStatus.Partial;
        else
            Invoice.PaymentStatus = PaymentStatus.Unpaid;

        // Set status
        Invoice.Status = InvoiceStatus.Confirmed;

        // Generate invoice number if new
        if (!_isEditMode)
        {
            Invoice.InvoiceNumber = GenerateInvoiceNumber();
        }

        // Set defaults
        if (_defaultCurrency != null) Invoice.CurrencyId = _defaultCurrency.Id;
        if (selectedCashbox != null) Invoice.CashboxId = selectedCashbox.Id;
        if (_defaultStore != null) Invoice.StoreId = _defaultStore.Id;

        // Set items
        Invoice.Items = _items.Select(i => new InvoiceItem
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            CostPrice = i.CostPrice,
            Subtotal = i.Total,
            Total = i.Total,
            Profit = (i.UnitPrice - i.CostPrice) * i.Quantity
        }).ToList();

        DialogResult = true;
        Close();
    }

    private string GenerateInvoiceNumber()
    {
        var today = DateTime.Today;
        var prefix = $"INV-{today:yyyyMMdd}-";
        
        var lastInvoice = _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefault();

        int sequence = 1;
        if (lastInvoice != null && lastInvoice.InvoiceNumber.Length > prefix.Length)
        {
            var lastSequence = lastInvoice.InvoiceNumber.Substring(prefix.Length);
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
    /// حفظ الفاتورة وطباعتها
    /// </summary>
    private void SaveAndPrintButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm()) return;

        // التحقق من وجود طابعة
        if (!PrintService.HasPrinter())
        {
            var result = MessageBox.Show(
                "لا توجد طابعة متصلة. هل تريد حفظ الفاتورة فقط؟",
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

        // حفظ الفاتورة أولاً (بدون إغلاق النافذة)
        if (Invoice == null) Invoice = new Invoice();

        var selectedCashbox = CashboxComboBox.SelectedItem as Cashbox;

        // Set basic info
        Invoice.Type = InvoiceType.Sale;
        Invoice.InvoiceDate = InvoiceDatePicker.SelectedDate ?? DateTime.Today;
        Invoice.Notes = NotesTextBox.Text;

        // Set customer
        var selectedCustomer = CustomerComboBox.SelectedItem as Customer;
        Invoice.CustomerId = selectedCustomer?.Id;
        Invoice.Customer = selectedCustomer; // للطباعة

        // Calculate totals
        decimal subtotal = _items.Sum(i => i.Total);
        decimal.TryParse(DiscountTextBox?.Text?.Replace(",", "") ?? "0", out decimal discountValue);
        bool isPercentage = DiscountTypeComboBox?.SelectedIndex == 1;
        decimal discountAmount = isPercentage ? (subtotal * discountValue / 100) : discountValue;
        decimal total = subtotal - discountAmount;
        if (total < 0) total = 0;

        decimal.TryParse(PaidAmountTextBox?.Text?.Replace(",", "") ?? "0", out decimal paidAmount);
        decimal remaining = total - paidAmount;

        Invoice.Subtotal = subtotal;
        Invoice.SubTotal = subtotal;
        Invoice.DiscountType = isPercentage ? DiscountType.Percentage : DiscountType.Fixed;
        Invoice.DiscountValue = discountValue;
        Invoice.DiscountAmount = discountAmount;
        Invoice.Discount = discountAmount;
        Invoice.Total = total;
        Invoice.PaidAmount = paidAmount;
        Invoice.RemainingAmount = remaining;

        // Set payment status
        if (paidAmount >= total)
            Invoice.PaymentStatus = PaymentStatus.Paid;
        else if (paidAmount > 0)
            Invoice.PaymentStatus = PaymentStatus.Partial;
        else
            Invoice.PaymentStatus = PaymentStatus.Unpaid;

        // Set status
        Invoice.Status = InvoiceStatus.Confirmed;

        // Generate invoice number if new
        if (!_isEditMode)
        {
            Invoice.InvoiceNumber = GenerateInvoiceNumber();
        }

        // Set defaults
        if (_defaultCurrency != null) Invoice.CurrencyId = _defaultCurrency.Id;
        if (selectedCashbox != null) Invoice.CashboxId = selectedCashbox.Id;
        if (_defaultStore != null) Invoice.StoreId = _defaultStore.Id;

        // Set items for database
        Invoice.Items = _items.Select(i => new InvoiceItem
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            CostPrice = i.CostPrice,
            Subtotal = i.Total,
            Total = i.Total,
            TotalPrice = i.Total,
            Description = i.ProductName,
            Product = _allProducts.FirstOrDefault(p => p.Id == i.ProductId),
            Profit = (i.UnitPrice - i.CostPrice) * i.Quantity
        }).ToList();

        // الطباعة
        try
        {
            var settings = InvoiceSettingsPage.GetCurrentSettings();
            var printService = new PrintService(settings);
            
            var printed = printService.PrintSaleInvoice(Invoice, showDialog: false);
            
            if (printed)
            {
                MessageBox.Show(
                    "تم حفظ الفاتورة وطباعتها بنجاح ✅",
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
                $"تم حفظ الفاتورة لكن حدث خطأ في الطباعة:\n{ex.Message}",
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
