using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using TailoringShop.WPF.Services;

namespace TailoringShop.WPF.Views.Pages;

public partial class InvoiceSettingsPage : UserControl
{
    private PrintSettings _settings = new();
    private string? _selectedLogoPath;
    
    private static readonly string SettingsFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, 
        "print_settings.json");

    public InvoiceSettingsPage()
    {
        InitializeComponent();
        LoadSettings();
        LoadPrinters();
    }

    /// <summary>
    /// تحميل الإعدادات
    /// </summary>
    private void LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                _settings = JsonSerializer.Deserialize<PrintSettings>(json) ?? new PrintSettings();
            }
        }
        catch
        {
            _settings = new PrintSettings();
        }

        // تعبئة الحقول
        ShopNameTextBox.Text = _settings.ShopName;
        ShopPhoneTextBox.Text = _settings.ShopPhone;
        ShopAddressTextBox.Text = _settings.ShopAddress;
        FooterMessageTextBox.Text = _settings.FooterMessage;
        
        // الشعار
        _selectedLogoPath = _settings.LogoPath;
        if (!string.IsNullOrEmpty(_selectedLogoPath) && File.Exists(_selectedLogoPath))
        {
            try
            {
                LogoPreview.Source = new BitmapImage(new Uri(_selectedLogoPath));
            }
            catch { }
        }
        
        // حجم الورق
        foreach (ComboBoxItem item in PaperSizeComboBox.Items)
        {
            if (item.Tag?.ToString() == _settings.PaperSize.ToString())
            {
                PaperSizeComboBox.SelectedItem = item;
                break;
            }
        }
        
        // خيارات العرض
        ShowLogoCheckBox.IsChecked = _settings.ShowLogo;
        ShowDateCheckBox.IsChecked = _settings.ShowDate;
        ShowCustomerPhoneCheckBox.IsChecked = _settings.ShowCustomerPhone;
        ShowFooterCheckBox.IsChecked = _settings.ShowFooter;
        SaleShowDiscountCheckBox.IsChecked = _settings.SaleShowDiscount;
        TailoringShowDeliveryDateCheckBox.IsChecked = _settings.TailoringShowDeliveryDate;
        TailoringShowNotesCheckBox.IsChecked = _settings.TailoringShowNotes;
    }

    /// <summary>
    /// تحميل قائمة الطابعات
    /// </summary>
    private void LoadPrinters()
    {
        PrinterComboBox.Items.Clear();
        
        var printers = PrintService.GetAvailablePrinters();
        foreach (var printer in printers)
        {
            PrinterComboBox.Items.Add(printer);
        }
        
        // اختيار الطابعة المحفوظة
        if (!string.IsNullOrEmpty(_settings.PrinterName) && printers.Contains(_settings.PrinterName))
        {
            PrinterComboBox.SelectedItem = _settings.PrinterName;
        }
        else if (printers.Count > 0)
        {
            PrinterComboBox.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// تحديث قائمة الطابعات
    /// </summary>
    private void RefreshPrinters_Click(object sender, RoutedEventArgs e)
    {
        LoadPrinters();
        MessageBox.Show("تم تحديث قائمة الطابعات", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// اختيار شعار
    /// </summary>
    private void SelectLogo_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "صور|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
            Title = "اختر شعار المتجر"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                // نسخ الصورة إلى مجلد التطبيق
                var logoFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                if (!Directory.Exists(logoFolder))
                {
                    Directory.CreateDirectory(logoFolder);
                }

                var extension = Path.GetExtension(dialog.FileName);
                var newPath = Path.Combine(logoFolder, $"logo{extension}");
                
                File.Copy(dialog.FileName, newPath, true);
                
                _selectedLogoPath = newPath;
                LogoPreview.Source = new BitmapImage(new Uri(newPath));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الشعار: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// حذف الشعار
    /// </summary>
    private void RemoveLogo_Click(object sender, RoutedEventArgs e)
    {
        _selectedLogoPath = null;
        LogoPreview.Source = null;
    }

    /// <summary>
    /// حفظ الإعدادات
    /// </summary>
    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // جمع الإعدادات
            _settings.ShopName = ShopNameTextBox.Text.Trim();
            _settings.ShopPhone = ShopPhoneTextBox.Text.Trim();
            _settings.ShopAddress = ShopAddressTextBox.Text.Trim();
            _settings.FooterMessage = FooterMessageTextBox.Text.Trim();
            _settings.LogoPath = _selectedLogoPath;
            _settings.PrinterName = PrinterComboBox.SelectedItem?.ToString() ?? "";
            
            // حجم الورق
            if (PaperSizeComboBox.SelectedItem is ComboBoxItem selectedPaper)
            {
                _settings.PaperSize = Enum.Parse<PaperSize>(selectedPaper.Tag?.ToString() ?? "Receipt80mm");
            }
            
            // خيارات العرض
            _settings.ShowLogo = ShowLogoCheckBox.IsChecked ?? true;
            _settings.ShowDate = ShowDateCheckBox.IsChecked ?? true;
            _settings.ShowCustomerPhone = ShowCustomerPhoneCheckBox.IsChecked ?? true;
            _settings.ShowFooter = ShowFooterCheckBox.IsChecked ?? true;
            _settings.SaleShowDiscount = SaleShowDiscountCheckBox.IsChecked ?? true;
            _settings.TailoringShowDeliveryDate = TailoringShowDeliveryDateCheckBox.IsChecked ?? true;
            _settings.TailoringShowNotes = TailoringShowNotesCheckBox.IsChecked ?? true;
            
            // حفظ في ملف
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
            
            MessageBox.Show("تم حفظ الإعدادات بنجاح", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في حفظ الإعدادات: {ex.Message}", "خطأ", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// طباعة تجريبية
    /// </summary>
    private void TestPrint_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // حفظ الإعدادات أولاً
            SaveSettings_Click(sender, e);
            
            // إنشاء فاتورة تجريبية
            var testInvoice = new Core.Entities.Invoice
            {
                InvoiceNumber = "TEST-001",
                InvoiceDate = DateTime.Now,
                Customer = new Core.Entities.Customer
                {
                    FullName = "عميل تجريبي",
                    Phone = "07701234567"
                },
                Items = new List<Core.Entities.InvoiceItem>
                {
                    new() { Description = "قماش قطني", Quantity = 2, UnitPrice = 15000, TotalPrice = 30000 },
                    new() { Description = "خياطة دشداشة", Quantity = 1, UnitPrice = 25000, TotalPrice = 25000 }
                },
                SubTotal = 55000,
                Discount = 5000,
                Total = 50000,
                PaidAmount = 30000,
                RemainingAmount = 20000
            };

            var printService = new PrintService(_settings);
            
            // طباعة مع عرض مربع الحوار
            var result = printService.PrintSaleInvoice(testInvoice, showDialog: true);
            
            if (result)
            {
                MessageBox.Show("تمت الطباعة التجريبية بنجاح", "تم", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// الحصول على إعدادات الطباعة الحالية (للاستخدام من صفحات أخرى)
    /// </summary>
    public static PrintSettings GetCurrentSettings()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<PrintSettings>(json) ?? new PrintSettings();
            }
        }
        catch { }
        
        return new PrintSettings();
    }
}
