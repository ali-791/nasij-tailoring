using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using TailoringShop.Infrastructure.Data;
using TailoringShop.WPF.Services;
using TailoringShop.WPF.Views.Dialogs;

namespace TailoringShop.WPF.Views.Pages;

public class UserDisplayModel
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string RoleDisplayName { get; set; } = "بدون دور";
    public long? RoleId { get; set; }
    public int PermissionsCount { get; set; }
    public string LastLoginDisplay => LastLoginAt?.ToString("yyyy/MM/dd HH:mm") ?? "لم يسجل دخول";
}

public partial class UsersPage : UserControl
{
    private readonly AppDbContext _context;
    private ObservableCollection<UserDisplayModel> _allUsers = new();
    private ObservableCollection<UserDisplayModel> _filteredUsers = new();

    public UsersPage()
    {
        InitializeComponent();
        _context = App.GetService<AppDbContext>();
        Loaded += UsersPage_Loaded;
    }

    private async void UsersPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
        ApplyPermissions();
    }

    private void ApplyPermissions()
    {
        var perms = PermissionService.Instance;
        
        // التحقق من صلاحية إضافة مستخدم
        AddUserButton.Visibility = perms.CanCreate("users") ? Visibility.Visible : Visibility.Collapsed;
        
        // التحقق من صلاحية إدارة الأدوار
        ManageRolesButton.Visibility = perms.HasPermission("users", "manage") ? Visibility.Visible : Visibility.Collapsed;
    }

    private async Task LoadDataAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;

        try
        {
            // Load users with their roles
            var users = await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Username)
                .ToListAsync();

            var userRoles = await _context.UserRoles
                .AsNoTracking()
                .Include(ur => ur.Role)
                .ToListAsync();

            // Get permissions count per role
            var rolePermCounts = await _context.RolePermissions
                .AsNoTracking()
                .GroupBy(rp => rp.RoleId)
                .Select(g => new { RoleId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RoleId, x => x.Count);

            var displayList = users.Select(u =>
            {
                var userRole = userRoles.FirstOrDefault(ur => ur.UserId == u.Id);
                var permCount = userRole != null && rolePermCounts.ContainsKey(userRole.RoleId)
                    ? rolePermCounts[userRole.RoleId]
                    : 0;

                return new UserDisplayModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    RoleDisplayName = userRole?.Role?.DisplayName ?? "بدون دور",
                    RoleId = userRole?.RoleId,
                    PermissionsCount = permCount
                };
            }).ToList();

            _allUsers = new ObservableCollection<UserDisplayModel>(displayList);
            ApplyFilter();

            // Update stats
            TotalUsersText.Text = users.Count.ToString();
            ActiveUsersText.Text = users.Count(u => u.IsActive).ToString();
            
            var adminRoleId = await _context.Roles
                .Where(r => r.Name.ToLower() == "admin")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();
            AdminsCountText.Text = userRoles.Count(ur => ur.RoleId == adminRoleId).ToString();
            
            RolesCountText.Text = (await _context.Roles.CountAsync(r => r.IsActive)).ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error,
                MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private void ApplyFilter()
    {
        var search = SearchTextBox.Text?.ToLower() ?? "";
        
        var filtered = string.IsNullOrWhiteSpace(search)
            ? _allUsers
            : new ObservableCollection<UserDisplayModel>(
                _allUsers.Where(u => 
                    u.Username.ToLower().Contains(search) || 
                    u.FullName.ToLower().Contains(search) ||
                    u.RoleDisplayName.ToLower().Contains(search)));

        _filteredUsers = filtered;
        UsersDataGrid.ItemsSource = _filteredUsers;
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void AddUser_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.CanCreate("users"))
        {
            ShowNoPermissionMessage();
            return;
        }

        var dialog = new UserDialog(_context);
        dialog.Owner = Window.GetWindow(this);
        if (dialog.ShowDialog() == true)
        {
            _ = LoadDataAsync();
        }
    }

    private void EditUser_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.CanUpdate("users"))
        {
            ShowNoPermissionMessage();
            return;
        }

        if (sender is Button btn && btn.DataContext is UserDisplayModel user)
        {
            var dialog = new UserDialog(_context, user.Id);
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                _ = LoadDataAsync();
            }
        }
    }

    private async void ResetPassword_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.CanUpdate("users"))
        {
            ShowNoPermissionMessage();
            return;
        }

        if (sender is Button btn && btn.DataContext is UserDisplayModel user)
        {
            var result = MessageBox.Show(
                $"هل تريد إعادة تعيين كلمة مرور '{user.Username}'?\nكلمة المرور الجديدة: 123456",
                "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.No, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            if (result == MessageBoxResult.Yes)
            {
                var dbUser = await _context.Users.FindAsync(user.Id);
                if (dbUser != null)
                {
                    dbUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
                    await _context.SaveChangesAsync();
                    MessageBox.Show("تم إعادة تعيين كلمة المرور\nكلمة المرور الجديدة: 123456", "تم",
                        MessageBoxButton.OK, MessageBoxImage.Information,
                        MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                }
            }
        }
    }

    private async void ToggleStatus_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.CanUpdate("users"))
        {
            ShowNoPermissionMessage();
            return;
        }

        if (sender is Button btn && btn.DataContext is UserDisplayModel user)
        {
            if (user.Id == App.CurrentUser?.Id)
            {
                MessageBox.Show("لا يمكنك تعطيل حسابك الخاص", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning,
                    MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            var action = user.IsActive ? "تعطيل" : "تفعيل";
            var result = MessageBox.Show(
                $"هل تريد {action} المستخدم '{user.Username}'?",
                "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.No, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            if (result == MessageBoxResult.Yes)
            {
                var dbUser = await _context.Users.FindAsync(user.Id);
                if (dbUser != null)
                {
                    dbUser.IsActive = !dbUser.IsActive;
                    await _context.SaveChangesAsync();
                    await LoadDataAsync();
                }
            }
        }
    }

    private async void DeleteUser_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.CanDelete("users"))
        {
            ShowNoPermissionMessage();
            return;
        }

        if (sender is Button btn && btn.DataContext is UserDisplayModel user)
        {
            if (user.Id == App.CurrentUser?.Id)
            {
                MessageBox.Show("لا يمكنك حذف حسابك الخاص", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning,
                    MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            var result = MessageBox.Show(
                $"هل تريد حذف المستخدم '{user.Username}'?\nهذا الإجراء لا يمكن التراجع عنه!",
                "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                MessageBoxResult.No, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            if (result == MessageBoxResult.Yes)
            {
                var dbUser = await _context.Users.FindAsync(user.Id);
                if (dbUser != null)
                {
                    // Remove user roles first
                    var userRoles = await _context.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync();
                    _context.UserRoles.RemoveRange(userRoles);
                    
                    // Soft delete - just deactivate
                    dbUser.IsActive = false;
                    await _context.SaveChangesAsync();
                    await LoadDataAsync();
                }
            }
        }
    }

    private void ManageRoles_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.HasPermission("users", "manage"))
        {
            ShowNoPermissionMessage();
            return;
        }

        var dialog = new RolePermissionsDialog(_context);
        dialog.Owner = Window.GetWindow(this);
        dialog.ShowDialog();
        _ = LoadDataAsync();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private void ShowNoPermissionMessage()
    {
        MessageBox.Show("ليس لديك صلاحية لتنفيذ هذا الإجراء", "غير مصرح",
            MessageBoxButton.OK, MessageBoxImage.Warning,
            MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
    }
}
