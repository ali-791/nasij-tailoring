using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using TailoringShop.Core.Entities;
using TailoringShop.Core.Enums;
using TailoringShop.Infrastructure.Data;

namespace TailoringShop.WPF.Views.Dialogs;

// Helper class for purchase items
public class PurchaseItemDisplay
{
    public long? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public long? CategoryId { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal Total { get; set; }
    public string Unit { get; set; } = "قطعة";
    public int MinStockLevel { get; set; } = 5;
    public bool IsNewProduct { get; set; }
}

public partial class PurchaseInvoiceDialog : Window
{
    public Invoice? Invoice { get; private set; }
    public bool ProductsUpdated { get; private set; } = false;
    
    private readonly AppDbContext _context;
    private Currency? _defaultCurrency;
    private Store? _defaultStore;
    private ObservableCollection<PurchaseItemDisplay> _items = new();
    private bool _isInitialized = false;

    public PurchaseInvoiceDialog(AppDbContext context)
    {
        InitializeComponent();
        
        _context = context;
        
        LoadData();
        
        InvoiceDatePicker.SelectedDate = DateTime.Today;
        ItemsDataGrid.ItemsSource = _items;
        _isInitialized = true;

        MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };
        
        Loaded += (s, e) => UpdateTotals();
    }

    private void LoadData()
    {
        // Load categories
        var categories = _context.Categories.Where(c => c.IsActive).ToList();
        CategoryComboBox.ItemsSource = categories;
        if (categories.Any())
            CategoryComboBox.SelectedIndex = 0;

        // Load cashboxes (with empty option)
        var cashboxes = _context.Cashboxes
            .Include(c => c.Currency)
            .Where(c => c.IsActive)
            .ToList();
        
        // Add empty option for initial stock
        CashboxComboBox.ItemsSource = cashboxes;
        CashboxComboBox.SelectedIndex = -1; // No selection by default

        // Get defaults
        _defaultCurrency = _context.Currencies.FirstOrDefault(c => c.IsDefault);
        _defaultStore = _context.Stores.FirstOrDefault(s => s.IsDefault);
    }

    private void AddProductButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
        {
            MessageBox.Show("يرجى إدخال اسم المنتج", "تنبيه",
                MessageBoxButton.OK, MessageBoxImage.Warning,
                MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            ProductNameTextBox.Focus();
            return;
        }

        decimal.TryParse(QuantityTextBox.Text?.Replace(",", "") ?? "1", out decimal quantity);
        decimal.TryParse(CostPriceTextBox.Text?.Replace(",", "") ?? "0", out decimal costPrice);
        decimal.TryParse(SellingPriceTextBox.Text?.Replace(",", "") ?? "0", out decimal sellingPrice);
        int.TryParse(MinStockTextBox.Text ?? "5", out int minStock);

        if (quantity <= 0) quantity = 1;

        var productName = ProductNameTextBox.Text.Trim();
        var sku = string.IsNullOrWhiteSpace(SKUTextBox.Text) ? null : SKUTextBox.Text.Trim();
        var barcode = string.IsNullOrWhiteSpace(BarcodeTextBox.Text) ? null : BarcodeTextBox.Text.Trim();
        var category = CategoryComboBox.SelectedItem as Category;
        var unit = (UnitComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "قطعة";

        // Check if product exists in database
        var existingProduct = _context.Products
            .FirstOrDefault(p => p.Name.ToLower() == productName.ToLower() || 
                                (barcode != null && p.Barcode == barcode) ||
                                (sku != null && p.SKU == sku));

        // Check if already in list
        var existingInList = _items.FirstOrDefault(i => 
            i.ProductName.ToLower() == productName.ToLower() ||
            (barcode != null && i.Barcode == barcode) ||
            (sku != null && i.SKU == sku));

        if (existingInList != null)
        {
            existingInList.Quantity += quantity;
            existingInList.Total = existingInList.Quantity * existingInList.CostPrice;
            ItemsDataGrid.Items.Refresh();
        }
        else
        {
            _items.Add(new PurchaseItemDisplay
            {
                ProductId = existingProduct?.Id,
                ProductName = productName,
                SKU = sku,
                Barcode = barcode,
                CategoryId = category?.Id,
                Quantity = quantity,
                CostPrice = costPrice,
                SellingPrice = sellingPrice,
                Total = quantity * costPrice,
                Unit = unit,
                MinStockLevel = minStock,
                IsNewProduct = existingProduct == null
            });
        }

        // Reset inputs
        ProductNameTextBox.Text = "";
        SKUTextBox.Text = "";
        BarcodeTextBox.Text = "";
        QuantityTextBox.Text = "1";
        CostPriceTextBox.Text = "0";
        SellingPriceTextBox.Text = "0";
        MinStockTextBox.Text = "5";
        ProductNameTextBox.Focus();

        UpdateTotals();
    }

    private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is PurchaseItemDisplay item)
        {
            _items.Remove(item);
            UpdateTotals();
        }
    }

    private void PaidAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isInitialized) UpdateTotals();
    }

    private void UpdateTotals()
    {
        if (ItemsCountText == null || TotalQuantityText == null || TotalText == null || RemainingText == null)
            return;

        decimal total = _items.Sum(i => i.Total);
        decimal totalQuantity = _items.Sum(i => i.Quantity);

        decimal.TryParse(PaidAmountTextBox?.Text?.Replace(",", "") ?? "0", out decimal paidAmount);
        decimal remaining = total - paidAmount;

        ItemsCountText.Text = _items.Count.ToString();
        TotalQuantityText.Text = totalQuantity.ToString("N0");
        TotalText.Text = $"{total:N0} د.ع";
        RemainingText.Text = $"{remaining:N0} د.ع";
    }

    private bool ValidateForm()
    {
        bool isValid = true;

        if (_items.Count == 0)
        {
            MessageBox.Show("يرجى إضافة منتج واحد على الأقل", "تنبيه",
                MessageBoxButton.OK, MessageBoxImage.Warning,
                MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            isValid = false;
        }

        return isValid;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm()) return;

        try
        {
            var selectedCashbox = CashboxComboBox.SelectedItem as Cashbox;
            
            // Calculate totals
            decimal total = _items.Sum(i => i.Total);
            decimal.TryParse(PaidAmountTextBox?.Text?.Replace(",", "") ?? "0", out decimal paidAmount);
            decimal remaining = total - paidAmount;

            // Create Invoice
            Invoice = new Invoice
            {
                Type = InvoiceType.Purchase,
                InvoiceNumber = GenerateInvoiceNumber(),
                SupplierName = string.IsNullOrWhiteSpace(SupplierTextBox.Text) ? "مخزون أولي" : SupplierTextBox.Text.Trim(),
                InvoiceDate = InvoiceDatePicker.SelectedDate ?? DateTime.Today,
                Notes = NotesTextBox.Text,
                Status = InvoiceStatus.Confirmed,
                Subtotal = total,
                Total = total,
                PaidAmount = paidAmount,
                RemainingAmount = remaining
            };

            // Set payment status
            if (paidAmount >= total)
                Invoice.PaymentStatus = PaymentStatus.Paid;
            else if (paidAmount > 0)
                Invoice.PaymentStatus = PaymentStatus.Partial;
            else
                Invoice.PaymentStatus = PaymentStatus.Unpaid;

            // Set defaults
            if (_defaultCurrency != null) Invoice.CurrencyId = _defaultCurrency.Id;
            if (_defaultStore != null) Invoice.StoreId = _defaultStore.Id;
            
            // Set cashbox (if selected)
            if (selectedCashbox != null)
            {
                Invoice.CashboxId = selectedCashbox.Id;
            }
            else
            {
                // Use default cashbox ID but don't deduct from it
                var defaultCashbox = await _context.Cashboxes.FirstOrDefaultAsync(c => c.IsActive);
                if (defaultCashbox != null) Invoice.CashboxId = defaultCashbox.Id;
            }

            Invoice.CreatedBy = App.CurrentUser?.Id;

            // Process items and add/update products
            var invoiceItems = new List<InvoiceItem>();

            foreach (var item in _items)
            {
                Product product;

                if (item.IsNewProduct)
                {
                    // Create new product
                    product = new Product
                    {
                        Name = item.ProductName,
                        SKU = item.SKU,
                        Barcode = item.Barcode,
                        CategoryId = item.CategoryId,
                        CashboxId = selectedCashbox?.Id, // ربط المنتج بالصندوق
                        CostPrice = item.CostPrice,
                        SellingPrice = item.SellingPrice,
                        CurrencyId = _defaultCurrency?.Id ?? 1,
                        Unit = item.Unit,
                        MinStockLevel = item.MinStockLevel,
                        IsActive = true,
                        CreatedBy = App.CurrentUser?.Id
                    };

                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    // Create inventory for new product
                    if (_defaultStore != null)
                    {
                        var inventory = new Inventory
                        {
                            ProductId = product.Id,
                            StoreId = _defaultStore.Id,
                            Quantity = item.Quantity
                        };
                        _context.Inventories.Add(inventory);
                    }
                }
                else
                {
                    // Update existing product
                    product = await _context.Products
                        .Include(p => p.Inventories)
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product != null)
                    {
                        // Update prices
                        product.CostPrice = item.CostPrice;
                        if (item.SellingPrice > 0)
                            product.SellingPrice = item.SellingPrice;

                        // Update inventory quantity
                        if (_defaultStore != null)
                        {
                            var inventory = product.Inventories?.FirstOrDefault(i => i.StoreId == _defaultStore.Id);
                            if (inventory != null)
                            {
                                inventory.Quantity += item.Quantity;
                            }
                            else
                            {
                                _context.Inventories.Add(new Inventory
                                {
                                    ProductId = product.Id,
                                    StoreId = _defaultStore.Id,
                                    Quantity = item.Quantity
                                });
                            }
                        }
                    }
                }

                // Add invoice item
                if (product != null)
                {
                    invoiceItems.Add(new InvoiceItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.CostPrice,
                        CostPrice = item.CostPrice,
                        Subtotal = item.Total,
                        Total = item.Total
                    });
                }
            }

            Invoice.Items = invoiceItems;

            // Save invoice
            _context.Invoices.Add(Invoice);
            await _context.SaveChangesAsync();

            // IMPORTANT: Deduct from cashbox if selected AND paid amount > 0
            if (selectedCashbox != null && paidAmount > 0)
            {
                var cashbox = await _context.Cashboxes.FindAsync(selectedCashbox.Id);
                if (cashbox != null)
                {
                    var balanceBefore = cashbox.CurrentBalance;
                    var balanceAfter = balanceBefore - paidAmount;

                    // Create transaction record
                    var transaction = new CashboxTransaction
                    {
                        CashboxId = cashbox.Id,
                        Type = TransactionType.Withdrawal,
                        Amount = paidAmount,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        ReferenceType = "PurchaseInvoice",
                        ReferenceId = Invoice.Id,
                        CreatedBy = App.CurrentUser?.Id
                    };

                    _context.CashboxTransactions.Add(transaction);
                    cashbox.CurrentBalance = balanceAfter;
                    await _context.SaveChangesAsync();
                }
            }

            ProductsUpdated = true;

            string message = selectedCashbox != null && paidAmount > 0
                ? $"تم حفظ الفاتورة وإضافة {_items.Count} منتج للمخزون وخصم {paidAmount:N0} من الصندوق!"
                : $"تم حفظ الفاتورة وإضافة {_items.Count} منتج للمخزون بنجاح!";

            MessageBox.Show(message, "تم بنجاح",
                MessageBoxButton.OK, MessageBoxImage.Information,
                MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Error,
                MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }
    }

    private string GenerateInvoiceNumber()
    {
        var today = DateTime.Today;
        var prefix = $"PUR-{today:yyyyMMdd}-";
        
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
