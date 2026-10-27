using System.Windows;
using System.Windows.Input;
using TailoringShop.Core.Entities;
using TailoringShop.Core.Enums;
using TailoringShop.Infrastructure.Data;

namespace TailoringShop.WPF.Views.Dialogs;

public partial class ExpenseDialog : Window
{
    public Expense? Expense { get; private set; }
    private readonly AppDbContext _context;
    private readonly bool _isEditMode;
    private Currency? _defaultCurrency;

    public ExpenseDialog(AppDbContext context, Expense? expense = null)
    {
        InitializeComponent();
        
        _context = context;
        _isEditMode = expense != null;

        LoadData();
        
        if (_isEditMode && expense != null)
        {
            TitleText.Text = "تعديل المصروف";
            Expense = expense;
            LoadExpenseData();
        }
        else
        {
            TitleText.Text = "إضافة مصروف جديد";
            ExpenseDatePicker.SelectedDate = DateTime.Today;
        }

        // Allow window dragging
        MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };
    }

    private void LoadData()
    {
        // Load categories
        var categories = _context.ExpenseCategories.Where(c => c.IsActive).ToList();
        CategoryComboBox.ItemsSource = categories;
        if (categories.Any())
            CategoryComboBox.SelectedIndex = 0;

        // Load cashboxes
        var cashboxes = _context.Cashboxes.Where(c => c.IsActive).ToList();
        CashboxComboBox.ItemsSource = cashboxes;
        
        // Select default cashbox
        var defaultCashbox = cashboxes.FirstOrDefault(c => c.Type == CashboxType.General);
        if (defaultCashbox != null)
            CashboxComboBox.SelectedItem = defaultCashbox;
        else if (cashboxes.Any())
            CashboxComboBox.SelectedIndex = 0;

        // Get default currency
        _defaultCurrency = _context.Currencies.FirstOrDefault(c => c.IsDefault);
    }

    private void LoadExpenseData()
    {
        if (Expense == null) return;

        // Set category
        foreach (var item in CategoryComboBox.Items)
        {
            if (item is ExpenseCategory category && category.Id == Expense.CategoryId)
            {
                CategoryComboBox.SelectedItem = category;
                break;
            }
        }

        // Set cashbox
        foreach (var item in CashboxComboBox.Items)
        {
            if (item is Cashbox cashbox && cashbox.Id == Expense.CashboxId)
            {
                CashboxComboBox.SelectedItem = cashbox;
                break;
            }
        }

        AmountTextBox.Text = Expense.Amount.ToString("N0");
        DescriptionTextBox.Text = Expense.Description;
        ExpenseDatePicker.SelectedDate = Expense.ExpenseDate;
        ReferenceTextBox.Text = Expense.ReferenceNumber ?? "";
    }

    private bool ValidateForm()
    {
        bool isValid = true;

        // Validate category
        if (CategoryComboBox.SelectedItem == null)
        {
            CategoryError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            CategoryError.Visibility = Visibility.Collapsed;
        }

        // Validate amount
        decimal.TryParse(AmountTextBox.Text?.Replace(",", "") ?? "0", out decimal amount);
        if (amount <= 0)
        {
            AmountError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            AmountError.Visibility = Visibility.Collapsed;
        }

        // Validate description
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

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm()) return;

        if (Expense == null)
        {
            Expense = new Expense();
        }

        var selectedCategory = CategoryComboBox.SelectedItem as ExpenseCategory;
        var selectedCashbox = CashboxComboBox.SelectedItem as Cashbox;

        Expense.CategoryId = selectedCategory?.Id ?? 0;
        Expense.CashboxId = selectedCashbox?.Id ?? 0;
        Expense.CurrencyId = _defaultCurrency?.Id ?? 1;
        
        decimal.TryParse(AmountTextBox.Text?.Replace(",", "") ?? "0", out decimal amount);
        Expense.Amount = amount;
        
        Expense.Description = DescriptionTextBox.Text.Trim();
        Expense.ExpenseDate = ExpenseDatePicker.SelectedDate ?? DateTime.Today;
        Expense.ReferenceNumber = string.IsNullOrWhiteSpace(ReferenceTextBox.Text) ? null : ReferenceTextBox.Text.Trim();

        // Generate expense number if new
        if (!_isEditMode)
        {
            Expense.ExpenseNumber = GenerateExpenseNumber();
        }

        DialogResult = true;
        Close();
    }

    private string GenerateExpenseNumber()
    {
        var today = DateTime.Today;
        var prefix = $"EXP-{today:yyyyMMdd}-";
        
        var lastExpense = _context.Expenses
            .Where(e => e.ExpenseNumber.StartsWith(prefix))
            .OrderByDescending(e => e.ExpenseNumber)
            .FirstOrDefault();

        int sequence = 1;
        if (lastExpense != null && lastExpense.ExpenseNumber.Length > prefix.Length)
        {
            var lastSequence = lastExpense.ExpenseNumber.Substring(prefix.Length);
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
