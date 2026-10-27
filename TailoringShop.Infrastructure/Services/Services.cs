using Microsoft.EntityFrameworkCore;
using TailoringShop.Core.Entities;
using TailoringShop.Core.Enums;
using TailoringShop.Core.Interfaces.Repositories;
using TailoringShop.Core.Interfaces.Services;
using TailoringShop.Infrastructure.Data;

namespace TailoringShop.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly AppDbContext _context;

    public AuthService(IUserRepository userRepository, AppDbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null || !user.IsActive)
            return null;

        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        // تحديث وقت آخر تسجيل دخول مباشرة في قاعدة البيانات
        await _context.Database.ExecuteSqlRawAsync(
            "UPDATE Users SET LastLoginAt = {0} WHERE Id = {1}",
            DateTime.Now, user.Id);

        return await _userRepository.GetWithRolesAsync(user.Id);
    }

    public async Task<bool> ChangePasswordAsync(long userId, string oldPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        if (!VerifyPassword(oldPassword, user.PasswordHash))
            return false;

        user.PasswordHash = HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> HasPermissionAsync(long userId, string module, string permission)
    {
        var permissions = await _userRepository.GetUserPermissionsAsync(userId);
        return permissions.Contains($"{module}.{permission}");
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

public class CashboxService : ICashboxService
{
    private readonly AppDbContext _context;
    private readonly ICashboxRepository _cashboxRepository;

    public CashboxService(AppDbContext context, ICashboxRepository cashboxRepository)
    {
        _context = context;
        _cashboxRepository = cashboxRepository;
    }

    public async Task<Cashbox?> GetByIdAsync(long id)
    {
        return await _cashboxRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Cashbox>> GetAllActiveAsync()
    {
        return await _cashboxRepository.GetActiveCashboxesAsync();
    }

    public async Task UpdateBalanceAsync(long cashboxId, TransactionType type, decimal amount, 
        string? referenceType, long? referenceId, long? userId)
    {
        var cashbox = await _context.Cashboxes.FindAsync(cashboxId);
        if (cashbox == null) throw new Exception("Cashbox not found");

        var balanceBefore = cashbox.CurrentBalance;
        decimal balanceAfter;

        switch (type)
        {
            case TransactionType.Deposit:
            case TransactionType.Sale:
            case TransactionType.Refund:
                balanceAfter = balanceBefore + amount;
                break;
            case TransactionType.Withdrawal:
            case TransactionType.Expense:
                balanceAfter = balanceBefore - amount;
                break;
            default:
                balanceAfter = balanceBefore;
                break;
        }

        var transaction = new CashboxTransaction
        {
            CashboxId = cashboxId,
            Type = type,
            Amount = amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            CreatedBy = userId
        };

        _context.CashboxTransactions.Add(transaction);
        cashbox.CurrentBalance = balanceAfter;
        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetBalanceAsync(long cashboxId)
    {
        var cashbox = await _cashboxRepository.GetByIdAsync(cashboxId);
        return cashbox?.CurrentBalance ?? 0;
    }
}

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _context;
    private readonly IProductRepository _productRepository;

    public InventoryService(AppDbContext context, IProductRepository productRepository)
    {
        _context = context;
        _productRepository = productRepository;
    }

    public async Task<decimal> GetQuantityAsync(long productId, long storeId)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.StoreId == storeId);
        return inventory?.Quantity ?? 0;
    }

    public async Task UpdateQuantityAsync(long productId, long storeId, MovementType type, decimal quantity,
        decimal? unitCost, string? referenceType, long? referenceId, string? notes, long? userId)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.StoreId == storeId);

        if (inventory == null)
        {
            inventory = new Inventory
            {
                ProductId = productId,
                StoreId = storeId,
                Quantity = 0,
                ReservedQuantity = 0
            };
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
        }

        var quantityBefore = inventory.Quantity;
        decimal quantityAfter;

        switch (type)
        {
            case MovementType.Purchase:
            case MovementType.ReturnIn:
            case MovementType.AdjustmentIn:
                quantityAfter = quantityBefore + quantity;
                break;
            case MovementType.Sale:
            case MovementType.ReturnOut:
            case MovementType.AdjustmentOut:
            case MovementType.Damage:
                quantityAfter = quantityBefore - quantity;
                break;
            default:
                quantityAfter = quantityBefore;
                break;
        }

        var movement = new InventoryMovement
        {
            ProductId = productId,
            StoreId = storeId,
            Type = type,
            Quantity = quantity,
            QuantityBefore = quantityBefore,
            QuantityAfter = quantityAfter,
            UnitCost = unitCost,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Notes = notes,
            CreatedBy = userId
        };

        _context.InventoryMovements.Add(movement);
        inventory.Quantity = quantityAfter;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
    {
        return await _productRepository.GetLowStockProductsAsync();
    }
}

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        var today = DateTime.Today;

        // Get today's sales invoices with items
        var todaySalesInvoices = await _context.Invoices
            .Include(i => i.Items)
            .Where(i => i.Type == InvoiceType.Sale && i.Status == InvoiceStatus.Confirmed && i.InvoiceDate == today)
            .ToListAsync();

        // Calculate total sales
        decimal totalSalesToday = todaySalesInvoices.Sum(i => i.Total);
        
        // Calculate cost of goods sold (تكلفة البضائع المباعة)
        decimal costOfGoodsSold = todaySalesInvoices
            .SelectMany(i => i.Items)
            .Sum(item => item.CostPrice * item.Quantity);

        // Gross profit = Sales - Cost of Goods Sold
        decimal grossProfitToday = totalSalesToday - costOfGoodsSold;

        var data = new DashboardData
        {
            TotalSalesToday = totalSalesToday,

            SalesCountToday = todaySalesInvoices.Count,

            TotalExpensesToday = await _context.Expenses
                .Where(e => e.ExpenseDate == today)
                .SumAsync(e => e.Amount),

            GrossProfitToday = grossProfitToday,

            NewOrdersToday = await _context.TailoringOrders
                .CountAsync(o => o.OrderDate == today),

            PendingOrders = await _context.TailoringOrders
                .CountAsync(o => o.Status == OrderStatus.New),

            InProgressOrders = await _context.TailoringOrders
                .CountAsync(o => o.Status == OrderStatus.InProgress),

            ReadyOrders = await _context.TailoringOrders
                .CountAsync(o => o.Status == OrderStatus.Ready),

            LowStockProducts = await _context.Products
                .Where(p => p.IsActive)
                .Join(_context.Inventories, p => p.Id, i => i.ProductId, (p, i) => new { p, i })
                .CountAsync(x => x.i.Quantity <= x.p.MinStockLevel),

            CashboxSummaries = await _context.Cashboxes
                .Include(c => c.Currency)
                .Where(c => c.IsActive)
                .Select(c => new CashboxSummary
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type.ToString(),
                    CurrentBalance = c.CurrentBalance,
                    CurrencySymbol = c.Currency.Symbol,
                    TodaySales = c.Transactions
                        .Where(t => t.Type == TransactionType.Sale && t.CreatedAt.Date == today)
                        .Sum(t => t.Amount),
                    TodayExpenses = c.Transactions
                        .Where(t => t.Type == TransactionType.Expense && t.CreatedAt.Date == today)
                        .Sum(t => t.Amount)
                })
                .ToListAsync()
        };

        // Net Profit = Gross Profit - Expenses (صافي الربح = إجمالي الربح - المصروفات)
        data.NetProfitToday = data.GrossProfitToday - data.TotalExpensesToday;

        return data;
    }
}
