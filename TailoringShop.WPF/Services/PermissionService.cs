using Microsoft.EntityFrameworkCore;
using TailoringShop.Infrastructure.Data;

namespace TailoringShop.WPF.Services;

/// <summary>
/// خدمة إدارة الصلاحيات - Singleton
/// تحمّل صلاحيات المستخدم عند تسجيل الدخول وتتحقق منها عند كل عملية
/// </summary>
public class PermissionService
{
    private static PermissionService? _instance;
    private static readonly object _lock = new();
    
    private HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);
    private long _userId = 0;
    private bool _isAdmin = false;

    private PermissionService() { }

    public static PermissionService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new PermissionService();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// تحميل صلاحيات المستخدم من قاعدة البيانات
    /// يجب استدعاء هذه الدالة بعد تسجيل الدخول مباشرة
    /// </summary>
    public async Task LoadPermissionsAsync(AppDbContext context, long userId)
    {
        _userId = userId;
        _permissions.Clear();
        _isAdmin = false;

        try
        {
            // 1. جلب أدوار المستخدم
            var userRoles = await context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .ToListAsync();

            if (!userRoles.Any())
            {
                System.Diagnostics.Debug.WriteLine($"[PermissionService] No roles found for user {userId}");
                return;
            }

            // 2. التحقق من كون المستخدم Admin
            _isAdmin = userRoles.Any(ur => ur.Role.Name.Equals("admin", StringComparison.OrdinalIgnoreCase));

            if (_isAdmin)
            {
                _permissions.Add("*.*");
                System.Diagnostics.Debug.WriteLine($"[PermissionService] User {userId} is ADMIN - full access granted");
                return;
            }

            // 3. جلب IDs الأدوار
            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

            // 4. جلب الصلاحيات لهذه الأدوار - Query منفصل وواضح
            var permissions = await context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Include(rp => rp.Permission)
                .Select(rp => new { rp.Permission.Module, rp.Permission.Name })
                .Distinct()
                .ToListAsync();

            // 5. إضافة الصلاحيات للـ HashSet
            foreach (var perm in permissions)
            {
                var permKey = $"{perm.Module}.{perm.Name}";
                _permissions.Add(permKey);
                System.Diagnostics.Debug.WriteLine($"[PermissionService] Added permission: {permKey}");
            }

            System.Diagnostics.Debug.WriteLine($"[PermissionService] Loaded {_permissions.Count} permissions for user {userId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PermissionService] Error loading permissions: {ex.Message}");
        }
    }

    /// <summary>
    /// التحقق من صلاحية معينة
    /// </summary>
    public bool HasPermission(string module, string action)
    {
        // Admin لديه كل الصلاحيات
        if (_isAdmin || _permissions.Contains("*.*"))
            return true;

        var permKey = $"{module}.{action}";
        var hasIt = _permissions.Contains(permKey);
        
        System.Diagnostics.Debug.WriteLine($"[PermissionService] Check '{permKey}' = {hasIt}");
        
        return hasIt;
    }

    /// <summary>
    /// صلاحية العرض
    /// </summary>
    public bool CanView(string module) => HasPermission(module, "view");

    /// <summary>
    /// صلاحية الإضافة
    /// </summary>
    public bool CanCreate(string module) => HasPermission(module, "create");

    /// <summary>
    /// صلاحية التعديل
    /// </summary>
    public bool CanUpdate(string module) => HasPermission(module, "update");

    /// <summary>
    /// صلاحية الحذف
    /// </summary>
    public bool CanDelete(string module) => HasPermission(module, "delete");

    /// <summary>
    /// هل المستخدم مدير؟
    /// </summary>
    public bool IsAdmin => _isAdmin;

    /// <summary>
    /// عدد الصلاحيات
    /// </summary>
    public int PermissionsCount => _isAdmin ? 999 : _permissions.Count;

    /// <summary>
    /// مسح الصلاحيات عند تسجيل الخروج
    /// </summary>
    public void Clear()
    {
        _permissions.Clear();
        _userId = 0;
        _isAdmin = false;
        System.Diagnostics.Debug.WriteLine("[PermissionService] Cleared all permissions");
    }

    /// <summary>
    /// جلب جميع الصلاحيات للعرض
    /// </summary>
    public IReadOnlyCollection<string> GetAllPermissions() => _permissions;

    /// <summary>
    /// طباعة جميع الصلاحيات للـ Debug
    /// </summary>
    public void PrintAllPermissions()
    {
        System.Diagnostics.Debug.WriteLine($"[PermissionService] === Current Permissions for User {_userId} ===");
        System.Diagnostics.Debug.WriteLine($"[PermissionService] IsAdmin: {_isAdmin}");
        foreach (var perm in _permissions)
        {
            System.Diagnostics.Debug.WriteLine($"[PermissionService]   - {perm}");
        }
        System.Diagnostics.Debug.WriteLine($"[PermissionService] === Total: {_permissions.Count} ===");
    }
}
