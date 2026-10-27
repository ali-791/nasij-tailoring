using System.Linq.Expressions;
using TailoringShop.Core.Entities;

namespace TailoringShop.Core.Interfaces.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(long id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(long id);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetWithRolesAsync(long id);
    Task<IEnumerable<string>> GetUserPermissionsAsync(long userId);
}

public interface ICashboxRepository : IRepository<Cashbox>
{
    Task<IEnumerable<Cashbox>> GetActiveCashboxesAsync();
    Task<decimal> GetTotalBalanceAsync();
}

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByPhoneAsync(string phone);
    Task<IEnumerable<Customer>> SearchAsync(string searchTerm);
}

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByBarcodeAsync(string barcode);
    Task<Product?> GetBySKUAsync(string sku);
    Task<IEnumerable<Product>> GetLowStockProductsAsync();
    Task<IEnumerable<Product>> SearchAsync(string searchTerm);
}

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetWithItemsAsync(long id);
    Task<string> GenerateInvoiceNumberAsync(string prefix);
    Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime from, DateTime to);
}

public interface ITailoringOrderRepository : IRepository<TailoringOrder>
{
    Task<TailoringOrder?> GetWithDetailsAsync(long id);
    Task<string> GenerateOrderNumberAsync(string prefix);
    Task<IEnumerable<TailoringOrder>> GetByStatusAsync(Enums.OrderStatus status);
    Task<IEnumerable<TailoringOrder>> GetByCustomerAsync(long customerId);
}

public interface IExpenseRepository : IRepository<Expense>
{
    Task<string> GenerateExpenseNumberAsync(string prefix);
    Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<decimal> GetTotalByDateRangeAsync(DateTime from, DateTime to);
}

public interface ISettingRepository : IRepository<Setting>
{
    Task<string?> GetValueAsync(string group, string key);
    Task SetValueAsync(string group, string key, string value);
    Task<IEnumerable<Setting>> GetByGroupAsync(string group);
}
