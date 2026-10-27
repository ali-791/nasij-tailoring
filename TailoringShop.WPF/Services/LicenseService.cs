using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TailoringShop.WPF.Services;

/// <summary>
/// أنواع الترخيص
/// </summary>
public enum LicenseType
{
    None,       // لا يوجد ترخيص
    Trial,      // تجريبي (أسبوع)
    Permanent   // دائمي
}

/// <summary>
/// بيانات الترخيص
/// </summary>
public class LicenseInfo
{
    public LicenseType Type { get; set; } = LicenseType.None;
    public string? LicenseKey { get; set; }
    public DateTime? ActivationDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? MachineId { get; set; }
}

/// <summary>
/// نتيجة التحقق من الترخيص
/// </summary>
public class LicenseValidationResult
{
    public bool IsValid { get; set; }
    public LicenseType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
}

/// <summary>
/// خدمة إدارة التراخيص - مفاتيح ثابتة
/// </summary>
public static class LicenseService
{
    private static readonly string LicenseFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TailoringShop"
    );
    
    private static readonly string LicenseFile = Path.Combine(LicenseFolder, "license.dat");
    
    // المفتاح السري للتشفير
    private static readonly string SecretKey = "TailoringShop2024SecretKey!@#$";
    
    // ═══════════════════════════════════════════════════════════════════════════
    // المفاتيح الثابتة - فقط المطور يعرفها!
    // ═══════════════════════════════════════════════════════════════════════════
    
    // مفاتيح التجريب (أسبوع واحد)
    private const string TRIAL_KEY_1 = "TAILOR-TRIAL-7DAY-2024-KEY1";
    private const string TRIAL_KEY_2 = "TAILOR-TRIAL-7DAY-2024-KEY2";
    
    // مفتاح التفعيل الدائمي
    private const string PERMANENT_KEY = "TAILOR-PERM-FULL-2024-MASTER";
    
    // ═══════════════════════════════════════════════════════════════════════════
    
    private static LicenseInfo? _cachedLicense;
    
    /// <summary>
    /// التحقق من صحة المفتاح
    /// </summary>
    public static (bool isValid, LicenseType type) ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return (false, LicenseType.None);
        
        key = key.Trim().ToUpper();
        
        // التحقق من مفاتيح التجريب
        if (key == TRIAL_KEY_1.ToUpper() || key == TRIAL_KEY_2.ToUpper())
        {
            return (true, LicenseType.Trial);
        }
        
        // التحقق من المفتاح الدائمي
        if (key == PERMANENT_KEY.ToUpper())
        {
            return (true, LicenseType.Permanent);
        }
        
        return (false, LicenseType.None);
    }
    
    /// <summary>
    /// تفعيل الترخيص
    /// </summary>
    public static LicenseValidationResult ActivateLicense(string key)
    {
        var (isValid, type) = ValidateKey(key);
        
        if (!isValid)
        {
            return new LicenseValidationResult
            {
                IsValid = false,
                Message = "مفتاح الترخيص غير صالح"
            };
        }
        
        // التحقق إذا كان هناك ترخيص سابق
        var currentLicense = LoadLicense();
        
        // إذا كان المفتاح التجريبي مستخدم سابقاً
        if (type == LicenseType.Trial && currentLicense.Type == LicenseType.Trial)
        {
            // التحقق إذا كان نفس المفتاح
            if (currentLicense.LicenseKey?.ToUpper() == key.ToUpper())
            {
                // إذا لم تنتهِ صلاحيته بعد، اسمح بالمتابعة
                if (currentLicense.ExpirationDate.HasValue && currentLicense.ExpirationDate.Value > DateTime.Now)
                {
                    var days = (currentLicense.ExpirationDate.Value - DateTime.Now).Days + 1;
                    return new LicenseValidationResult
                    {
                        IsValid = true,
                        Type = LicenseType.Trial,
                        Message = $"الترخيص التجريبي مفعّل بالفعل\nمتبقي {days} يوم",
                        DaysRemaining = days
                    };
                }
            }
        }
        
        // إنشاء الترخيص الجديد
        var newLicense = new LicenseInfo
        {
            Type = type,
            LicenseKey = key.ToUpper(),
            ActivationDate = DateTime.Now,
            MachineId = Environment.MachineName
        };
        
        // تعيين تاريخ الانتهاء للتجريبي
        if (type == LicenseType.Trial)
        {
            newLicense.ExpirationDate = DateTime.Now.AddDays(7);
        }
        
        // حفظ الترخيص
        SaveLicense(newLicense);
        
        var message = type == LicenseType.Trial
            ? $"تم تفعيل الفترة التجريبية بنجاح!\nصالح حتى: {newLicense.ExpirationDate:yyyy/MM/dd}"
            : "تم تفعيل الترخيص الدائمي بنجاح!";
        
        return new LicenseValidationResult
        {
            IsValid = true,
            Type = type,
            Message = message,
            DaysRemaining = type == LicenseType.Trial ? 7 : -1
        };
    }
    
    /// <summary>
    /// التحقق من حالة الترخيص الحالي
    /// </summary>
    public static LicenseValidationResult CheckLicense()
    {
        var license = LoadLicense();
        
        // لا يوجد ترخيص
        if (license.Type == LicenseType.None || string.IsNullOrEmpty(license.LicenseKey))
        {
            return new LicenseValidationResult
            {
                IsValid = false,
                Type = LicenseType.None,
                Message = "لم يتم تفعيل البرنامج بعد"
            };
        }
        
        // ترخيص دائمي
        if (license.Type == LicenseType.Permanent)
        {
            return new LicenseValidationResult
            {
                IsValid = true,
                Type = LicenseType.Permanent,
                Message = "ترخيص دائمي - مفعّل",
                DaysRemaining = -1
            };
        }
        
        // ترخيص تجريبي - التحقق من الصلاحية
        if (license.Type == LicenseType.Trial)
        {
            if (license.ExpirationDate.HasValue)
            {
                var daysRemaining = (license.ExpirationDate.Value - DateTime.Now).Days;
                
                if (daysRemaining < 0)
                {
                    return new LicenseValidationResult
                    {
                        IsValid = false,
                        Type = LicenseType.Trial,
                        Message = "انتهت الفترة التجريبية",
                        DaysRemaining = 0
                    };
                }
                
                return new LicenseValidationResult
                {
                    IsValid = true,
                    Type = LicenseType.Trial,
                    Message = $"فترة تجريبية - متبقي {daysRemaining + 1} يوم",
                    DaysRemaining = daysRemaining + 1
                };
            }
        }
        
        return new LicenseValidationResult
        {
            IsValid = false,
            Message = "خطأ في بيانات الترخيص"
        };
    }
    
    /// <summary>
    /// الحصول على معلومات الترخيص الحالي
    /// </summary>
    public static LicenseInfo GetCurrentLicense()
    {
        return LoadLicense();
    }
    
    /// <summary>
    /// تحميل بيانات الترخيص
    /// </summary>
    private static LicenseInfo LoadLicense()
    {
        if (_cachedLicense != null)
            return _cachedLicense;
        
        try
        {
            if (File.Exists(LicenseFile))
            {
                var encryptedData = File.ReadAllText(LicenseFile);
                var json = Decrypt(encryptedData);
                _cachedLicense = JsonSerializer.Deserialize<LicenseInfo>(json) ?? new LicenseInfo();
            }
            else
            {
                _cachedLicense = new LicenseInfo();
            }
        }
        catch
        {
            _cachedLicense = new LicenseInfo();
        }
        
        return _cachedLicense;
    }
    
    /// <summary>
    /// حفظ بيانات الترخيص
    /// </summary>
    private static void SaveLicense(LicenseInfo license)
    {
        try
        {
            if (!Directory.Exists(LicenseFolder))
                Directory.CreateDirectory(LicenseFolder);
            
            var json = JsonSerializer.Serialize(license);
            var encryptedData = Encrypt(json);
            File.WriteAllText(LicenseFile, encryptedData);
            
            _cachedLicense = license;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving license: {ex.Message}");
        }
    }
    
    /// <summary>
    /// إعادة تعيين الـ cache
    /// </summary>
    public static void ResetCache()
    {
        _cachedLicense = null;
    }
    
    /// <summary>
    /// تشفير النص
    /// </summary>
    private static string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        var key = Encoding.UTF8.GetBytes(SecretKey.PadRight(32).Substring(0, 32));
        var iv = Encoding.UTF8.GetBytes(SecretKey.PadRight(16).Substring(0, 16));
        
        aes.Key = key;
        aes.IV = iv;
        
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        
        return Convert.ToBase64String(encryptedBytes);
    }
    
    /// <summary>
    /// فك تشفير النص
    /// </summary>
    private static string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        var key = Encoding.UTF8.GetBytes(SecretKey.PadRight(32).Substring(0, 32));
        var iv = Encoding.UTF8.GetBytes(SecretKey.PadRight(16).Substring(0, 16));
        
        aes.Key = key;
        aes.IV = iv;
        
        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        
        return Encoding.UTF8.GetString(plainBytes);
    }
}
