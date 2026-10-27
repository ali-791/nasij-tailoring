using System.IO;
using System.Windows.Threading;
using TailoringShop.Infrastructure.Data;

namespace TailoringShop.WPF.Services;

/// <summary>
/// خدمة النسخ الاحتياطي التلقائي اليومي
/// </summary>
public class AutoBackupService : IDisposable
{
    private static AutoBackupService? _instance;
    public static AutoBackupService Instance => _instance ??= new AutoBackupService();

    private readonly DispatcherTimer _timer;
    private readonly Func<AppDbContext> _contextFactory;
    private BackupService? _backupService;
    
    private bool _isEnabled;
    private DateTime _lastBackupTime;
    private DateTime _nextBackupTime;
    private string _backupFolder = string.Empty;
    private bool _isRunning;

    /// <summary>
    /// هل النسخ الاحتياطي التلقائي مفعّل؟
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            if (_isEnabled)
                StartTimer();
            else
                StopTimer();
            StatusChanged?.Invoke();
        }
    }

    /// <summary>
    /// وقت آخر نسخة احتياطية
    /// </summary>
    public DateTime LastBackupTime => _lastBackupTime;

    /// <summary>
    /// وقت النسخة الاحتياطية القادمة
    /// </summary>
    public DateTime NextBackupTime => _nextBackupTime;

    /// <summary>
    /// هل يعمل مؤقت النسخ الاحتياطي؟
    /// </summary>
    public bool IsTimerRunning => _timer.IsEnabled;

    /// <summary>
    /// الوقت المتبقي للنسخة القادمة
    /// </summary>
    public TimeSpan TimeUntilNextBackup => _nextBackupTime > DateTime.Now 
        ? _nextBackupTime - DateTime.Now 
        : TimeSpan.Zero;

    /// <summary>
    /// حدث يُطلق عند تغيّر حالة النسخ الاحتياطي
    /// </summary>
    public event Action? StatusChanged;

    /// <summary>
    /// حدث يُطلق عند اكتمال نسخة احتياطية
    /// </summary>
    public event Action<bool, string>? BackupCompleted;

    private AutoBackupService()
    {
        _contextFactory = () => App.GetService<AppDbContext>();
        
        // إنشاء مؤقت يعمل كل دقيقة للتحقق من وقت النسخ الاحتياطي
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _timer.Tick += Timer_Tick;

        // تحميل آخر وقت نسخ احتياطي من الإعدادات
        LoadLastBackupTime();
        
        // حساب وقت النسخة القادمة
        CalculateNextBackupTime();
    }

    /// <summary>
    /// تهيئة الخدمة مع مجلد النسخ الاحتياطي
    /// </summary>
    public void Initialize(string backupFolder, bool enableAutoBackup = true)
    {
        _backupFolder = backupFolder;
        _backupService = new BackupService(_contextFactory);
        
        // إنشاء مجلد النسخ الاحتياطية إذا لم يكن موجوداً
        if (!Directory.Exists(_backupFolder))
        {
            Directory.CreateDirectory(_backupFolder);
        }

        if (enableAutoBackup)
        {
            IsEnabled = true;
            
            // التحقق فوراً إذا كانت هناك نسخة احتياطية مطلوبة لهذا اليوم
            _ = CheckAndPerformBackupAsync();
        }
    }

    private void StartTimer()
    {
        if (!_timer.IsEnabled)
        {
            _timer.Start();
        }
    }

    private void StopTimer()
    {
        if (_timer.IsEnabled)
        {
            _timer.Stop();
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _ = CheckAndPerformBackupAsync();
        StatusChanged?.Invoke();
    }

    /// <summary>
    /// التحقق من الحاجة للنسخ الاحتياطي وتنفيذه
    /// </summary>
    public async Task CheckAndPerformBackupAsync()
    {
        if (!_isEnabled || _isRunning || _backupService == null)
            return;

        // التحقق مما إذا كان قد تم إنشاء نسخة احتياطية لهذا اليوم
        var todayBackupFile = GetTodayBackupFilePath();
        if (File.Exists(todayBackupFile))
        {
            // نسخة اليوم موجودة، تحديث وقت النسخة القادمة
            _lastBackupTime = File.GetCreationTime(todayBackupFile);
            CalculateNextBackupTime();
            return;
        }

        // تنفيذ النسخ الاحتياطي
        await PerformBackupAsync();
    }

    /// <summary>
    /// تنفيذ النسخ الاحتياطي التلقائي
    /// </summary>
    public async Task<(bool success, string message)> PerformBackupAsync()
    {
        if (_isRunning || _backupService == null)
            return (false, "النسخ الاحتياطي قيد التنفيذ بالفعل");

        _isRunning = true;
        StatusChanged?.Invoke();

        try
        {
            var backupFilePath = GetTodayBackupFilePath();
            
            // إذا كان الملف موجوداً، لا نحتاج لإنشاء نسخة جديدة
            if (File.Exists(backupFilePath))
            {
                _isRunning = false;
                return (true, "توجد نسخة احتياطية لهذا اليوم بالفعل");
            }

            // تنفيذ النسخ الاحتياطي
            await _backupService.CreateBackupAsync(backupFilePath);

            _lastBackupTime = DateTime.Now;
            SaveLastBackupTime();
            CalculateNextBackupTime();

            var message = $"تم إنشاء نسخة احتياطية تلقائية: {Path.GetFileName(backupFilePath)}";
            BackupCompleted?.Invoke(true, message);
            
            return (true, message);
        }
        catch (Exception ex)
        {
            var message = $"فشل النسخ الاحتياطي التلقائي: {ex.Message}";
            BackupCompleted?.Invoke(false, message);
            return (false, message);
        }
        finally
        {
            _isRunning = false;
            StatusChanged?.Invoke();
        }
    }

    /// <summary>
    /// الحصول على مسار ملف نسخة اليوم
    /// </summary>
    private string GetTodayBackupFilePath()
    {
        var fileName = $"Backup_{DateTime.Today:yyyy-MM-dd}.bak";
        return Path.Combine(_backupFolder, fileName);
    }

    /// <summary>
    /// حساب وقت النسخة الاحتياطية القادمة
    /// </summary>
    private void CalculateNextBackupTime()
    {
        // النسخة القادمة ستكون في منتصف الليل غداً
        _nextBackupTime = DateTime.Today.AddDays(1);
    }

    /// <summary>
    /// تحميل وقت آخر نسخة احتياطية
    /// </summary>
    private void LoadLastBackupTime()
    {
        try
        {
            var settingsFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TailoringShop",
                "autobackup_settings.txt"
            );

            if (File.Exists(settingsFile))
            {
                var content = File.ReadAllText(settingsFile);
                if (DateTime.TryParse(content, out var lastTime))
                {
                    _lastBackupTime = lastTime;
                }
            }
        }
        catch
        {
            // تجاهل الأخطاء
        }
    }

    /// <summary>
    /// حفظ وقت آخر نسخة احتياطية
    /// </summary>
    private void SaveLastBackupTime()
    {
        try
        {
            var settingsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TailoringShop"
            );

            if (!Directory.Exists(settingsDir))
            {
                Directory.CreateDirectory(settingsDir);
            }

            var settingsFile = Path.Combine(settingsDir, "autobackup_settings.txt");
            File.WriteAllText(settingsFile, _lastBackupTime.ToString("o"));
        }
        catch
        {
            // تجاهل الأخطاء
        }
    }

    /// <summary>
    /// الحصول على قائمة النسخ الاحتياطية الموجودة
    /// </summary>
    public List<BackupFileInfo> GetBackupFiles()
    {
        var files = new List<BackupFileInfo>();

        if (string.IsNullOrEmpty(_backupFolder) || !Directory.Exists(_backupFolder))
            return files;

        foreach (var file in Directory.GetFiles(_backupFolder, "Backup_*.bak"))
        {
            var fileInfo = new FileInfo(file);
            files.Add(new BackupFileInfo
            {
                FileName = fileInfo.Name,
                FilePath = fileInfo.FullName,
                CreatedAt = fileInfo.CreationTime,
                Size = fileInfo.Length
            });
        }

        return files.OrderByDescending(f => f.CreatedAt).ToList();
    }

    /// <summary>
    /// حذف النسخ الاحتياطية القديمة (الإبقاء على آخر N نسخة)
    /// </summary>
    public int CleanupOldBackups(int keepCount = 30)
    {
        var files = GetBackupFiles();
        var deletedCount = 0;

        if (files.Count <= keepCount)
            return 0;

        var filesToDelete = files.Skip(keepCount).ToList();

        foreach (var file in filesToDelete)
        {
            try
            {
                File.Delete(file.FilePath);
                deletedCount++;
            }
            catch
            {
                // تجاهل الأخطاء
            }
        }

        return deletedCount;
    }

    public void Dispose()
    {
        _timer.Stop();
    }
}

/// <summary>
/// معلومات ملف النسخة الاحتياطية
/// </summary>
public class BackupFileInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long Size { get; set; }

    public string SizeDisplay
    {
        get
        {
            if (Size < 1024)
                return $"{Size} B";
            if (Size < 1024 * 1024)
                return $"{Size / 1024.0:N1} KB";
            return $"{Size / (1024.0 * 1024.0):N2} MB";
        }
    }
}
