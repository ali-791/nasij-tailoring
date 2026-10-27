using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using TailoringShop.Core.Entities;
using TailoringShop.Infrastructure.Data;

namespace TailoringShop.WPF.Views.Dialogs;

public class PermissionDisplay
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
}

public class ModulePermissions
{
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleDisplayName { get; set; } = string.Empty;
    public ObservableCollection<PermissionDisplay> Permissions { get; set; } = new();
}

public partial class RolePermissionsDialog : Window
{
    private readonly AppDbContext _context;
    private Role? _selectedRole;
    private ObservableCollection<ModulePermissions> _modulePermissions = new();
    private bool _hasChanges = false;

    private static readonly Dictionary<string, string> ModuleNames = new()
    {
        { "dashboard", "لوحة التحكم" },
        { "customers", "العملاء" },
        { "products", "المنتجات" },
        { "invoices", "الفواتير" },
        { "tailoring", "طلبات الفصال" },
        { "expenses", "المصروفات" },
        { "cashboxes", "القاصات" },
        { "reports", "التقارير" },
        { "users", "المستخدمين" },
        { "settings", "الإعدادات" }
    };

    private static readonly Dictionary<string, string> PermissionNames = new()
    {
        { "view", "عرض" },
        { "create", "إضافة" },
        { "update", "تعديل" },
        { "delete", "حذف" },
        { "manage", "إدارة" },
        { "export", "تصدير" }
    };

    public RolePermissionsDialog(AppDbContext context)
    {
        InitializeComponent();
        _context = context;
        Loaded += RolePermissionsDialog_Loaded;
    }

    private async void RolePermissionsDialog_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadRolesAsync();
    }

    private async Task LoadRolesAsync()
    {
        var roles = await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
        RolesListBox.ItemsSource = roles;
    }

    private async void RolesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RolesListBox.SelectedItem is Role role)
        {
            _selectedRole = role;
            SelectedRoleTitle.Text = "صلاحيات: " + role.DisplayName;
            SelectedRoleDesc.Text = role.IsSystem ? "دور نظام" : "دور مخصص";
            SaveButton.IsEnabled = true;
            await LoadPermissionsAsync(role.Id);
        }
    }

    private async Task LoadPermissionsAsync(long roleId)
    {
        var allPermissions = await _context.Permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .ToListAsync();

        var grantedPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        _modulePermissions.Clear();

        var grouped = allPermissions.GroupBy(p => p.Module);
        foreach (var group in grouped)
        {
            var modulePerms = new ModulePermissions
            {
                ModuleName = group.Key,
                ModuleDisplayName = ModuleNames.GetValueOrDefault(group.Key, group.Key)
            };

            foreach (var perm in group)
            {
                modulePerms.Permissions.Add(new PermissionDisplay
                {
                    Id = perm.Id,
                    Name = perm.Name,
                    DisplayName = PermissionNames.GetValueOrDefault(perm.Name, perm.Name),
                    IsGranted = grantedPermissions.Contains(perm.Id)
                });
            }

            _modulePermissions.Add(modulePerms);
        }

        ModulesItemsControl.ItemsSource = _modulePermissions;
        _hasChanges = false;
    }

    private void Permission_Changed(object sender, RoutedEventArgs e)
    {
        _hasChanges = true;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRole == null) return;

        try
        {
            var existingPerms = await _context.RolePermissions
                .Where(rp => rp.RoleId == _selectedRole.Id)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(existingPerms);

            foreach (var module in _modulePermissions)
            {
                foreach (var perm in module.Permissions.Where(p => p.IsGranted))
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = _selectedRole.Id,
                        PermissionId = perm.Id
                    });
                }
            }

            await _context.SaveChangesAsync();

            MessageBox.Show("تم حفظ الصلاحيات بنجاح", "تم",
                MessageBoxButton.OK, MessageBoxImage.Information,
                MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            _hasChanges = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show("حدث خطأ: " + ex.Message, "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Error,
                MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        if (_hasChanges)
        {
            var result = MessageBox.Show("يوجد تغييرات غير محفوظة. هل تريد الإغلاق؟", "تأكيد",
                MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.No, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            if (result != MessageBoxResult.Yes) return;
        }
        Close();
    }
}
