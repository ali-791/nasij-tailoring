using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TailoringShop.Core.Entities;

namespace TailoringShop.WPF.Services;

/// <summary>
/// إعدادات الطباعة
/// </summary>
public class PrintSettings
{
    // معلومات المتجر
    public string ShopName { get; set; } = "متجر الخياطة";
    public string ShopPhone { get; set; } = "";
    public string ShopAddress { get; set; } = "";
    public string? LogoPath { get; set; }
    public string FooterMessage { get; set; } = "شكراً لتعاملكم معنا";
    
    // إعدادات الطباعة
    public string PrinterName { get; set; } = "";
    public PaperSize PaperSize { get; set; } = PaperSize.Receipt80mm;
    public bool ShowLogo { get; set; } = true;
    public bool ShowCustomerPhone { get; set; } = true;
    public bool ShowDate { get; set; } = true;
    public bool ShowFooter { get; set; } = true;
    
    // إعدادات فاتورة البيع
    public bool SaleShowProductCode { get; set; } = false;
    public bool SaleShowDiscount { get; set; } = true;
    
    // إعدادات فاتورة الفصال
    public bool TailoringShowMeasurements { get; set; } = true;
    public bool TailoringShowDeliveryDate { get; set; } = true;
    public bool TailoringShowNotes { get; set; } = true;
}

/// <summary>
/// أحجام الورق المدعومة
/// </summary>
public enum PaperSize
{
    Receipt58mm,    // طابعة حرارية 58mm
    Receipt80mm,    // طابعة حرارية 80mm
    A4,             // ورق A4
    A5              // ورق A5
}

/// <summary>
/// خدمة طباعة الفواتير
/// </summary>
public class PrintService
{
    private readonly PrintSettings _settings;

    public PrintService(PrintSettings settings)
    {
        _settings = settings;
    }

    #region طباعة فاتورة البيع

    /// <summary>
    /// طباعة فاتورة بيع
    /// </summary>
    public bool PrintSaleInvoice(Invoice invoice, bool showDialog = false)
    {
        try
        {
            var document = CreateSaleInvoiceDocument(invoice);
            return PrintDocument(document, $"فاتورة بيع - {invoice.InvoiceNumber}", showDialog);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    /// <summary>
    /// إنشاء مستند فاتورة البيع
    /// </summary>
    private FlowDocument CreateSaleInvoiceDocument(Invoice invoice)
    {
        var doc = CreateBaseDocument();
        
        // العنوان
        AddHeader(doc);
        
        // معلومات الفاتورة
        var infoSection = new Section();
        infoSection.Blocks.Add(CreateInfoParagraph("رقم الفاتورة:", invoice.InvoiceNumber));
        
        if (_settings.ShowDate)
        {
            infoSection.Blocks.Add(CreateInfoParagraph("التاريخ:", invoice.InvoiceDate.ToString("yyyy/MM/dd hh:mm tt")));
        }
        
        if (invoice.Customer != null)
        {
            infoSection.Blocks.Add(CreateInfoParagraph("العميل:", invoice.Customer.FullName));
            if (_settings.ShowCustomerPhone && !string.IsNullOrEmpty(invoice.Customer.Phone))
            {
                infoSection.Blocks.Add(CreateInfoParagraph("الهاتف:", invoice.Customer.Phone));
            }
        }
        
        doc.Blocks.Add(infoSection);
        
        // خط فاصل
        AddSeparator(doc);
        
        // جدول المنتجات
        var table = CreateProductsTable(invoice);
        doc.Blocks.Add(table);
        
        // خط فاصل
        AddSeparator(doc);
        
        // المجاميع
        var totalsSection = new Section();
        
        if (invoice.Discount > 0 && _settings.SaleShowDiscount)
        {
            totalsSection.Blocks.Add(CreateTotalParagraph("الخصم:", $"{invoice.Discount:N0}"));
        }
        
        totalsSection.Blocks.Add(CreateTotalParagraph("المجموع الكلي:", $"{invoice.Total:N0}", isBold: true, fontSize: 14));
        totalsSection.Blocks.Add(CreateTotalParagraph("المدفوع:", $"{invoice.PaidAmount:N0}"));
        
        if (invoice.RemainingAmount > 0)
        {
            totalsSection.Blocks.Add(CreateTotalParagraph("المتبقي:", $"{invoice.RemainingAmount:N0}", isBold: true));
        }
        
        doc.Blocks.Add(totalsSection);
        
        // Footer
        if (_settings.ShowFooter)
        {
            AddFooter(doc);
        }
        
        return doc;
    }

    /// <summary>
    /// إنشاء جدول المنتجات
    /// </summary>
    private Table CreateProductsTable(Invoice invoice)
    {
        var table = new Table();
        table.CellSpacing = 0;
        table.FontFamily = new FontFamily("Segoe UI");
        table.FontSize = GetFontSize();
        
        // تعريف الأعمدة
        table.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) }); // المنتج
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) }); // الكمية
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) }); // السعر
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) }); // المجموع
        
        var rowGroup = new TableRowGroup();
        
        // رأس الجدول
        var headerRow = new TableRow();
        headerRow.Background = Brushes.LightGray;
        headerRow.FontWeight = FontWeights.Bold;
        headerRow.Cells.Add(CreateCell("المنتج", TextAlignment.Right));
        headerRow.Cells.Add(CreateCell("الكمية", TextAlignment.Center));
        headerRow.Cells.Add(CreateCell("السعر", TextAlignment.Center));
        headerRow.Cells.Add(CreateCell("المجموع", TextAlignment.Left));
        rowGroup.Rows.Add(headerRow);
        
        // صفوف المنتجات
        if (invoice.Items != null)
        {
            foreach (var item in invoice.Items)
            {
                var row = new TableRow();
                row.Cells.Add(CreateCell(item.Product?.Name ?? item.Description ?? "-", TextAlignment.Right));
                row.Cells.Add(CreateCell(item.Quantity.ToString(), TextAlignment.Center));
                row.Cells.Add(CreateCell($"{item.UnitPrice:N0}", TextAlignment.Center));
                row.Cells.Add(CreateCell($"{item.TotalPrice:N0}", TextAlignment.Left));
                rowGroup.Rows.Add(row);
            }
        }
        
        table.RowGroups.Add(rowGroup);
        return table;
    }

    #endregion

    #region طباعة فاتورة الفصال

    /// <summary>
    /// طباعة فاتورة طلب فصال
    /// </summary>
    public bool PrintTailoringOrder(TailoringOrder order, bool showDialog = false)
    {
        try
        {
            var document = CreateTailoringOrderDocument(order);
            return PrintDocument(document, $"طلب فصال - {order.OrderNumber}", showDialog);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    /// <summary>
    /// إنشاء مستند فاتورة الفصال
    /// </summary>
    private FlowDocument CreateTailoringOrderDocument(TailoringOrder order)
    {
        var doc = CreateBaseDocument();
        
        // العنوان
        AddHeader(doc);
        
        // عنوان الفاتورة
        var title = new Paragraph(new Run("طلب فصال"));
        title.FontSize = 16;
        title.FontWeight = FontWeights.Bold;
        title.TextAlignment = TextAlignment.Center;
        title.Margin = new Thickness(0, 10, 0, 10);
        doc.Blocks.Add(title);
        
        // معلومات الطلب
        var infoSection = new Section();
        infoSection.Blocks.Add(CreateInfoParagraph("رقم الطلب:", order.OrderNumber));
        
        if (_settings.ShowDate)
        {
            infoSection.Blocks.Add(CreateInfoParagraph("تاريخ الطلب:", order.OrderDate.ToString("yyyy/MM/dd")));
        }
        
        if (_settings.TailoringShowDeliveryDate)
        {
            infoSection.Blocks.Add(CreateInfoParagraph("تاريخ التسليم:", order.ExpectedDeliveryDate.ToString("yyyy/MM/dd")));
        }
        
        doc.Blocks.Add(infoSection);
        
        // خط فاصل
        AddSeparator(doc);
        
        // معلومات العميل
        if (order.Customer != null)
        {
            var customerSection = new Section();
            var customerTitle = new Paragraph(new Run("معلومات العميل"));
            customerTitle.FontWeight = FontWeights.Bold;
            customerTitle.Margin = new Thickness(0, 5, 0, 5);
            customerSection.Blocks.Add(customerTitle);
            
            customerSection.Blocks.Add(CreateInfoParagraph("الاسم:", order.Customer.FullName));
            
            if (_settings.ShowCustomerPhone && !string.IsNullOrEmpty(order.Customer.Phone))
            {
                customerSection.Blocks.Add(CreateInfoParagraph("الهاتف:", order.Customer.Phone));
            }
            
            doc.Blocks.Add(customerSection);
            AddSeparator(doc);
        }
        
        // البنود
        if (order.Items != null && order.Items.Any())
        {
            var itemsTitle = new Paragraph(new Run("البنود"));
            itemsTitle.FontWeight = FontWeights.Bold;
            itemsTitle.Margin = new Thickness(0, 5, 0, 5);
            doc.Blocks.Add(itemsTitle);
            
            var table = CreateTailoringItemsTable(order);
            doc.Blocks.Add(table);
            
            AddSeparator(doc);
        }
        
        // الملاحظات
        if (_settings.TailoringShowNotes && !string.IsNullOrEmpty(order.Notes))
        {
            var notesSection = new Section();
            var notesTitle = new Paragraph(new Run("ملاحظات:"));
            notesTitle.FontWeight = FontWeights.Bold;
            notesSection.Blocks.Add(notesTitle);
            
            var notesContent = new Paragraph(new Run(order.Notes));
            notesContent.Margin = new Thickness(10, 0, 0, 0);
            notesSection.Blocks.Add(notesContent);
            
            doc.Blocks.Add(notesSection);
            AddSeparator(doc);
        }
        
        // المجاميع
        var totalsSection = new Section();
        totalsSection.Blocks.Add(CreateTotalParagraph("المجموع:", $"{order.TotalAmount:N0}", isBold: true, fontSize: 14));
        totalsSection.Blocks.Add(CreateTotalParagraph("العربون:", $"{order.DepositAmount:N0}"));
        totalsSection.Blocks.Add(CreateTotalParagraph("المتبقي:", $"{order.RemainingAmount:N0}", isBold: true));
        doc.Blocks.Add(totalsSection);
        
        // Footer
        if (_settings.ShowFooter)
        {
            AddFooter(doc);
        }
        
        return doc;
    }

    /// <summary>
    /// إنشاء جدول بنود الفصال
    /// </summary>
    private Table CreateTailoringItemsTable(TailoringOrder order)
    {
        var table = new Table();
        table.CellSpacing = 0;
        table.FontFamily = new FontFamily("Segoe UI");
        table.FontSize = GetFontSize();
        
        table.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) }); // الوصف
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) }); // الكمية
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) }); // السعر
        
        var rowGroup = new TableRowGroup();
        
        // رأس الجدول
        var headerRow = new TableRow();
        headerRow.Background = Brushes.LightGray;
        headerRow.FontWeight = FontWeights.Bold;
        headerRow.Cells.Add(CreateCell("الوصف", TextAlignment.Right));
        headerRow.Cells.Add(CreateCell("الكمية", TextAlignment.Center));
        headerRow.Cells.Add(CreateCell("السعر", TextAlignment.Left));
        rowGroup.Rows.Add(headerRow);
        
        // الصفوف
        if (order.Items != null)
        {
            foreach (var item in order.Items)
            {
                var row = new TableRow();
                // استخدام ItemName أولاً ثم Description
                var displayName = !string.IsNullOrEmpty(item.ItemName) ? item.ItemName : (item.Description ?? "-");
                row.Cells.Add(CreateCell(displayName, TextAlignment.Right));
                row.Cells.Add(CreateCell(item.Quantity.ToString(), TextAlignment.Center));
                row.Cells.Add(CreateCell($"{item.Price:N0}", TextAlignment.Left));
                rowGroup.Rows.Add(row);
            }
        }
        
        table.RowGroups.Add(rowGroup);
        return table;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// إنشاء المستند الأساسي
    /// </summary>
    private FlowDocument CreateBaseDocument()
    {
        var doc = new FlowDocument();
        doc.FontFamily = new FontFamily("Segoe UI");
        doc.FontSize = GetFontSize();
        doc.FlowDirection = FlowDirection.RightToLeft;
        doc.TextAlignment = TextAlignment.Right;
        
        // تحديد عرض الصفحة حسب حجم الورق
        switch (_settings.PaperSize)
        {
            case PaperSize.Receipt58mm:
                doc.PageWidth = 200; // ~58mm
                doc.PagePadding = new Thickness(5);
                break;
            case PaperSize.Receipt80mm:
                doc.PageWidth = 280; // ~80mm
                doc.PagePadding = new Thickness(10);
                break;
            case PaperSize.A5:
                doc.PageWidth = 420;
                doc.PagePadding = new Thickness(30);
                break;
            case PaperSize.A4:
            default:
                doc.PageWidth = 600;
                doc.PagePadding = new Thickness(40);
                break;
        }
        
        return doc;
    }

    /// <summary>
    /// إضافة رأس الفاتورة
    /// </summary>
    private void AddHeader(FlowDocument doc)
    {
        // الشعار
        if (_settings.ShowLogo && !string.IsNullOrEmpty(_settings.LogoPath) && File.Exists(_settings.LogoPath))
        {
            try
            {
                var logo = new Image();
                logo.Source = new BitmapImage(new Uri(_settings.LogoPath));
                logo.Width = _settings.PaperSize == PaperSize.A4 ? 100 : 60;
                logo.Height = _settings.PaperSize == PaperSize.A4 ? 100 : 60;
                
                var logoContainer = new BlockUIContainer(logo);
                logoContainer.TextAlignment = TextAlignment.Center;
                doc.Blocks.Add(logoContainer);
            }
            catch { }
        }
        
        // اسم المتجر
        var shopName = new Paragraph(new Run(_settings.ShopName));
        shopName.FontSize = _settings.PaperSize == PaperSize.A4 ? 20 : 14;
        shopName.FontWeight = FontWeights.Bold;
        shopName.TextAlignment = TextAlignment.Center;
        shopName.Margin = new Thickness(0, 5, 0, 0);
        doc.Blocks.Add(shopName);
        
        // معلومات المتجر
        if (!string.IsNullOrEmpty(_settings.ShopPhone))
        {
            var phone = new Paragraph(new Run($"هاتف: {_settings.ShopPhone}"));
            phone.TextAlignment = TextAlignment.Center;
            phone.FontSize = GetFontSize() - 1;
            phone.Margin = new Thickness(0);
            doc.Blocks.Add(phone);
        }
        
        if (!string.IsNullOrEmpty(_settings.ShopAddress))
        {
            var address = new Paragraph(new Run(_settings.ShopAddress));
            address.TextAlignment = TextAlignment.Center;
            address.FontSize = GetFontSize() - 1;
            address.Margin = new Thickness(0, 0, 0, 10);
            doc.Blocks.Add(address);
        }
    }

    /// <summary>
    /// إضافة خط فاصل
    /// </summary>
    private void AddSeparator(FlowDocument doc)
    {
        var separator = new Paragraph(new Run(new string('-', 40)));
        separator.TextAlignment = TextAlignment.Center;
        separator.Margin = new Thickness(0, 5, 0, 5);
        doc.Blocks.Add(separator);
    }

    /// <summary>
    /// إضافة Footer
    /// </summary>
    private void AddFooter(FlowDocument doc)
    {
        AddSeparator(doc);
        
        var footer = new Paragraph(new Run(_settings.FooterMessage));
        footer.TextAlignment = TextAlignment.Center;
        footer.FontStyle = FontStyles.Italic;
        footer.Margin = new Thickness(0, 10, 0, 0);
        doc.Blocks.Add(footer);
        
        // وقت الطباعة
        var printTime = new Paragraph(new Run($"طُبع في: {DateTime.Now:yyyy/MM/dd hh:mm tt}"));
        printTime.TextAlignment = TextAlignment.Center;
        printTime.FontSize = GetFontSize() - 2;
        printTime.Foreground = Brushes.Gray;
        doc.Blocks.Add(printTime);
    }

    /// <summary>
    /// إنشاء فقرة معلومات
    /// </summary>
    private Paragraph CreateInfoParagraph(string label, string value)
    {
        var para = new Paragraph();
        para.Margin = new Thickness(0, 2, 0, 2);
        
        var labelRun = new Run($"{label} ");
        labelRun.FontWeight = FontWeights.SemiBold;
        para.Inlines.Add(labelRun);
        
        para.Inlines.Add(new Run(value));
        
        return para;
    }

    /// <summary>
    /// إنشاء فقرة مجموع
    /// </summary>
    private Paragraph CreateTotalParagraph(string label, string value, bool isBold = false, double fontSize = 0)
    {
        var para = new Paragraph();
        para.Margin = new Thickness(0, 3, 0, 3);
        
        if (isBold)
            para.FontWeight = FontWeights.Bold;
        
        if (fontSize > 0)
            para.FontSize = fontSize;
        
        var labelRun = new Run($"{label} ");
        para.Inlines.Add(labelRun);
        para.Inlines.Add(new Run(value));
        
        return para;
    }

    /// <summary>
    /// إنشاء خلية جدول
    /// </summary>
    private TableCell CreateCell(string text, TextAlignment alignment)
    {
        var cell = new TableCell(new Paragraph(new Run(text)) { TextAlignment = alignment });
        cell.Padding = new Thickness(3);
        cell.BorderThickness = new Thickness(0, 0, 0, 0.5);
        cell.BorderBrush = Brushes.LightGray;
        return cell;
    }

    /// <summary>
    /// الحصول على حجم الخط المناسب
    /// </summary>
    private double GetFontSize()
    {
        return _settings.PaperSize switch
        {
            PaperSize.Receipt58mm => 9,
            PaperSize.Receipt80mm => 10,
            PaperSize.A5 => 11,
            PaperSize.A4 => 12,
            _ => 11
        };
    }

    /// <summary>
    /// طباعة المستند
    /// </summary>
    private bool PrintDocument(FlowDocument document, string documentName, bool showDialog)
    {
        var printDialog = new PrintDialog();
        
        // اختيار الطابعة المحددة في الإعدادات
        if (!string.IsNullOrEmpty(_settings.PrinterName))
        {
            try
            {
                var printServer = new LocalPrintServer();
                var printQueue = printServer.GetPrintQueues()
                    .FirstOrDefault(q => q.Name == _settings.PrinterName);
                
                if (printQueue != null)
                {
                    printDialog.PrintQueue = printQueue;
                }
            }
            catch { }
        }
        
        // عرض مربع حوار الطباعة إذا طُلب
        if (showDialog)
        {
            if (printDialog.ShowDialog() != true)
                return false;
        }
        
        // تحديد حجم الورق
        var pageSize = GetPageSize();
        document.PageWidth = pageSize.Width;
        document.PageHeight = pageSize.Height;
        
        // إنشاء DocumentPaginator
        var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
        paginator.PageSize = pageSize;
        
        // الطباعة
        printDialog.PrintDocument(paginator, documentName);
        
        return true;
    }

    /// <summary>
    /// الحصول على حجم الصفحة
    /// </summary>
    private Size GetPageSize()
    {
        return _settings.PaperSize switch
        {
            PaperSize.Receipt58mm => new Size(164, 1000), // 58mm width, variable height
            PaperSize.Receipt80mm => new Size(227, 1000), // 80mm width, variable height
            PaperSize.A5 => new Size(420, 595),           // A5
            PaperSize.A4 => new Size(595, 842),           // A4
            _ => new Size(595, 842)
        };
    }

    #endregion

    #region Static Methods

    /// <summary>
    /// الحصول على قائمة الطابعات المتاحة
    /// </summary>
    public static List<string> GetAvailablePrinters()
    {
        var printers = new List<string>();
        
        try
        {
            var printServer = new LocalPrintServer();
            foreach (var queue in printServer.GetPrintQueues())
            {
                printers.Add(queue.Name);
            }
        }
        catch { }
        
        return printers;
    }

    /// <summary>
    /// التحقق من وجود طابعة متصلة
    /// </summary>
    public static bool HasPrinter()
    {
        return GetAvailablePrinters().Count > 0;
    }

    #endregion
}
