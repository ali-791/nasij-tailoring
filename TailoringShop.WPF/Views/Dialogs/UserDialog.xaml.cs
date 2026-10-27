using System.Windows;
using Microsoft.EntityFrameworkCore;
using TailoringShop.Core.Entities;
using TailoringShop.Infrastructure.Data;

namespace TailoringShop.WPF.Views.Dialogs;

public partial class UserDialog : Window
{
    private readonly AppDbContext _context;
    private readonly long? _userId;
    private User? _existingUser;

    public UserDialog(AppDbContext context, long? userId = null)
    {
        InitializeComponent();
        _context = context;
        _userId = userId;
        Loaded += UserDialog_Loaded;
    }

    private async void UserDialog_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadRolesAsync();
        if (_userId.HasValue)
        {
            await LoadUserAsync();
            DialogTitle.Text = "تعديل المستخدم";
            PasswordPanel.Visibility = Visibility.Collapsed;
            ConfirmPasswordPanel.Visibility = Visibility.Collapsed;
        }
    }

    private async Task LoadRolesAsync()
    {
        var roles = await _context.Roles.Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync();
        RoleComboBox.ItemsSource = roles;
        var defaultRole = roles.FirstOrDefault(r => r.Name == "user");
        if (defaultRole != null) RoleComboBox.SelectedItem = defaultRole;
    }

    private async Task LoadUserAsync()
    {
        _existingUser = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == _userId);

        if (_existingUser != null)
        {
            UsernameTextBox.Text = _existingUser.Username;
            FullNameTextBox.Text = _existingUser.FullName;
            IsActiveCheckBox.IsChecked = _existingUser.IsActive;
            var userRole = _existingUser.UserRoles.FirstOrDefault();
            if (userRole != null)
            {
                foreach (Role role in RoleComboBox.Items)
                {
                    if (role.Id == userRole.RoleId) { RoleComboBox.SelectedItem = role; break; }
                }
            }
        }
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UsernameTextBox.Text)) { ShowError("يرجى إدخال اسم المستخدم"); return; }
        if (string.IsNullOrWhiteSpace(FullNameTextBox.Text)) { ShowError("يرجى إدخال الاسم الكامل"); return; }
        if (RoleComboBox.SelectedItem == null) { ShowError("يرجى اختيار الدور"); return; }

        if (!_userId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox.Password)) { ShowError("يرجى إدخال كلمة المرور"); return; }
            if (PasswordBox.Password.Length < 4) { ShowError("كلمة المرور يجب أن تكون 4 أحرف على الأقل"); return; }
            if (PasswordBox.Password != ConfirmPasswordBox.Password) { ShowError("كلمة المرور غير متطابقة"); return; }
        }

        var usernameExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == UsernameTextBox.Text.ToLower().Trim() && u.Id != (_userId ?? 0));
        if (usernameExists) { ShowError("اسم المستخدم موجود مسبقاً"); return; }

        try
        {
            var selectedRole = (Role)RoleComboBox.SelectedItem;

            if (_userId.HasValue && _existingUser != null)
            {
                _existingUser.Username = UsernameTextBox.Text.Trim();
                _existingUser.FullName = FullNameTextBox.Text.Trim();
                _existingUser.IsActive = IsActiveCheckBox.IsChecked ?? true;

                var existingUserRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == _existingUser.Id);
                if (existingUserRole != null && existingUserRole.RoleId != selectedRole.Id)
                {
                    _context.UserRoles.Remove(existingUserRole);
                    _context.UserRoles.Add(new UserRole { UserId = _existingUser.Id, RoleId = selectedRole.Id, AssignedBy = App.CurrentUser?.Id });
                }
                else if (existingUserRole == null)
                {
                    _context.UserRoles.Add(new UserRole { UserId = _existingUser.Id, RoleId = selectedRole.Id, AssignedBy = App.CurrentUser?.Id });
                }
            }
            else
            {
                var newUser = new User
                {
                    Username = UsernameTextBox.Text.Trim(),
                    FullName = FullNameTextBox.Text.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordBox.Password),
                    IsActive = IsActiveCheckBox.IsChecked ?? true
                };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                _context.UserRoles.Add(new UserRole { UserId = newUser.Id, RoleId = selectedRole.Id, AssignedBy = App.CurrentUser?.Id });
            }

            await _context.SaveChangesAsync();
            MessageBox.Show(_userId.HasValue ? "تم تعديل المستخدم بنجاح" : "تم إضافة المستخدم بنجاح", "تم", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            DialogResult = true;
            Close();
        }
        catch (Exception ex) { ShowError("حدث خطأ: " + ex.Message); }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }

    private void ShowError(string message)
    {
        MessageBox.Show(message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
    }
}
