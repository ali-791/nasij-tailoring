using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TailoringShop.Core.Entities;
using TailoringShop.Core.Enums;
using TailoringShop.Infrastructure.Data;

namespace TailoringShop.WPF.Views.Dialogs;

public partial class CashboxTransactionDialog : Window
{
    private readonly AppDbContext _context;
    private readonly Cashbox _cashbox;
    private readonly TransactionType _transactionType;
    private bool _isInitialized = false;

    public CashboxTransactionDialog(AppDbContext context, Cashbox cashbox, TransactionType transactionType)
    {
        InitializeComponent();
        
        _context = context;
        _cashbox = cashbox;
        _transactionType = transactionType;

        SetupUI();
        _isInitialized = true;

        // Allow window dragging
        MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };
    }

    private void SetupUI()
    {
        CashboxNameText.Text = _cashbox.Name;
        CurrentBalanceText.Text = $"{_cashbox.CurrentBalance:N0} د.ع";

        if (_transactionType == TransactionType.Deposit)
        {
            TitleText.Text = "إيداع مبلغ";
            SubtitleText.Text = "إضافة مبلغ للصندوق";
            SaveButton.Content = "💵 تأكيد الإيداع";
            SaveButton.Style = (Style)FindResource("SuccessButton");
            HeaderBorder.Background = (Brush)FindResource("SuccessGradientBrush");
        }
        else
        {
            TitleText.Text = "سحب مبلغ";
            SubtitleText.Text = "سحب مبلغ من الصندوق";
            SaveButton.Content = "💸 تأكيد السحب";
            SaveButton.Style = (Style)FindResource("DangerButton");
            HeaderBorder.Background = (Brush)FindResource("DangerBrush");
        }

        UpdateNewBalance();
    }

    private void AmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isInitialized)
        {
            UpdateNewBalance();
        }
    }

    private void UpdateNewBalance()
    {
        decimal.TryParse(AmountTextBox.Text?.Replace(",", "") ?? "0", out decimal amount);

        decimal newBalance;
        if (_transactionType == TransactionType.Deposit)
        {
            newBalance = _cashbox.CurrentBalance + amount;
        }
        else
        {
            newBalance = _cashbox.CurrentBalance - amount;
        }

        NewBalanceText.Text = $"{newBalance:N0} د.ع";

        // Change color if negative
        if (newBalance < 0)
        {
            NewBalanceText.Foreground = (Brush)FindResource("DangerBrush");
        }
        else
        {
            NewBalanceText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565c0"));
        }
    }

    private bool ValidateForm()
    {
        decimal.TryParse(AmountTextBox.Text?.Replace(",", "") ?? "0", out decimal amount);

        if (amount <= 0)
        {
            AmountError.Text = "يرجى إدخال مبلغ صحيح";
            AmountError.Visibility = Visibility.Visible;
            return false;
        }

        if (_transactionType == TransactionType.Withdrawal && amount > _cashbox.CurrentBalance)
        {
            AmountError.Text = "المبلغ أكبر من الرصيد المتاح";
            AmountError.Visibility = Visibility.Visible;
            return false;
        }

        AmountError.Visibility = Visibility.Collapsed;
        return true;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateForm()) return;

        try
        {
            decimal.TryParse(AmountTextBox.Text?.Replace(",", "") ?? "0", out decimal amount);

            var balanceBefore = _cashbox.CurrentBalance;
            decimal balanceAfter;

            if (_transactionType == TransactionType.Deposit)
            {
                balanceAfter = balanceBefore + amount;
            }
            else
            {
                balanceAfter = balanceBefore - amount;
            }

            // Create transaction
            var transaction = new CashboxTransaction
            {
                CashboxId = _cashbox.Id,
                Type = _transactionType,
                Amount = amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                ReferenceType = _transactionType == TransactionType.Deposit ? "إيداع يدوي" : "سحب يدوي",
                CreatedBy = App.CurrentUser?.Id
            };

            _context.CashboxTransactions.Add(transaction);

            // Update cashbox balance
            _cashbox.CurrentBalance = balanceAfter;
            _context.Cashboxes.Update(_cashbox);

            await _context.SaveChangesAsync();

            var actionText = _transactionType == TransactionType.Deposit ? "إيداع" : "سحب";
            MessageBox.Show(
                $"تم {actionText} مبلغ {amount:N0} د.ع بنجاح",
                "تمت العملية",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ: {ex.Message}",
                "خطأ",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }
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
