using System.IO;
using System.Text.Json;

namespace TailoringShop.WPF.Services;

/// <summary>
/// إعدادات المحل التي تظهر في النظام
/// </summary>
public class ShopSettings
{
    public string ShopName { get; set; } = "متجر الخياطة";
    public string? LogoPath { get; set; }
}

/// <summary>
/// خدمة إدارة إعدادات المحل
/// </summary>
public static class ShopSettingsService
{
    private static readonly string SettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TailoringShop"
    );
    
    private static readonly string SettingsFile = Path.Combine(SettingsFolder, "shop_settings.json");
    private static readonly string LogoFolder = Path.Combine(SettingsFolder, "Logo");
    
    private static ShopSettings? _cachedSettings;
    
    /// <summary>
    /// حدث يُطلق عند تغيير الإعدادات
    /// </summary>
    public static event Action? SettingsChanged;
    
    /// <summary>
    /// تحميل إعدادات المحل
    /// </summary>
    public static ShopSettings LoadSettings()
    {
        if (_cachedSettings != null)
            return _cachedSettings;
            
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                _cachedSettings = JsonSerializer.Deserialize<ShopSettings>(json) ?? new ShopSettings();
            }
            else
            {
                _cachedSettings = new ShopSettings();
            }
        }
        catch
        {
            _cachedSettings = new ShopSettings();
        }
        
        return _cachedSettings;
    }
    
    /// <summary>
    /// حفظ إعدادات المحل
    /// </summary>
    public static void SaveSettings(ShopSettings settings)
    {
        try
        {
            // إنشاء المجلد إذا لم يكن موجوداً
            if (!Directory.Exists(SettingsFolder))
                Directory.CreateDirectory(SettingsFolder);
            
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            File.WriteAllText(SettingsFile, json);
            
            _cachedSettings = settings;
            
            // إطلاق حدث تغيير الإعدادات
            SettingsChanged?.Invoke();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving shop settings: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// حفظ صورة اللوغو ونسخها للمجلد المخصص
    /// </summary>
    public static string? SaveLogo(string sourceImagePath)
    {
        try
        {
            if (!File.Exists(sourceImagePath))
                return null;
            
            // إنشاء مجلد اللوغو
            if (!Directory.Exists(LogoFolder))
                Directory.CreateDirectory(LogoFolder);
            
            // الحصول على امتداد الملف
            var extension = Path.GetExtension(sourceImagePath);
            var destFileName = $"shop_logo{extension}";
            var destPath = Path.Combine(LogoFolder, destFileName);
            
            // حذف اللوغو القديم إن وجد
            DeleteExistingLogos();
            
            // نسخ الصورة الجديدة
            File.Copy(sourceImagePath, destPath, true);
            
            return destPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving logo: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// حذف جميع ملفات اللوغو القديمة
    /// </summary>
    private static void DeleteExistingLogos()
    {
        try
        {
            if (Directory.Exists(LogoFolder))
            {
                var files = Directory.GetFiles(LogoFolder, "shop_logo.*");
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
        }
        catch { }
    }
    
    /// <summary>
    /// حذف اللوغو
    /// </summary>
    public static void DeleteLogo()
    {
        try
        {
            DeleteExistingLogos();
            
            var settings = LoadSettings();
            settings.LogoPath = null;
            SaveSettings(settings);
        }
        catch { }
    }
    
    /// <summary>
    /// التحقق من وجود اللوغو
    /// </summary>
    public static bool HasLogo()
    {
        var settings = LoadSettings();
        return !string.IsNullOrEmpty(settings.LogoPath) && File.Exists(settings.LogoPath);
    }
    
    /// <summary>
    /// الحصول على مسار اللوغو
    /// </summary>
    public static string? GetLogoPath()
    {
        var settings = LoadSettings();
        if (!string.IsNullOrEmpty(settings.LogoPath) && File.Exists(settings.LogoPath))
            return settings.LogoPath;
        return null;
    }
    
    /// <summary>
    /// الحصول على اسم المحل
    /// </summary>
    public static string GetShopName()
    {
        return LoadSettings().ShopName;
    }
    
    /// <summary>
    /// تعيين اسم المحل
    /// </summary>
    public static void SetShopName(string name)
    {
        var settings = LoadSettings();
        settings.ShopName = name;
        SaveSettings(settings);
    }
}
