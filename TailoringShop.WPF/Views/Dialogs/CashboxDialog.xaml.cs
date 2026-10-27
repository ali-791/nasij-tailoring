using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TailoringShop.Core.Entities;
using TailoringShop.Core.Enums;
using TailoringShop.Infrastructure.Data;

namespace TailoringShop.WPF.Views.Dialogs;

public partial class CashboxDialog : Window
{
    public Cashbox? Cashbox { get; private set; }
    private readonly AppDbContext _context;
    private readonly bool _isEditMode;
    private Currency? _defaultCurrency;

    public CashboxDialog(AppDbContext context, Cashbox? cashbox = null)
    {
        InitializeComponent();
        
        _context = context;
        _isEditMode = cashbox != null;

        _defaultCurrency = _context.Currencies.FirstOrDefault(c => c.IsDefault);

        if (_isEditMode && cashbox != null)
        {
            TitleText.Text = "تعديل الصندوق";
            Cashbox = cashbox;
            LoadCashboxData();
            InitialBalancePanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            TitleText.Text = "إضافة صندوق جديد";
            TypeComboBox.SelectedIndex = 0;
        }

        // Allow window dragging
        MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };
    }

    private void LoadCashboxData()
    {
        if (Cashbox == null) return;

        NameTextBox.Text = Cashbox.Name;

        // Set type
        var typeIndex = Cashbox.Type switch
        {
            CashboxType.General => 0,
            CashboxType.Tailoring => 1,
            CashboxType.ReadyMade => 2,
            CashboxType.Perfumes => 3,
            _ => 0
        };
        TypeComboBox.SelectedIndex = typeIndex;
    }

    private bool ValidateForm()
    {
        bool isValid = true;

        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            NameError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            NameError.Visibility = Visibility.Collapsed;
        }

        return isValid;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm()) return;

        if (Cashbox == null)
        {
            Cashbox = new Cashbox();
        }

        Cashbox.Name = NameTextBox.Text.Trim();
        Cashbox.CurrencyId = _defaultCurrency?.Id ?? 1;

        // Get type
        var selectedItem = TypeComboBox.SelectedItem as ComboBoxItem;
        var typeTag = selectedItem?.Tag?.ToString() ?? "General";
        Cashbox.Type = typeTag switch
        {
            "Tailoring" => CashboxType.Tailoring,
            "ReadyMade" => CashboxType.ReadyMade,
            "Perfumes" => CashboxType.Perfumes,
            _ => CashboxType.General
        };

        // Set initial balance for new cashbox
        if (!_isEditMode)
        {
            decimal.TryParse(InitialBalanceTextBox.Text?.Replace(",", "") ?? "0", out decimal initialBalance);
            Cashbox.CurrentBalance = initialBalance;
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
