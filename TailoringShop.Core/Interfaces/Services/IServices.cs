using TailoringShop.Core.Entities;
using TailoringShop.Core.Enums;

namespace TailoringShop.Core.Interfaces.Services;

public interface IAuthService
{
    Task<User?> LoginAsync(string username, string password);
    Task<bool> ChangePasswordAsync(long userId, string oldPassword, string newPassword);
    Task<bool> HasPermissionAsync(long userId, string module, string permission);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public interface ICashboxService
{
    Task<Cashbox?> GetByIdAsync(long id);
    Task<IEnumerable<Cashbox>> GetAllActiveAsync();
    Task UpdateBalanceAsync(long cashboxId, TransactionType type, decimal amount, string? referenceType, long? referenceId, long? userId);
    Task<decimal> GetBalanceAsync(long cashboxId);
}

public interface IInventoryService
{
    Task<decimal> GetQuantityAsync(long productId, long storeId);
    Task UpdateQuantityAsync(long productId, long storeId, MovementType type, decimal quantity, decimal? unitCost, string? referenceType, long? referenceId, string? notes, long? userId);
    Task<IEnumerable<Product>> GetLowStockProductsAsync();
}

public interface IInvoiceService
{
    Task<Invoice> CreateSaleInvoiceAsync(Invoice invoice, IEnumerable<InvoiceItem> items, long userId);
    Task<Invoice> CreatePurchaseInvoiceAsync(Invoice invoice, IEnumerable<InvoiceItem> items, long userId);
    Task ConfirmInvoiceAsync(long invoiceId, long userId);
    Task CancelInvoiceAsync(long invoiceId, long userId);
    Task AddPaymentAsync(long invoiceId, decimal amount, PaymentMethod method, long cashboxId, long userId);
}

public interface ITailoringOrderService
{
    Task<TailoringOrder> CreateOrderAsync(TailoringOrder order, IEnumerable<TailoringOrderItem> items, long userId);
    Task ChangeStatusAsync(long orderId, OrderStatus newStatus, string? notes, long userId);
    Task AddPaymentAsync(long orderId, decimal amount, PaymentType type, PaymentMethod method, long cashboxId, long userId);
    Task<IEnumerable<TailoringOrder>> GetPendingOrdersAsync();
}

public interface IExpenseService
{
    Task<Expense> CreateExpenseAsync(Expense expense, long userId);
    Task<decimal> GetTotalExpensesTodayAsync();
    Task<decimal> GetTotalExpensesByDateRangeAsync(DateTime from, DateTime to);
}

public interface IDashboardService
{
    Task<DashboardData> GetDashboardDataAsync();
}

public class DashboardData
{
    public decimal TotalSalesToday { get; set; }
    public int SalesCountToday { get; set; }
    public decimal TotalExpensesToday { get; set; }
    public decimal GrossProfitToday { get; set; }  // إجمالي الربح (المبيعات - تكلفة البضائع)
    public decimal NetProfitToday { get; set; }    // صافي الربح (إجمالي الربح - المصروفات)
    public int NewOrdersToday { get; set; }
    public int PendingOrders { get; set; }
    public int InProgressOrders { get; set; }
    public int ReadyOrders { get; set; }
    public int LowStockProducts { get; set; }
    public IEnumerable<CashboxSummary> CashboxSummaries { get; set; } = new List<CashboxSummary>();
}

public class CashboxSummary
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public string CurrencySymbol { get; set; } = string.Empty;
    public decimal TodaySales { get; set; }
    public decimal TodayExpenses { get; set; }
}

public interface IBackupService
{
    Task<string> CreateBackupAsync(long userId);
    Task RestoreBackupAsync(string filePath, long userId);
    Task<IEnumerable<Backup>> GetBackupHistoryAsync();
}

public interface IReportService
{
    Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName);
    Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, string title);
}
