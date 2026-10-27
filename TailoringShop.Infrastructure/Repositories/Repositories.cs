using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TailoringShop.Core.Entities;
using TailoringShop.Core.Interfaces.Repositories;
using TailoringShop.Infrastructure.Data;

namespace TailoringShop.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(long id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(long id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate == null 
            ? await _dbSet.CountAsync() 
            : await _dbSet.CountAsync(predicate);
    }
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetWithRolesAsync(long id)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(long userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => $"{rp.Permission.Module}.{rp.Permission.Name}")
            .Distinct()
            .ToListAsync();
    }
}

public class CashboxRepository : Repository<Cashbox>, ICashboxRepository
{
    public CashboxRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Cashbox>> GetActiveCashboxesAsync()
    {
        return await _dbSet
            .Include(c => c.Currency)
            .Where(c => c.IsActive)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalBalanceAsync()
    {
        return await _dbSet.Where(c => c.IsActive).SumAsync(c => c.CurrentBalance);
    }
}

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context) { }

    public async Task<Customer?> GetByPhoneAsync(string phone)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Phone == phone);
    }

    public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm)
    {
        return await _dbSet
            .Where(c => c.FullName.Contains(searchTerm) || c.Phone.Contains(searchTerm))
            .ToListAsync();
    }
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<Product?> GetByBarcodeAsync(string barcode)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Barcode == barcode);
    }

    public async Task<Product?> GetBySKUAsync(string sku)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
    {
        return await _dbSet
            .Include(p => p.Inventories)
            .Where(p => p.IsActive && p.Inventories.Any(i => i.Quantity <= p.MinStockLevel))
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Currency)
            .Where(p => p.Name.Contains(searchTerm) || 
                       (p.SKU != null && p.SKU.Contains(searchTerm)) || 
                       (p.Barcode != null && p.Barcode.Contains(searchTerm)))
            .ToListAsync();
    }
}

public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(AppDbContext context) : base(context) { }

    public async Task<Invoice?> GetWithItemsAsync(long id)
    {
        return await _dbSet
            .Include(i => i.Items)
                .ThenInclude(item => item.Product)
            .Include(i => i.Payments)
            .Include(i => i.Customer)
            .Include(i => i.Cashbox)
            .Include(i => i.Currency)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<string> GenerateInvoiceNumberAsync(string prefix)
    {
        var lastNumber = await _dbSet
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.Id)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastNumber != null)
        {
            var numPart = lastNumber.Replace(prefix, "");
            if (int.TryParse(numPart, out int lastNum))
                nextNumber = lastNum + 1;
        }

        return $"{prefix}{nextNumber:D6}";
    }

    public async Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Include(i => i.Customer)
            .Include(i => i.Cashbox)
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }
}

public class TailoringOrderRepository : Repository<TailoringOrder>, ITailoringOrderRepository
{
    public TailoringOrderRepository(AppDbContext context) : base(context) { }

    public async Task<TailoringOrder?> GetWithDetailsAsync(long id)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .Include(o => o.StatusHistory)
            .Include(o => o.Customer)
            .Include(o => o.Cashbox)
            .Include(o => o.Currency)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<string> GenerateOrderNumberAsync(string prefix)
    {
        var lastNumber = await _dbSet
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.Id)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastNumber != null)
        {
            var numPart = lastNumber.Replace(prefix, "");
            if (int.TryParse(numPart, out int lastNum))
                nextNumber = lastNum + 1;
        }

        return $"{prefix}{nextNumber:D6}";
    }

    public async Task<IEnumerable<TailoringOrder>> GetByStatusAsync(Core.Enums.OrderStatus status)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TailoringOrder>> GetByCustomerAsync(long customerId)
    {
        return await _dbSet
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }
}

public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    public ExpenseRepository(AppDbContext context) : base(context) { }

    public async Task<string> GenerateExpenseNumberAsync(string prefix)
    {
        var lastNumber = await _dbSet
            .Where(e => e.ExpenseNumber.StartsWith(prefix))
            .OrderByDescending(e => e.Id)
            .Select(e => e.ExpenseNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastNumber != null)
        {
            var numPart = lastNumber.Replace(prefix, "");
            if (int.TryParse(numPart, out int lastNum))
                nextNumber = lastNum + 1;
        }

        return $"{prefix}{nextNumber:D6}";
    }

    public async Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Include(e => e.Category)
            .Include(e => e.Cashbox)
            .Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to)
            .SumAsync(e => e.Amount);
    }
}

public class SettingRepository : Repository<Setting>, ISettingRepository
{
    public SettingRepository(AppDbContext context) : base(context) { }

    public async Task<string?> GetValueAsync(string group, string key)
    {
        var setting = await _dbSet.FirstOrDefaultAsync(s => s.Group == group && s.Key == key);
        return setting?.Value;
    }

    public async Task SetValueAsync(string group, string key, string value)
    {
        var setting = await _dbSet.FirstOrDefaultAsync(s => s.Group == group && s.Key == key);
        if (setting != null)
        {
            setting.Value = value;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Setting>> GetByGroupAsync(string group)
    {
        return await _dbSet.Where(s => s.Group == group).ToListAsync();
    }
}
