using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using TailoringShop.Core.Entities;
using TailoringShop.Core.Interfaces.Repositories;
using TailoringShop.Core.Interfaces.Services;
using TailoringShop.Infrastructure.Data;
using TailoringShop.WPF.Views.Dialogs;

namespace TailoringShop.WPF.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public event Action<User>? LoginSuccessful;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "الرجاء إدخال اسم المستخدم وكلمة المرور";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var user = await _authService.LoginAsync(Username, Password);
            if (user != null)
            {
                App.CurrentUser = user;
                LoginSuccessful?.Invoke(user);
            }
            else
            {
                ErrorMessage = "اسم المستخدم أو كلمة المرور غير صحيحة";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"حدث خطأ: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableObject? _currentPage;

    [ObservableProperty]
    private string _currentPageTitle = "لوحة التحكم";

    [ObservableProperty]
    private User? _currentUser;

    public MainViewModel()
    {
        CurrentUser = App.CurrentUser;
        NavigateToDashboard();
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentPage = App.GetService<DashboardViewModel>();
        CurrentPageTitle = "لوحة التحكم";
    }

    [RelayCommand]
    private void NavigateToCustomers()
    {
        CurrentPage = App.GetService<CustomersViewModel>();
        CurrentPageTitle = "العملاء";
    }

    [RelayCommand]
    private void NavigateToProducts()
    {
        CurrentPage = App.GetService<ProductsViewModel>();
        CurrentPageTitle = "المنتجات";
    }

    [RelayCommand]
    private void NavigateToTailoringOrders()
    {
        CurrentPage = App.GetService<TailoringOrdersViewModel>();
        CurrentPageTitle = "طلبات الفصال";
    }

    [RelayCommand]
    private void NavigateToInvoices()
    {
        CurrentPage = App.GetService<InvoicesViewModel>();
        CurrentPageTitle = "الفواتير";
    }

    [RelayCommand]
    private void NavigateToExpenses()
    {
        CurrentPage = App.GetService<ExpensesViewModel>();
        CurrentPageTitle = "المصروفات";
    }

    [RelayCommand]
    private void NavigateToCashboxes()
    {
        CurrentPage = App.GetService<CashboxesViewModel>();
        CurrentPageTitle = "القاصات";
    }

    [RelayCommand]
    private void NavigateToReports()
    {
        CurrentPage = App.GetService<ReportsViewModel>();
        CurrentPageTitle = "التقارير";
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentPage = App.GetService<SettingsViewModel>();
        CurrentPageTitle = "الإعدادات";
    }

    [RelayCommand]
    private void NavigateToInvoiceSettings()
    {
        CurrentPage = App.GetService<InvoiceSettingsViewModel>();
        CurrentPageTitle = "تخصيص الفاتورة";
    }

    [RelayCommand]
    private void NavigateToUsers()
    {
        CurrentPage = App.GetService<UsersViewModel>();
        CurrentPageTitle = "المستخدمين";
    }

    [RelayCommand]
    private void Logout()
    {
        App.CurrentUser = null;
    }
}

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardService _dashboardService;

    [ObservableProperty]
    private decimal _totalSalesToday;

    [ObservableProperty]
    private int _salesCountToday;

    [ObservableProperty]
    private decimal _totalExpensesToday;

    [ObservableProperty]
    private decimal _grossProfitToday;  // إجمالي الربح

    [ObservableProperty]
    private decimal _netProfitToday;    // صافي الربح

    [ObservableProperty]
    private int _pendingOrders;

    [ObservableProperty]
    private int _inProgressOrders;

    [ObservableProperty]
    private int _readyOrders;

    [ObservableProperty]
    private int _lowStockProducts;

    [ObservableProperty]
    private IEnumerable<CashboxSummary> _cashboxSummaries = new List<CashboxSummary>();

    [ObservableProperty]
    private bool _isLoading;

    public DashboardViewModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        _ = LoadDashboardDataAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        IsLoading = true;
        try
        {
            var data = await _dashboardService.GetDashboardDataAsync();
            
            TotalSalesToday = data.TotalSalesToday;
            SalesCountToday = data.SalesCountToday;
            TotalExpensesToday = data.TotalExpensesToday;
            GrossProfitToday = data.GrossProfitToday;
            NetProfitToday = data.NetProfitToday;
            PendingOrders = data.PendingOrders;
            InProgressOrders = data.InProgressOrders;
            ReadyOrders = data.ReadyOrders;
            LowStockProducts = data.LowStockProducts;
            CashboxSummaries = data.CashboxSummaries;
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public partial class CustomersViewModel : ObservableObject
{
    private readonly ICustomerRepository _customerRepository;
    private List<Customer> _filteredCustomers = new();

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalCustomers;

    [ObservableProperty]
    private int _activeCustomers;

    // Pagination Properties
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalRecords;

    public CustomersViewModel(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
        _ = LoadCustomersAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1; // العودة للصفحة الأولى عند البحث
        _ = LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        IsLoading = true;
        try
        {
            var allCustomers = await _customerRepository.GetAllAsync();
            var query = allCustomers.AsQueryable();
            
            // تطبيق الفلتر
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                query = query.Where(c => 
                    c.FullName.ToLower().Contains(search) ||
                    (c.Phone != null && c.Phone.Contains(search)));
            }
            
            // ترتيب البيانات
            _filteredCustomers = query.OrderByDescending(c => c.CreatedAt).ToList();
            
            // إحصائيات عامة (من كل البيانات)
            TotalCustomers = allCustomers.Count();
            ActiveCustomers = allCustomers.Count(c => c.IsActive);
            
            // حساب الـ Pagination
            TotalRecords = _filteredCustomers.Count;
            TotalPages = TotalRecords == 0 ? 1 : (int)Math.Ceiling((double)TotalRecords / PageSize);
            
            // التأكد من أن الصفحة الحالية صالحة
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;
            
            // جلب البيانات للصفحة الحالية
            var pagedData = _filteredCustomers
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            
            Customers = new ObservableCollection<Customer>(pagedData);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PageChangedAsync()
    {
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task AddCustomerAsync()
    {
        var dialog = new CustomerDialog();
        if (dialog.ShowDialog() == true && dialog.Customer != null)
        {
            dialog.Customer.CreatedBy = App.CurrentUser?.Id;
            await _customerRepository.AddAsync(dialog.Customer);
            await LoadCustomersAsync();
        }
    }

    [RelayCommand]
    private async Task EditCustomerAsync()
    {
        if (SelectedCustomer == null) return;
        
        var dialog = new CustomerDialog(SelectedCustomer);
        if (dialog.ShowDialog() == true && dialog.Customer != null)
        {
            await _customerRepository.UpdateAsync(dialog.Customer);
            await LoadCustomersAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteCustomerAsync()
    {
        if (SelectedCustomer == null) return;
        
        var result = MessageBox.Show(
            $"هل تريد حذف العميل '{SelectedCustomer.FullName}'؟",
            "تأكيد الحذف",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            
        if (result == MessageBoxResult.Yes)
        {
            SelectedCustomer.IsActive = false;
            await _customerRepository.UpdateAsync(SelectedCustomer);
            await LoadCustomersAsync();
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        SearchText = string.Empty;
        CurrentPage = 1;
        await LoadCustomersAsync();
    }
}

public partial class ProductsViewModel : ObservableObject
{
    private readonly IProductRepository _productRepository;
    private readonly AppDbContext _context;
    private List<Product> _allProducts = new();
    private List<Product> _filteredProducts = new();

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalProducts;

    [ObservableProperty]
    private int _activeProducts;

    [ObservableProperty]
    private int _lowStockProducts;

    // Pagination Properties
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalRecords;

    public ProductsViewModel(IProductRepository productRepository, AppDbContext context)
    {
        _productRepository = productRepository;
        _context = context;
        _ = LoadDataAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        ApplyFilterAndPagination();
    }

    partial void OnSelectedCategoryChanged(Category? value)
    {
        CurrentPage = 1;
        ApplyFilterAndPagination();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            // Load categories with "All" option
            var cats = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            var categoriesWithAll = new List<Category> { new Category { Id = 0, Name = "الكل" } };
            categoriesWithAll.AddRange(cats);
            Categories = new ObservableCollection<Category>(categoriesWithAll);

            // Load products with includes
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Currency)
                .Include(p => p.Inventories)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            _allProducts = products;
            
            TotalProducts = _allProducts.Count;
            ActiveProducts = _allProducts.Count(p => p.IsActive);
            LowStockProducts = _allProducts.Count(p => p.IsActive && 
                p.Inventories.Any(i => i.Quantity <= p.MinStockLevel));
            
            ApplyFilterAndPagination();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilterAndPagination()
    {
        var filtered = _allProducts.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(p => 
                p.Name.ToLower().Contains(search) ||
                (p.SKU != null && p.SKU.ToLower().Contains(search)) ||
                (p.Barcode != null && p.Barcode.Contains(search)));
        }

        // فلترة حسب الفئة (تجاهل خيار "الكل" الذي Id=0)
        if (SelectedCategory != null && SelectedCategory.Id != 0)
        {
            filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);
        }
        
        _filteredProducts = filtered.ToList();
        
        // حساب الـ Pagination
        TotalRecords = _filteredProducts.Count;
        TotalPages = TotalRecords == 0 ? 1 : (int)Math.Ceiling((double)TotalRecords / PageSize);
        
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;
        
        // جلب البيانات للصفحة الحالية
        var pagedData = _filteredProducts
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
        
        Products = new ObservableCollection<Product>(pagedData);
    }

    [RelayCommand]
    private void PageChanged()
    {
        ApplyFilterAndPagination();
    }

    [RelayCommand]
    private async Task AddProductAsync()
    {
        var dialog = new ProductDialog(_context);
        if (dialog.ShowDialog() == true && dialog.Product != null)
        {
            dialog.Product.CreatedBy = App.CurrentUser?.Id;
            await _productRepository.AddAsync(dialog.Product);
            
            // Create inventory record for default store
            var defaultStore = await _context.Stores.FirstOrDefaultAsync(s => s.IsDefault);
            if (defaultStore != null)
            {
                var inventory = new Inventory
                {
                    ProductId = dialog.Product.Id,
                    StoreId = defaultStore.Id,
                    Quantity = 0,
                    ReservedQuantity = 0
                };
                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();
            }
            
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task EditProductAsync()
    {
        if (SelectedProduct == null) return;
        
        var dialog = new ProductDialog(_context, SelectedProduct);
        if (dialog.ShowDialog() == true && dialog.Product != null)
        {
            await _productRepository.UpdateAsync(dialog.Product);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null) return;
        
        var result = MessageBox.Show(
            $"هل تريد حذف المنتج '{SelectedProduct.Name}'؟",
            "تأكيد الحذف",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            
        if (result == MessageBoxResult.Yes)
        {
            SelectedProduct.IsActive = false;
            await _productRepository.UpdateAsync(SelectedProduct);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        SearchText = string.Empty;
        SelectedCategory = null;
        CurrentPage = 1;
        await LoadDataAsync();
    }

    [RelayCommand]
    private void ClearFilter()
    {
        SelectedCategory = null;
    }

    [RelayCommand]
    private async Task AddPurchaseInvoiceAsync()
    {
        var dialog = new PurchaseInvoiceDialog(_context);
        if (dialog.ShowDialog() == true && dialog.ProductsUpdated)
        {
            // Reload products since new ones may have been added
            await LoadDataAsync();
        }
    }
}

public partial class TailoringOrdersViewModel : ObservableObject
{
    private readonly ITailoringOrderRepository _orderRepository;
    private readonly AppDbContext _context;
    private List<TailoringOrder> _allOrders = new();
    private List<TailoringOrder> _filteredOrders = new();

    [ObservableProperty]
    private ObservableCollection<TailoringOrder> _orders = new();

    [ObservableProperty]
    private TailoringOrder? _selectedOrder;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string? _selectedStatus;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalOrders;

    [ObservableProperty]
    private int _pendingOrders;

    [ObservableProperty]
    private int _inProgressOrders;

    [ObservableProperty]
    private int _readyOrders;

    // Pagination Properties
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalRecords;

    public List<string> StatusList { get; } = new()
    {
        "الكل", "جديد", "قيد التنفيذ", "جاهز", "تم التسليم", "ملغي"
    };

    public TailoringOrdersViewModel(ITailoringOrderRepository orderRepository, AppDbContext context)
    {
        _orderRepository = orderRepository;
        _context = context;
        _ = LoadDataAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        ApplyFilterAndPagination();
    }

    partial void OnSelectedStatusChanged(string? value)
    {
        CurrentPage = 1;
        ApplyFilterAndPagination();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var orders = await _context.TailoringOrders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .Include(o => o.Currency)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            _allOrders = orders;
            
            TotalOrders = _allOrders.Count;
            PendingOrders = _allOrders.Count(o => o.Status == Core.Enums.OrderStatus.New);
            InProgressOrders = _allOrders.Count(o => o.Status == Core.Enums.OrderStatus.InProgress);
            ReadyOrders = _allOrders.Count(o => o.Status == Core.Enums.OrderStatus.Ready);
            
            ApplyFilterAndPagination();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilterAndPagination()
    {
        var filtered = _allOrders.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(o => 
                o.OrderNumber.ToLower().Contains(search) ||
                (o.Customer != null && o.Customer.FullName.ToLower().Contains(search)) ||
                (o.Customer != null && o.Customer.Phone != null && o.Customer.Phone.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(SelectedStatus) && SelectedStatus != "الكل")
        {
            var statusMap = new Dictionary<string, Core.Enums.OrderStatus>
            {
                { "جديد", Core.Enums.OrderStatus.New },
                { "قيد التنفيذ", Core.Enums.OrderStatus.InProgress },
                { "جاهز", Core.Enums.OrderStatus.Ready },
                { "تم التسليم", Core.Enums.OrderStatus.Delivered },
                { "ملغي", Core.Enums.OrderStatus.Cancelled }
            };

            if (statusMap.TryGetValue(SelectedStatus, out var status))
            {
                filtered = filtered.Where(o => o.Status == status);
            }
        }
        
        _filteredOrders = filtered.ToList();
        
        // حساب الـ Pagination
        TotalRecords = _filteredOrders.Count;
        TotalPages = TotalRecords == 0 ? 1 : (int)Math.Ceiling((double)TotalRecords / PageSize);
        
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;
        
        // جلب البيانات للصفحة الحالية
        var pagedData = _filteredOrders
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
        
        Orders = new ObservableCollection<TailoringOrder>(pagedData);
    }

    [RelayCommand]
    private void PageChanged()
    {
        ApplyFilterAndPagination();
    }

    [RelayCommand]
    private async Task AddOrderAsync()
    {
        var dialog = new TailoringOrderDialog(_context);
        if (dialog.ShowDialog() == true && dialog.Order != null)
        {
            dialog.Order.CreatedBy = App.CurrentUser?.Id;
            await _orderRepository.AddAsync(dialog.Order);
            
            // إضافة العربون للصندوق إذا كان أكبر من صفر
            if (dialog.Order.DepositAmount > 0)
            {
                var tailoringCashbox = await _context.Cashboxes
                    .FirstOrDefaultAsync(c => c.Type == Core.Enums.CashboxType.Tailoring && c.IsActive);
                
                if (tailoringCashbox == null)
                {
                    tailoringCashbox = await _context.Cashboxes
                        .FirstOrDefaultAsync(c => c.Type == Core.Enums.CashboxType.General && c.IsActive);
                }

                if (tailoringCashbox != null)
                {
                    var balanceBefore = tailoringCashbox.CurrentBalance;
                    var balanceAfter = balanceBefore + dialog.Order.DepositAmount;

                    var transaction = new CashboxTransaction
                    {
                        CashboxId = tailoringCashbox.Id,
                        Type = Core.Enums.TransactionType.Deposit,
                        Amount = dialog.Order.DepositAmount,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        ReferenceType = "TailoringOrderDeposit",
                        ReferenceId = dialog.Order.Id,
                        CreatedBy = App.CurrentUser?.Id
                    };

                    _context.CashboxTransactions.Add(transaction);
                    tailoringCashbox.CurrentBalance = balanceAfter;
                    await _context.SaveChangesAsync();
                }
            }
            
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task EditOrderAsync()
    {
        if (SelectedOrder == null) return;
        
        var dialog = new TailoringOrderDialog(_context, SelectedOrder);
        if (dialog.ShowDialog() == true && dialog.Order != null)
        {
            await _orderRepository.UpdateAsync(dialog.Order);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task ChangeStatusAsync(string newStatus)
    {
        if (SelectedOrder == null) return;

        var statusMap = new Dictionary<string, Core.Enums.OrderStatus>
        {
            { "قيد التنفيذ", Core.Enums.OrderStatus.InProgress },
            { "جاهز", Core.Enums.OrderStatus.Ready },
            { "تم التسليم", Core.Enums.OrderStatus.Delivered },
            { "ملغي", Core.Enums.OrderStatus.Cancelled }
        };

        if (statusMap.TryGetValue(newStatus, out var status))
        {
            // جلب الطلب من قاعدة البيانات بدون تتبع لضمان الحصول على القيم الفعلية
            var order = await _context.TailoringOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == SelectedOrder.Id);
            if (order == null) return;

            // إعادة إرفاق الطلب للتتبع
            var trackedOrder = await _context.TailoringOrders.FindAsync(order.Id);
            if (trackedOrder == null) return;

            var oldStatus = trackedOrder.Status;
            trackedOrder.Status = status;

            // عند التسليم - تسديد المبلغ المتبقي وإضافته للصندوق
            // نحسب المتبقي من TotalAmount - DepositAmount لأن RemainingAmount قد يكون قديماً
            var remainingToPay = order.TotalAmount - order.DepositAmount;
            
            if (status == Core.Enums.OrderStatus.Delivered && remainingToPay > 0)
            {
                // تحديث المبالغ في الطلب
                trackedOrder.DepositAmount = trackedOrder.TotalAmount;
                trackedOrder.RemainingAmount = 0;
                trackedOrder.PaymentStatus = Core.Enums.PaymentStatus.Paid;

                // البحث عن صندوق الفصال
                var tailoringCashbox = await _context.Cashboxes
                    .FirstOrDefaultAsync(c => c.Type == Core.Enums.CashboxType.Tailoring && c.IsActive);
                
                // إذا لم يوجد صندوق فصال، استخدم الصندوق العام
                if (tailoringCashbox == null)
                {
                    tailoringCashbox = await _context.Cashboxes
                        .FirstOrDefaultAsync(c => c.Type == Core.Enums.CashboxType.General && c.IsActive);
                }

                if (tailoringCashbox != null)
                {
                    var balanceBefore = tailoringCashbox.CurrentBalance;
                    var balanceAfter = balanceBefore + remainingToPay;

                    // إضافة معاملة إيداع للمبلغ المتبقي
                    var transaction = new CashboxTransaction
                    {
                        CashboxId = tailoringCashbox.Id,
                        Type = Core.Enums.TransactionType.Deposit,
                        Amount = remainingToPay,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        ReferenceType = "TailoringOrderDelivery",
                        ReferenceId = trackedOrder.Id,
                        CreatedBy = App.CurrentUser?.Id
                    };

                    _context.CashboxTransactions.Add(transaction);
                    tailoringCashbox.CurrentBalance = balanceAfter;
                }
            }

            // Add status history
            _context.TailoringOrderStatusHistory.Add(new TailoringOrderStatusHistory
            {
                OrderId = trackedOrder.Id,
                FromStatus = oldStatus,
                ToStatus = status,
                ChangedBy = App.CurrentUser?.Id,
                Notes = $"تغيير الحالة من {oldStatus} إلى {status}"
            });

            await _context.SaveChangesAsync();
            await LoadDataAsync();
            
            // رسالة تأكيد عند التسليم
            if (status == Core.Enums.OrderStatus.Delivered)
            {
                MessageBox.Show(
                    $"تم تسليم الطلب {order.OrderNumber} بنجاح!",
                    "تم التسليم",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    MessageBoxResult.OK,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            }
        }
    }

    [RelayCommand]
    private async Task DeleteOrderAsync()
    {
        if (SelectedOrder == null) return;
        
        var result = MessageBox.Show(
            $"هل تريد إلغاء الطلب '{SelectedOrder.OrderNumber}'؟",
            "تأكيد الإلغاء",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            
        if (result == MessageBoxResult.Yes)
        {
            SelectedOrder.Status = Core.Enums.OrderStatus.Cancelled;
            await _orderRepository.UpdateAsync(SelectedOrder);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        SearchText = string.Empty;
        SelectedStatus = null;
        CurrentPage = 1;
        await LoadDataAsync();
    }

    [RelayCommand]
    private void ClearFilter()
    {
        SelectedStatus = null;
    }
}

public partial class InvoicesViewModel : ObservableObject
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly AppDbContext _context;
    private List<Invoice> _allInvoices = new();
    private List<Invoice> _filteredInvoices = new();

    [ObservableProperty]
    private ObservableCollection<Invoice> _invoices = new();

    [ObservableProperty]
    private Invoice? _selectedInvoice;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string? _selectedPaymentStatus;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalInvoices;

    [ObservableProperty]
    private decimal _totalSales;

    [ObservableProperty]
    private decimal _totalPaid;

    [ObservableProperty]
    private decimal _totalRemaining;

    // Pagination Properties
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalRecords;

    public List<string> PaymentStatusList { get; } = new() { "الكل", "مدفوع", "مدفوع جزئياً", "غير مدفوع" };

    public InvoicesViewModel(IInvoiceRepository invoiceRepository, AppDbContext context)
    {
        _invoiceRepository = invoiceRepository;
        _context = context;
        _ = LoadDataAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        ApplyFilterAndPagination();
    }

    partial void OnSelectedPaymentStatusChanged(string? value)
    {
        CurrentPage = 1;
        ApplyFilterAndPagination();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            // Load only SALE invoices
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items)
                .Include(i => i.Currency)
                .Where(i => i.Type == Core.Enums.InvoiceType.Sale)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            _allInvoices = invoices;
            
            TotalInvoices = _allInvoices.Count;
            TotalSales = _allInvoices.Sum(i => i.Total);
            TotalPaid = _allInvoices.Sum(i => i.PaidAmount);
            TotalRemaining = _allInvoices.Sum(i => i.RemainingAmount);
            
            ApplyFilterAndPagination();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilterAndPagination()
    {
        var filtered = _allInvoices.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(i => 
                i.InvoiceNumber.ToLower().Contains(search) ||
                (i.Customer != null && i.Customer.FullName.ToLower().Contains(search)) ||
                (i.Customer != null && i.Customer.Phone != null && i.Customer.Phone.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(SelectedPaymentStatus) && SelectedPaymentStatus != "الكل")
        {
            var statusMap = new Dictionary<string, Core.Enums.PaymentStatus>
            {
                { "مدفوع", Core.Enums.PaymentStatus.Paid },
                { "مدفوع جزئياً", Core.Enums.PaymentStatus.Partial },
                { "غير مدفوع", Core.Enums.PaymentStatus.Unpaid }
            };

            if (statusMap.TryGetValue(SelectedPaymentStatus, out var status))
            {
                filtered = filtered.Where(i => i.PaymentStatus == status);
            }
        }
        
        _filteredInvoices = filtered.ToList();
        
        // حساب الـ Pagination
        TotalRecords = _filteredInvoices.Count;
        TotalPages = TotalRecords == 0 ? 1 : (int)Math.Ceiling((double)TotalRecords / PageSize);
        
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;
        
        // جلب البيانات للصفحة الحالية
        var pagedData = _filteredInvoices
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
        
        Invoices = new ObservableCollection<Invoice>(pagedData);
    }

    [RelayCommand]
    private void PageChanged()
    {
        ApplyFilterAndPagination();
    }

    [RelayCommand]
    private async Task AddInvoiceAsync()
    {
        var dialog = new InvoiceDialog(_context);
        if (dialog.ShowDialog() == true && dialog.Invoice != null)
        {
            dialog.Invoice.CreatedBy = App.CurrentUser?.Id;
            dialog.Invoice.Type = Core.Enums.InvoiceType.Sale;
            await _invoiceRepository.AddAsync(dialog.Invoice);
            
            // خصم الكمية من المخزون
            foreach (var item in dialog.Invoice.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.StoreId == dialog.Invoice.StoreId);
                
                if (inventory != null)
                {
                    inventory.Quantity -= item.Quantity;
                    if (inventory.Quantity < 0) inventory.Quantity = 0;
                }
            }
            
            // إضافة المبلغ المدفوع للصندوق
            if (dialog.Invoice.PaidAmount > 0 && dialog.Invoice.CashboxId > 0)
            {
                var cashbox = await _context.Cashboxes.FindAsync(dialog.Invoice.CashboxId);
                if (cashbox != null)
                {
                    var balanceBefore = cashbox.CurrentBalance;
                    var balanceAfter = balanceBefore + dialog.Invoice.PaidAmount;
                    
                    // تسجيل الحركة
                    var transaction = new CashboxTransaction
                    {
                        CashboxId = cashbox.Id,
                        Type = Core.Enums.TransactionType.Sale,
                        Amount = dialog.Invoice.PaidAmount,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        ReferenceType = "SaleInvoice",
                        ReferenceId = dialog.Invoice.Id,
                        CreatedBy = App.CurrentUser?.Id
                    };
                    
                    _context.CashboxTransactions.Add(transaction);
                    cashbox.CurrentBalance = balanceAfter;
                }
            }
            
            await _context.SaveChangesAsync();
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task EditInvoiceAsync()
    {
        if (SelectedInvoice == null) return;
        
        var dialog = new InvoiceDialog(_context, SelectedInvoice);
        if (dialog.ShowDialog() == true && dialog.Invoice != null)
        {
            await _invoiceRepository.UpdateAsync(dialog.Invoice);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteInvoiceAsync()
    {
        if (SelectedInvoice == null) return;
        
        var result = MessageBox.Show(
            $"هل تريد إلغاء الفاتورة '{SelectedInvoice.InvoiceNumber}'؟",
            "تأكيد الإلغاء",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            
        if (result == MessageBoxResult.Yes)
        {
            SelectedInvoice.Status = Core.Enums.InvoiceStatus.Cancelled;
            await _invoiceRepository.UpdateAsync(SelectedInvoice);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        SearchText = string.Empty;
        SelectedPaymentStatus = null;
        CurrentPage = 1;
        await LoadDataAsync();
    }
}

public partial class ExpensesViewModel : ObservableObject
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly AppDbContext _context;
    private List<Expense> _allExpenses = new();
    private List<Expense> _filteredExpenses = new();

    [ObservableProperty]
    private ObservableCollection<Expense> _expenses = new();

    [ObservableProperty]
    private Expense? _selectedExpense;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ExpenseCategory? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<ExpenseCategory> _categories = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalExpenses;

    [ObservableProperty]
    private decimal _todayExpenses;

    [ObservableProperty]
    private decimal _monthExpenses;

    [ObservableProperty]
    private decimal _yearExpenses;

    // Pagination Properties
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalRecords;

    public ExpensesViewModel(IExpenseRepository expenseRepository, AppDbContext context)
    {
        _expenseRepository = expenseRepository;
        _context = context;
        _ = LoadDataAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        ApplyFilterAndPagination();
    }

    partial void OnSelectedCategoryChanged(ExpenseCategory? value)
    {
        CurrentPage = 1;
        ApplyFilterAndPagination();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            // Load categories
            var cats = await _context.ExpenseCategories.Where(c => c.IsActive).ToListAsync();
            Categories = new ObservableCollection<ExpenseCategory>(cats);

            // Load expenses
            var expenses = await _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.Cashbox)
                .Include(e => e.Currency)
                .OrderByDescending(e => e.ExpenseDate)
                .ThenByDescending(e => e.CreatedAt)
                .ToListAsync();

            _allExpenses = expenses;

            // Calculate stats
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);

            TotalExpenses = _allExpenses.Count;
            TodayExpenses = _allExpenses.Where(e => e.ExpenseDate.Date == today).Sum(e => e.Amount);
            MonthExpenses = _allExpenses.Where(e => e.ExpenseDate >= startOfMonth).Sum(e => e.Amount);
            YearExpenses = _allExpenses.Where(e => e.ExpenseDate >= startOfYear).Sum(e => e.Amount);

            ApplyFilterAndPagination();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilterAndPagination()
    {
        var filtered = _allExpenses.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(e =>
                e.Description.ToLower().Contains(search) ||
                e.ExpenseNumber.ToLower().Contains(search) ||
                (e.ReferenceNumber != null && e.ReferenceNumber.ToLower().Contains(search)));
        }

        if (SelectedCategory != null)
        {
            filtered = filtered.Where(e => e.CategoryId == SelectedCategory.Id);
        }

        _filteredExpenses = filtered.ToList();
        
        // حساب الـ Pagination
        TotalRecords = _filteredExpenses.Count;
        TotalPages = TotalRecords == 0 ? 1 : (int)Math.Ceiling((double)TotalRecords / PageSize);
        
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;
        
        // جلب البيانات للصفحة الحالية
        var pagedData = _filteredExpenses
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        Expenses = new ObservableCollection<Expense>(pagedData);
    }

    [RelayCommand]
    private void PageChanged()
    {
        ApplyFilterAndPagination();
    }

    [RelayCommand]
    private async Task AddExpenseAsync()
    {
        var dialog = new ExpenseDialog(_context);
        if (dialog.ShowDialog() == true && dialog.Expense != null)
        {
            dialog.Expense.CreatedBy = App.CurrentUser?.Id;
            await _expenseRepository.AddAsync(dialog.Expense);
            
            // خصم المبلغ من الصندوق
            if (dialog.Expense.CashboxId > 0)
            {
                var cashbox = await _context.Cashboxes.FindAsync(dialog.Expense.CashboxId);
                if (cashbox != null)
                {
                    var balanceBefore = cashbox.CurrentBalance;
                    var balanceAfter = balanceBefore - dialog.Expense.Amount;
                    
                    // تسجيل الحركة
                    var transaction = new CashboxTransaction
                    {
                        CashboxId = cashbox.Id,
                        Type = Core.Enums.TransactionType.Expense,
                        Amount = dialog.Expense.Amount,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        ReferenceType = "Expense",
                        ReferenceId = dialog.Expense.Id,
                        CreatedBy = App.CurrentUser?.Id
                    };
                    
                    _context.CashboxTransactions.Add(transaction);
                    cashbox.CurrentBalance = balanceAfter;
                    await _context.SaveChangesAsync();
                }
            }
            
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task EditExpenseAsync()
    {
        if (SelectedExpense == null) return;

        var dialog = new ExpenseDialog(_context, SelectedExpense);
        if (dialog.ShowDialog() == true && dialog.Expense != null)
        {
            await _expenseRepository.UpdateAsync(dialog.Expense);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteExpenseAsync()
    {
        if (SelectedExpense == null) return;

        var result = MessageBox.Show(
            $"هل تريد حذف المصروف '{SelectedExpense.Description}'؟\nسيتم إرجاع المبلغ للصندوق.",
            "تأكيد الحذف",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

        if (result == MessageBoxResult.Yes)
        {
            // إرجاع المبلغ للصندوق
            if (SelectedExpense.CashboxId > 0)
            {
                var cashbox = await _context.Cashboxes.FindAsync(SelectedExpense.CashboxId);
                if (cashbox != null)
                {
                    var balanceBefore = cashbox.CurrentBalance;
                    var balanceAfter = balanceBefore + SelectedExpense.Amount;
                    
                    // تسجيل حركة الإرجاع
                    var transaction = new CashboxTransaction
                    {
                        CashboxId = cashbox.Id,
                        Type = Core.Enums.TransactionType.Refund,
                        Amount = SelectedExpense.Amount,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        ReferenceType = "ExpenseRefund",
                        ReferenceId = SelectedExpense.Id,
                        CreatedBy = App.CurrentUser?.Id
                    };
                    
                    _context.CashboxTransactions.Add(transaction);
                    cashbox.CurrentBalance = balanceAfter;
                    await _context.SaveChangesAsync();
                }
            }
            
            await _expenseRepository.DeleteAsync(SelectedExpense);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        SearchText = string.Empty;
        SelectedCategory = null;
        CurrentPage = 1;
        await LoadDataAsync();
    }

    [RelayCommand]
    private void ClearFilter()
    {
        SelectedCategory = null;
    }
}

public partial class CashboxesViewModel : ObservableObject
{
    private readonly ICashboxRepository _cashboxRepository;
    private readonly AppDbContext _context;

    [ObservableProperty]
    private ObservableCollection<Cashbox> _cashboxes = new();

    [ObservableProperty]
    private Cashbox? _selectedCashbox;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private decimal _totalBalance;

    [ObservableProperty]
    private int _activeCashboxes;

    public CashboxesViewModel(ICashboxRepository cashboxRepository, AppDbContext context)
    {
        _cashboxRepository = cashboxRepository;
        _context = context;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var cashboxes = await _context.Cashboxes
                .Include(c => c.Currency)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            Cashboxes = new ObservableCollection<Cashbox>(cashboxes);

            TotalBalance = cashboxes.Sum(c => c.CurrentBalance);
            ActiveCashboxes = cashboxes.Count;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddCashboxAsync()
    {
        var dialog = new CashboxDialog(_context);
        if (dialog.ShowDialog() == true && dialog.Cashbox != null)
        {
            dialog.Cashbox.CreatedBy = App.CurrentUser?.Id;
            await _cashboxRepository.AddAsync(dialog.Cashbox);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task EditCashboxAsync()
    {
        if (SelectedCashbox == null) return;

        var dialog = new CashboxDialog(_context, SelectedCashbox);
        if (dialog.ShowDialog() == true && dialog.Cashbox != null)
        {
            await _cashboxRepository.UpdateAsync(dialog.Cashbox);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task DepositAsync()
    {
        if (SelectedCashbox == null) return;

        var dialog = new CashboxTransactionDialog(_context, SelectedCashbox, Core.Enums.TransactionType.Deposit);
        if (dialog.ShowDialog() == true)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task WithdrawAsync()
    {
        if (SelectedCashbox == null) return;

        var dialog = new CashboxTransactionDialog(_context, SelectedCashbox, Core.Enums.TransactionType.Withdrawal);
        if (dialog.ShowDialog() == true)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteCashboxAsync()
    {
        if (SelectedCashbox == null) return;

        if (SelectedCashbox.CurrentBalance != 0)
        {
            MessageBox.Show(
                "لا يمكن حذف صندوق يحتوي على رصيد. يرجى تفريغ الصندوق أولاً.",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }

        var result = MessageBox.Show(
            $"هل تريد حذف الصندوق '{SelectedCashbox.Name}'؟",
            "تأكيد الحذف",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

        if (result == MessageBoxResult.Yes)
        {
            SelectedCashbox.IsActive = false;
            await _cashboxRepository.UpdateAsync(SelectedCashbox);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }
}

public partial class ReportsViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private DateTime _fromDate = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    private DateTime _toDate = DateTime.Today;

    [ObservableProperty]
    private bool _isLoading;

    // Sales Report
    [ObservableProperty]
    private decimal _totalSales;

    [ObservableProperty]
    private int _salesCount;

    [ObservableProperty]
    private decimal _totalPaid;

    [ObservableProperty]
    private decimal _totalRemaining;

    // Expenses Report
    [ObservableProperty]
    private decimal _totalExpenses;

    [ObservableProperty]
    private int _expensesCount;

    // Profit
    [ObservableProperty]
    private decimal _grossProfit;

    [ObservableProperty]
    private decimal _netProfit;

    // Tailoring Orders
    [ObservableProperty]
    private int _totalOrders;

    [ObservableProperty]
    private int _completedOrders;

    [ObservableProperty]
    private int _pendingOrders;

    [ObservableProperty]
    private decimal _tailoringRevenue;

    // Products
    [ObservableProperty]
    private int _totalProducts;

    [ObservableProperty]
    private int _lowStockCount;

    // Customers
    [ObservableProperty]
    private int _totalCustomers;

    [ObservableProperty]
    private int _newCustomers;

    // Top Products
    [ObservableProperty]
    private ObservableCollection<ProductSalesReport> _topProducts = new();

    // Expense by Category
    [ObservableProperty]
    private ObservableCollection<ExpenseCategoryReport> _expensesByCategory = new();

    public ReportsViewModel(AppDbContext context)
    {
        _context = context;
        _ = LoadReportAsync();
    }

    partial void OnFromDateChanged(DateTime value)
    {
        _ = LoadReportAsync();
    }

    partial void OnToDateChanged(DateTime value)
    {
        _ = LoadReportAsync();
    }

    [RelayCommand]
    private async Task LoadReportAsync()
    {
        IsLoading = true;
        try
        {
            var from = FromDate.Date;
            var to = ToDate.Date.AddDays(1).AddSeconds(-1);

            // Sales Report
            var sales = await _context.Invoices
                .Where(i => i.Type == Core.Enums.InvoiceType.Sale && 
                           i.Status != Core.Enums.InvoiceStatus.Cancelled &&
                           i.InvoiceDate >= from && i.InvoiceDate <= to)
                .ToListAsync();

            TotalSales = sales.Sum(s => s.Total);
            SalesCount = sales.Count;
            TotalPaid = sales.Sum(s => s.PaidAmount);
            TotalRemaining = sales.Sum(s => s.RemainingAmount);

            // Expenses Report
            var expenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to)
                .ToListAsync();

            TotalExpenses = expenses.Sum(e => e.Amount);
            ExpensesCount = expenses.Count;

            // Expenses by Category
            var expensesByCategory = expenses
                .GroupBy(e => e.Category?.Name ?? "غير مصنف")
                .Select(g => new ExpenseCategoryReport
                {
                    CategoryName = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(e => e.Amount)
                .ToList();
            ExpensesByCategory = new ObservableCollection<ExpenseCategoryReport>(expensesByCategory);

            // Profit Calculation
            var salesCost = await _context.InvoiceItems
                .Include(i => i.Invoice)
                .Where(i => i.Invoice.Type == Core.Enums.InvoiceType.Sale &&
                           i.Invoice.Status != Core.Enums.InvoiceStatus.Cancelled &&
                           i.Invoice.InvoiceDate >= from && i.Invoice.InvoiceDate <= to)
                .SumAsync(i => i.CostPrice * i.Quantity);

            GrossProfit = TotalSales - salesCost;
            NetProfit = GrossProfit - TotalExpenses;

            // Tailoring Orders
            var orders = await _context.TailoringOrders
                .Where(o => o.OrderDate >= from && o.OrderDate <= to)
                .ToListAsync();

            TotalOrders = orders.Count;
            CompletedOrders = orders.Count(o => o.Status == Core.Enums.OrderStatus.Delivered);
            PendingOrders = orders.Count(o => o.Status != Core.Enums.OrderStatus.Delivered && 
                                              o.Status != Core.Enums.OrderStatus.Cancelled);
            TailoringRevenue = orders.Where(o => o.Status != Core.Enums.OrderStatus.Cancelled)
                                    .Sum(o => o.DepositAmount);

            // Products Stats
            var products = await _context.Products
                .Include(p => p.Inventories)
                .Where(p => p.IsActive)
                .ToListAsync();

            TotalProducts = products.Count;
            LowStockCount = products.Count(p => p.Inventories.Any(i => i.Quantity <= p.MinStockLevel));

            // Top Selling Products
            var topProducts = await _context.InvoiceItems
                .Include(i => i.Product)
                .Include(i => i.Invoice)
                .Where(i => i.Invoice.Type == Core.Enums.InvoiceType.Sale &&
                           i.Invoice.Status != Core.Enums.InvoiceStatus.Cancelled &&
                           i.Invoice.InvoiceDate >= from && i.Invoice.InvoiceDate <= to)
                .GroupBy(i => new { i.ProductId, i.Product.Name })
                .Select(g => new ProductSalesReport
                {
                    ProductName = g.Key.Name,
                    QuantitySold = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.Total)
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(5)
                .ToListAsync();

            TopProducts = new ObservableCollection<ProductSalesReport>(topProducts);

            // Customers
            TotalCustomers = await _context.Customers.CountAsync(c => c.IsActive);
            NewCustomers = await _context.Customers
                .CountAsync(c => c.IsActive && c.CreatedAt >= from && c.CreatedAt <= to);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SetToday()
    {
        FromDate = DateTime.Today;
        ToDate = DateTime.Today;
    }

    [RelayCommand]
    private void SetThisWeek()
    {
        var today = DateTime.Today;
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Saturday)) % 7;
        FromDate = today.AddDays(-diff);
        ToDate = DateTime.Today;
    }

    [RelayCommand]
    private void SetThisMonth()
    {
        var today = DateTime.Today;
        FromDate = new DateTime(today.Year, today.Month, 1);
        ToDate = DateTime.Today;
    }

    [RelayCommand]
    private void SetThisYear()
    {
        var today = DateTime.Today;
        FromDate = new DateTime(today.Year, 1, 1);
        ToDate = DateTime.Today;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadReportAsync();
    }
}

// Helper classes for reports
public class ProductSalesReport
{
    public string ProductName { get; set; } = string.Empty;
    public decimal QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class ExpenseCategoryReport
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppDbContext _context;
    private long _settingsId;

    [ObservableProperty]
    private string _shopName = string.Empty;

    [ObservableProperty]
    private string _shopPhone = string.Empty;

    [ObservableProperty]
    private string _shopAddress = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    // Product Categories
    [ObservableProperty]
    private ObservableCollection<Category> _productCategories = new();

    [ObservableProperty]
    private Category? _selectedProductCategory;

    // Expense Categories
    [ObservableProperty]
    private ObservableCollection<ExpenseCategory> _expenseCategories = new();

    [ObservableProperty]
    private ExpenseCategory? _selectedExpenseCategory;

    // Stores
    [ObservableProperty]
    private ObservableCollection<Store> _stores = new();

    [ObservableProperty]
    private Store? _selectedStore;

    [ObservableProperty]
    private string _newStoreName = string.Empty;

    // New Category Names
    [ObservableProperty]
    private string _newProductCategoryName = string.Empty;

    [ObservableProperty]
    private string _newExpenseCategoryName = string.Empty;

    public SettingsViewModel(AppDbContext context)
    {
        _context = context;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            // Load shop settings (use AsNoTracking to avoid tracking issues)
            var settings = await _context.SystemSettings.AsNoTracking().FirstOrDefaultAsync();
            if (settings != null)
            {
                _settingsId = settings.Id;
                ShopName = settings.ShopName;
                ShopPhone = settings.ShopPhone ?? string.Empty;
                ShopAddress = settings.ShopAddress ?? string.Empty;
            }

            // Load product categories
            var productCats = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
            ProductCategories = new ObservableCollection<Category>(productCats);

            // Load expense categories
            var expenseCats = await _context.ExpenseCategories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
            ExpenseCategories = new ObservableCollection<ExpenseCategory>(expenseCats);

            // Load stores
            var stores = await _context.Stores.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            Stores = new ObservableCollection<Store>(stores);

            // Load users
            await LoadUsersAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveShopSettingsAsync()
    {
        try
        {
            // Get the tracked entity or attach it
            var settings = await _context.SystemSettings.FindAsync(_settingsId);
            if (settings == null)
            {
                settings = new SystemSettings();
                _context.SystemSettings.Add(settings);
            }

            settings.ShopName = ShopName;
            settings.ShopPhone = ShopPhone;
            settings.ShopAddress = ShopAddress;

            await _context.SaveChangesAsync();
            _settingsId = settings.Id;

            MessageBox.Show(
                "تم حفظ الإعدادات بنجاح",
                "تم الحفظ",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
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

    [RelayCommand]
    private async Task AddProductCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewProductCategoryName)) return;

        var category = new Category
        {
            Name = NewProductCategoryName.Trim(),
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        ProductCategories.Add(category);
        NewProductCategoryName = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteProductCategoryAsync()
    {
        if (SelectedProductCategory == null) return;

        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == SelectedProductCategory.Id);
        if (hasProducts)
        {
            MessageBox.Show(
                "لا يمكن حذف هذه الفئة لأنها مرتبطة بمنتجات",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }

        var result = MessageBox.Show(
            $"هل تريد حذف الفئة '{SelectedProductCategory.Name}'؟",
            "تأكيد الحذف",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

        if (result == MessageBoxResult.Yes)
        {
            var catToDelete = await _context.Categories.FindAsync(SelectedProductCategory.Id);
            if (catToDelete != null)
            {
                _context.Categories.Remove(catToDelete);
                await _context.SaveChangesAsync();
                ProductCategories.Remove(SelectedProductCategory);
            }
        }
    }

    [RelayCommand]
    private async Task AddExpenseCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewExpenseCategoryName)) return;

        var category = new ExpenseCategory
        {
            Name = NewExpenseCategoryName.Trim(),
            IsActive = true
        };

        _context.ExpenseCategories.Add(category);
        await _context.SaveChangesAsync();

        ExpenseCategories.Add(category);
        NewExpenseCategoryName = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteExpenseCategoryAsync()
    {
        if (SelectedExpenseCategory == null) return;

        var hasExpenses = await _context.Expenses.AnyAsync(e => e.CategoryId == SelectedExpenseCategory.Id);
        if (hasExpenses)
        {
            MessageBox.Show(
                "لا يمكن حذف هذه الفئة لأنها مرتبطة بمصروفات",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }

        var result = MessageBox.Show(
            $"هل تريد حذف الفئة '{SelectedExpenseCategory.Name}'؟",
            "تأكيد الحذف",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

        if (result == MessageBoxResult.Yes)
        {
            var catToDelete = await _context.ExpenseCategories.FindAsync(SelectedExpenseCategory.Id);
            if (catToDelete != null)
            {
                _context.ExpenseCategories.Remove(catToDelete);
                await _context.SaveChangesAsync();
                ExpenseCategories.Remove(SelectedExpenseCategory);
            }
        }
    }

    [RelayCommand]
    private async Task AddStoreAsync()
    {
        if (string.IsNullOrWhiteSpace(NewStoreName)) return;

        var store = new Store
        {
            Name = NewStoreName.Trim(),
            IsActive = true
        };

        _context.Stores.Add(store);
        await _context.SaveChangesAsync();

        Stores.Add(store);
        NewStoreName = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteStoreAsync()
    {
        if (SelectedStore == null) return;

        var hasInventory = await _context.Inventories.AnyAsync(i => i.StoreId == SelectedStore.Id);
        if (hasInventory)
        {
            MessageBox.Show(
                "لا يمكن حذف هذا المخزن لأنه يحتوي على منتجات",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }

        var result = MessageBox.Show(
            $"هل تريد حذف المخزن '{SelectedStore.Name}'؟",
            "تأكيد الحذف",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

        if (result == MessageBoxResult.Yes)
        {
            var storeToDelete = await _context.Stores.FindAsync(SelectedStore.Id);
            if (storeToDelete != null)
            {
                storeToDelete.IsActive = false;
                await _context.SaveChangesAsync();
                Stores.Remove(SelectedStore);
            }
        }
    }

    public async void ChangePassword(string currentPassword, string newPassword)
    {
        try
        {
            if (App.CurrentUser == null)
            {
                MessageBox.Show(
                    "لم يتم تسجيل الدخول",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            var user = await _context.Users.FindAsync(App.CurrentUser.Id);
            if (user == null)
            {
                MessageBox.Show(
                    "لم يتم العثور على المستخدم",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            // Verify current password
            if (user.PasswordHash != currentPassword)
            {
                MessageBox.Show(
                    "كلمة المرور الحالية غير صحيحة",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            // Update password
            user.PasswordHash = newPassword;
            await _context.SaveChangesAsync();

            MessageBox.Show(
                "تم تغيير كلمة المرور بنجاح",
                "تم",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
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

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
        await LoadUsersAsync();
    }

    // ========== User Management ==========
    [ObservableProperty]
    private ObservableCollection<User> _users = new();

    [ObservableProperty]
    private User? _selectedUser;

    [ObservableProperty]
    private string _newUsername = string.Empty;

    [ObservableProperty]
    private string _newUserPassword = string.Empty;

    [ObservableProperty]
    private string _newUserFullName = string.Empty;

    [ObservableProperty]
    private bool _newUserIsAdmin = false;

    public async Task LoadUsersAsync()
    {
        var users = await _context.Users.AsNoTracking().Where(u => u.IsActive).OrderBy(u => u.Username).ToListAsync();
        Users = new ObservableCollection<User>(users);
    }

    [RelayCommand]
    private async Task AddUserAsync()
    {
        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewUserPassword))
        {
            MessageBox.Show(
                "يرجى إدخال اسم المستخدم وكلمة المرور",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }

        // Check if username exists
        var existingUser = await _context.Users.AnyAsync(u => u.Username.ToLower() == NewUsername.ToLower().Trim());
        if (existingUser)
        {
            MessageBox.Show(
                "اسم المستخدم موجود مسبقاً",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }

        var user = new User
        {
            Username = NewUsername.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewUserPassword),
            FullName = string.IsNullOrWhiteSpace(NewUserFullName) ? NewUsername.Trim() : NewUserFullName.Trim(),
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assign Admin role if checked
        if (NewUserIsAdmin)
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                _context.Set<UserRole>().Add(new UserRole { UserId = user.Id, RoleId = adminRole.Id });
                await _context.SaveChangesAsync();
            }
        }

        Users.Add(user);
        NewUsername = string.Empty;
        NewUserPassword = string.Empty;
        NewUserFullName = string.Empty;
        NewUserIsAdmin = false;

        MessageBox.Show(
            "تم إضافة المستخدم بنجاح",
            "تم",
            MessageBoxButton.OK,
            MessageBoxImage.Information,
            MessageBoxResult.OK,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
    }

    [RelayCommand]
    private async Task DeleteUserAsync()
    {
        if (SelectedUser == null) return;

        if (SelectedUser.Id == App.CurrentUser?.Id)
        {
            MessageBox.Show(
                "لا يمكنك حذف حسابك الخاص",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }

        var result = MessageBox.Show(
            $"هل تريد حذف المستخدم '{SelectedUser.Username}'؟",
            "تأكيد الحذف",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

        if (result == MessageBoxResult.Yes)
        {
            var userToDelete = await _context.Users.FindAsync(SelectedUser.Id);
            if (userToDelete != null)
            {
                userToDelete.IsActive = false;
                await _context.SaveChangesAsync();
                Users.Remove(SelectedUser);
            }
        }
    }

    [RelayCommand]
    private async Task ResetUserPasswordAsync()
    {
        if (SelectedUser == null) return;

        var result = MessageBox.Show(
            $"هل تريد إعادة تعيين كلمة مرور المستخدم '{SelectedUser.Username}'?\nستصبح كلمة المرور الجديدة: 123456",
            "تأكيد",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

        if (result == MessageBoxResult.Yes)
        {
            var userToUpdate = await _context.Users.FindAsync(SelectedUser.Id);
            if (userToUpdate != null)
            {
                userToUpdate.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
                await _context.SaveChangesAsync();

                MessageBox.Show(
                    "تم إعادة تعيين كلمة المرور بنجاح\nكلمة المرور الجديدة: 123456",
                    "تم",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    MessageBoxResult.OK,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            }
        }
    }
}


// ========== Users View Model ==========
public partial class UsersViewModel : ObservableObject
{
    public UsersViewModel()
    {
    }
}

// ========== Invoice Settings View Model ==========
public partial class InvoiceSettingsViewModel : ObservableObject
{
    public InvoiceSettingsViewModel()
    {
    }
}
