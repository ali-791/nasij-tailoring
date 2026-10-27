using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using TailoringShop.Core.Entities;
using TailoringShop.Infrastructure.Data;

namespace TailoringShop.WPF.Views.Dialogs;

public partial class ProductDialog : Window
{
    public Product? Product { get; private set; }
    private readonly AppDbContext _context;
    private readonly bool _isEditMode;
    private Currency? _defaultCurrency;

    public ProductDialog(AppDbContext context, Product? product = null)
    {
        InitializeComponent();
        
        _context = context;
        _isEditMode = product != null;
        
        LoadData();
        
        if (_isEditMode && product != null)
        {
            TitleText.Text = "تعديل بيانات المنتج";
            Product = product;
            LoadProductData();
        }
        else
        {
            TitleText.Text = "إضافة منتج جديد";
            Product = new Product();
        }

        // Allow window dragging
        MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };
        
        // Focus on first field
        Loaded += (s, e) => NameTextBox.Focus();
    }

    private void LoadData()
    {
        // Load categories
        var categories = _context.Categories.Where(c => c.IsActive).ToList();
        CategoryComboBox.ItemsSource = categories;

        // Get default currency
        _defaultCurrency = _context.Currencies.FirstOrDefault(c => c.IsDefault);
    }

    private void LoadProductData()
    {
        if (Product == null) return;
        
        NameTextBox.Text = Product.Name;
        SKUTextBox.Text = Product.SKU;
        BarcodeTextBox.Text = Product.Barcode;
        CostPriceTextBox.Text = Product.CostPrice.ToString("N0");
        SellingPriceTextBox.Text = Product.SellingPrice.ToString("N0");
        MinStockTextBox.Text = Product.MinStockLevel.ToString();
        IsActiveCheckBox.IsChecked = Product.IsActive;

        // Set category using SelectedValue (works with SelectedValuePath="Id")
        if (Product.CategoryId.HasValue)
        {
            CategoryComboBox.SelectedValue = Product.CategoryId.Value;
        }

        // Set unit
        var units = new[] { "قطعة", "متر", "كيلو", "لتر", "علبة", "كرتون" };
        var unitMap = new Dictionary<string, int>
        {
            { "piece", 0 }, { "قطعة", 0 },
            { "meter", 1 }, { "متر", 1 },
            { "kg", 2 }, { "كيلو", 2 },
            { "liter", 3 }, { "لتر", 3 },
            { "box", 4 }, { "علبة", 4 },
            { "carton", 5 }, { "كرتون", 5 }
        };

        if (unitMap.TryGetValue(Product.Unit.ToLower(), out int unitIndex))
        {
            UnitComboBox.SelectedIndex = unitIndex;
        }
    }

    private bool ValidateForm()
    {
        bool isValid = true;

        // Validate Name
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            NameError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            NameError.Visibility = Visibility.Collapsed;
        }

        // Validate Selling Price
        if (!decimal.TryParse(SellingPriceTextBox.Text.Replace(",", ""), out decimal sellingPrice) || sellingPrice < 0)
        {
            SellingPriceError.Text = "أدخل سعر بيع صحيح";
            SellingPriceError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            SellingPriceError.Visibility = Visibility.Collapsed;
        }

        return isValid;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm()) return;

        if (Product == null) Product = new Product();

        Product.Name = NameTextBox.Text.Trim();
        Product.SKU = string.IsNullOrWhiteSpace(SKUTextBox.Text) ? null : SKUTextBox.Text.Trim();
        Product.Barcode = string.IsNullOrWhiteSpace(BarcodeTextBox.Text) ? null : BarcodeTextBox.Text.Trim();
        
        decimal.TryParse(CostPriceTextBox.Text.Replace(",", ""), out decimal costPrice);
        decimal.TryParse(SellingPriceTextBox.Text.Replace(",", ""), out decimal sellingPrice);
        int.TryParse(MinStockTextBox.Text, out int minStock);

        Product.CostPrice = costPrice;
        Product.SellingPrice = sellingPrice;
        Product.MinStockLevel = minStock;
        Product.IsActive = IsActiveCheckBox.IsChecked ?? true;

        // Set category using SelectedValue
        if (CategoryComboBox.SelectedValue is long categoryId)
        {
            Product.CategoryId = categoryId;
        }
        else if (CategoryComboBox.SelectedItem is Category selectedCategory)
        {
            Product.CategoryId = selectedCategory.Id;
        }
        else
        {
            Product.CategoryId = null;
        }

        // Set unit
        var unitValues = new[] { "piece", "meter", "kg", "liter", "box", "carton" };
        Product.Unit = unitValues[UnitComboBox.SelectedIndex];

        // Set currency
        if (_defaultCurrency != null)
        {
            Product.CurrencyId = _defaultCurrency.Id;
        }

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
