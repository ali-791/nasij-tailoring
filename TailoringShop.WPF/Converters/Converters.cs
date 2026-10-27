using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TailoringShop.Core.Entities;

namespace TailoringShop.WPF.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool inverse = parameter?.ToString()?.ToLower() == "inverse";
        
        if (value is bool boolValue)
        {
            if (inverse) boolValue = !boolValue;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return inverse ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool inverse = parameter?.ToString()?.ToLower() == "inverse";
        
        if (value is string stringValue)
        {
            bool isVisible = !string.IsNullOrEmpty(stringValue);
            if (inverse) isVisible = !isVisible;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        return inverse ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool inverse = parameter?.ToString()?.ToLower() == "inverse";
        
        if (value is int intValue)
        {
            bool isVisible = intValue > 0;
            if (inverse) isVisible = !isVisible;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        return inverse ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DecimalToCurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue.ToString("N0") + " د.ع";
        }
        return "0 د.ع";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.ToLower() switch
            {
                "new" => "#f57c00",
                "in_progress" or "inprogress" => "#1976d2",
                "ready" => "#388e3c",
                "delivered" => "#00b894",
                "cancelled" => "#e74c3c",
                "paid" => "#00b894",
                "partial" => "#f5a623",
                "unpaid" => "#e74c3c",
                _ => "#636e72"
            };
        }
        return "#636e72";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.ToLower() switch
            {
                "new" => "جديد",
                "in_progress" or "inprogress" => "قيد التنفيذ",
                "ready" => "جاهز",
                "delivered" => "تم التسليم",
                "cancelled" => "ملغي",
                "draft" => "مسودة",
                "confirmed" => "مؤكد",
                "paid" => "مدفوع",
                "partial" => "مدفوع جزئياً",
                "unpaid" => "غير مدفوع",
                _ => status
            };
        }
        return value?.ToString() ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? "نشط" : "غير نشط";
        }
        return "غير محدد";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? new SolidColorBrush(Color.FromRgb(232, 245, 233)) : new SolidColorBrush(Color.FromRgb(255, 235, 238));
        }
        return new SolidColorBrush(Colors.LightGray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? new SolidColorBrush(Color.FromRgb(56, 142, 60)) : new SolidColorBrush(Color.FromRgb(211, 47, 47));
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InverseStringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return string.IsNullOrEmpty(stringValue) ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StockQuantityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ICollection<Inventory> inventories)
        {
            var total = inventories.Sum(i => i.Quantity);
            return total.ToString("N0");
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StockToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ICollection<Inventory> inventories)
        {
            var total = inventories.Sum(i => i.Quantity);
            if (total <= 0)
                return new SolidColorBrush(Color.FromRgb(255, 235, 238)); // Red background
            else if (total <= 10)
                return new SolidColorBrush(Color.FromRgb(255, 248, 225)); // Yellow background
            else
                return new SolidColorBrush(Color.FromRgb(232, 245, 233)); // Green background
        }
        return new SolidColorBrush(Color.FromRgb(245, 245, 245));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StockToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ICollection<Inventory> inventories)
        {
            var total = inventories.Sum(i => i.Quantity);
            if (total <= 0)
                return new SolidColorBrush(Color.FromRgb(211, 47, 47)); // Red
            else if (total <= 10)
                return new SolidColorBrush(Color.FromRgb(245, 166, 35)); // Orange
            else
                return new SolidColorBrush(Color.FromRgb(56, 142, 60)); // Green
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class OrderStatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TailoringShop.Core.Enums.OrderStatus status)
        {
            return status switch
            {
                TailoringShop.Core.Enums.OrderStatus.New => "جديد",
                TailoringShop.Core.Enums.OrderStatus.InProgress => "قيد التنفيذ",
                TailoringShop.Core.Enums.OrderStatus.Ready => "جاهز",
                TailoringShop.Core.Enums.OrderStatus.Delivered => "تم التسليم",
                TailoringShop.Core.Enums.OrderStatus.Cancelled => "ملغي",
                _ => status.ToString()
            };
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class OrderStatusToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TailoringShop.Core.Enums.OrderStatus status)
        {
            return status switch
            {
                TailoringShop.Core.Enums.OrderStatus.New => new SolidColorBrush(Color.FromRgb(255, 243, 224)),
                TailoringShop.Core.Enums.OrderStatus.InProgress => new SolidColorBrush(Color.FromRgb(227, 242, 253)),
                TailoringShop.Core.Enums.OrderStatus.Ready => new SolidColorBrush(Color.FromRgb(232, 245, 233)),
                TailoringShop.Core.Enums.OrderStatus.Delivered => new SolidColorBrush(Color.FromRgb(225, 245, 254)),
                TailoringShop.Core.Enums.OrderStatus.Cancelled => new SolidColorBrush(Color.FromRgb(255, 235, 238)),
                _ => new SolidColorBrush(Colors.LightGray)
            };
        }
        return new SolidColorBrush(Colors.LightGray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class OrderStatusToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TailoringShop.Core.Enums.OrderStatus status)
        {
            return status switch
            {
                TailoringShop.Core.Enums.OrderStatus.New => new SolidColorBrush(Color.FromRgb(245, 124, 0)),
                TailoringShop.Core.Enums.OrderStatus.InProgress => new SolidColorBrush(Color.FromRgb(25, 118, 210)),
                TailoringShop.Core.Enums.OrderStatus.Ready => new SolidColorBrush(Color.FromRgb(56, 142, 60)),
                TailoringShop.Core.Enums.OrderStatus.Delivered => new SolidColorBrush(Color.FromRgb(2, 136, 209)),
                TailoringShop.Core.Enums.OrderStatus.Cancelled => new SolidColorBrush(Color.FromRgb(211, 47, 47)),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Invoice Type Converters
public class InvoiceTypeToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TailoringShop.Core.Enums.InvoiceType type)
        {
            return type switch
            {
                TailoringShop.Core.Enums.InvoiceType.Sale => "بيع",
                TailoringShop.Core.Enums.InvoiceType.Purchase => "شراء",
                _ => type.ToString()
            };
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InvoiceTypeToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TailoringShop.Core.Enums.InvoiceType type)
        {
            return type switch
            {
                TailoringShop.Core.Enums.InvoiceType.Sale => new SolidColorBrush(Color.FromRgb(232, 245, 233)),
                TailoringShop.Core.Enums.InvoiceType.Purchase => new SolidColorBrush(Color.FromRgb(255, 243, 224)),
                _ => new SolidColorBrush(Colors.LightGray)
            };
        }
        return new SolidColorBrush(Colors.LightGray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InvoiceTypeToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TailoringShop.Core.Enums.InvoiceType type)
        {
            return type switch
            {
                TailoringShop.Core.Enums.InvoiceType.Sale => new SolidColorBrush(Color.FromRgb(56, 142, 60)),
                TailoringShop.Core.Enums.InvoiceType.Purchase => new SolidColorBrush(Color.FromRgb(245, 124, 0)),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Payment Status Converters
public class PaymentStatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TailoringShop.Core.Enums.PaymentStatus status)
        {
            return status switch
            {
                TailoringShop.Core.Enums.PaymentStatus.Paid => "مدفوع",
                TailoringShop.Core.Enums.PaymentStatus.Partial => "جزئي",
                TailoringShop.Core.Enums.PaymentStatus.Unpaid => "غير مدفوع",
                _ => status.ToString()
            };
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PaymentStatusToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TailoringShop.Core.Enums.PaymentStatus status)
        {
            return status switch
            {
                TailoringShop.Core.Enums.PaymentStatus.Paid => new SolidColorBrush(Color.FromRgb(232, 245, 233)),
                TailoringShop.Core.Enums.PaymentStatus.Partial => new SolidColorBrush(Color.FromRgb(255, 248, 225)),
                TailoringShop.Core.Enums.PaymentStatus.Unpaid => new SolidColorBrush(Color.FromRgb(255, 235, 238)),
                _ => new SolidColorBrush(Colors.LightGray)
            };
        }
        return new SolidColorBrush(Colors.LightGray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PaymentStatusToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TailoringShop.Core.Enums.PaymentStatus status)
        {
            return status switch
            {
                TailoringShop.Core.Enums.PaymentStatus.Paid => new SolidColorBrush(Color.FromRgb(56, 142, 60)),
                TailoringShop.Core.Enums.PaymentStatus.Partial => new SolidColorBrush(Color.FromRgb(245, 166, 35)),
                TailoringShop.Core.Enums.PaymentStatus.Unpaid => new SolidColorBrush(Color.FromRgb(211, 47, 47)),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CustomerSupplierConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Invoice invoice)
        {
            if (invoice.Customer != null)
                return invoice.Customer.FullName;
            if (!string.IsNullOrEmpty(invoice.SupplierName))
                return invoice.SupplierName;
            return "غير محدد";
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CashboxTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TailoringShop.Core.Enums.CashboxType type)
        {
            return type switch
            {
                TailoringShop.Core.Enums.CashboxType.General => "صندوق عام",
                TailoringShop.Core.Enums.CashboxType.Tailoring => "صندوق الفصال",
                TailoringShop.Core.Enums.CashboxType.ReadyMade => "صندوق الملابس الجاهزة",
                TailoringShop.Core.Enums.CashboxType.Perfumes => "صندوق العطور",
                _ => "غير محدد"
            };
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class IsNegativeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue < 0;
        }
        if (value is double doubleValue)
        {
            return doubleValue < 0;
        }
        if (value is int intValue)
        {
            return intValue < 0;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// ========== Permission Converters ==========

/// <summary>
/// Converter للتحقق من صلاحية معينة وتحويلها إلى Visibility
/// Usage: Visibility="{Binding Converter={StaticResource PermissionToVisibilityConverter}, ConverterParameter='expenses.create'}"
/// </summary>
public class PermissionToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string permissionKey)
        {
            var parts = permissionKey.Split('.');
            if (parts.Length == 2)
            {
                var hasPermission = TailoringShop.WPF.Services.PermissionService.Instance.HasPermission(parts[0], parts[1]);
                return hasPermission ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter للتحقق من صلاحية معينة وتحويلها إلى bool
/// Usage: IsEnabled="{Binding Converter={StaticResource PermissionToBoolConverter}, ConverterParameter='expenses.update'}"
/// </summary>
public class PermissionToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string permissionKey)
        {
            var parts = permissionKey.Split('.');
            if (parts.Length == 2)
            {
                return TailoringShop.WPF.Services.PermissionService.Instance.HasPermission(parts[0], parts[1]);
            }
        }
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
