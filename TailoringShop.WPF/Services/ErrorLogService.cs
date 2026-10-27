using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace TailoringShop.WPF.Services;

/// <summary>
/// خدمة تسجيل الأخطاء - تحفظ جميع الأخطاء في ملف log
/// </summary>
public class ErrorLogService
{
    private static readonly Lazy<ErrorLogService> _instance = new(() => new ErrorLogService());
    public static ErrorLogService Instance => _instance.Value;

    private readonly string _logFolder;
    private readonly object _lockObject = new();

    private ErrorLogService()
    {
        _logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        
        // إنشاء مجلد السجلات إذا لم يكن موجوداً
        if (!Directory.Exists(_logFolder))
        {
            Directory.CreateDirectory(_logFolder);
        }
    }

    /// <summary>
    /// تسجيل خطأ
    /// </summary>
    public void LogError(
        Exception ex, 
        string? additionalInfo = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            var logEntry = BuildLogEntry("ERROR", ex.Message, ex.StackTrace, additionalInfo, memberName, filePath, lineNumber);
            WriteLog(logEntry);
            
            // أيضاً اكتب في Debug output
            Debug.WriteLine(logEntry);
        }
        catch
        {
            // لا نريد أن يسبب التسجيل نفسه خطأ
        }
    }

    /// <summary>
    /// تسجيل تحذير
    /// </summary>
    public void LogWarning(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            var logEntry = BuildLogEntry("WARNING", message, null, null, memberName, filePath, lineNumber);
            WriteLog(logEntry);
            Debug.WriteLine(logEntry);
        }
        catch { }
    }

    /// <summary>
    /// تسجيل معلومات
    /// </summary>
    public void LogInfo(string message)
    {
        try
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [INFO] {message}";
            WriteLog(logEntry);
            Debug.WriteLine(logEntry);
        }
        catch { }
    }

    /// <summary>
    /// بناء سجل الخطأ
    /// </summary>
    private string BuildLogEntry(
        string level,
        string message,
        string? stackTrace,
        string? additionalInfo,
        string memberName,
        string filePath,
        int lineNumber)
    {
        var fileName = Path.GetFileName(filePath);
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}]");
        sb.AppendLine($"Location: {fileName} → {memberName}() (Line {lineNumber})");
        sb.AppendLine($"Message: {message}");
        
        if (!string.IsNullOrEmpty(additionalInfo))
        {
            sb.AppendLine($"Additional Info: {additionalInfo}");
        }
        
        if (!string.IsNullOrEmpty(stackTrace))
        {
            sb.AppendLine("Stack Trace:");
            sb.AppendLine(stackTrace);
        }
        
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        
        return sb.ToString();
    }

    /// <summary>
    /// كتابة السجل في الملف
    /// </summary>
    private void WriteLog(string logEntry)
    {
        lock (_lockObject)
        {
            var logFile = Path.Combine(_logFolder, $"error_{DateTime.Now:yyyy-MM-dd}.log");
            File.AppendAllText(logFile, logEntry + Environment.NewLine);
        }
    }

    /// <summary>
    /// الحصول على مسار مجلد السجلات
    /// </summary>
    public string LogFolder => _logFolder;

    /// <summary>
    /// الحصول على ملف سجل اليوم
    /// </summary>
    public string TodayLogFile => Path.Combine(_logFolder, $"error_{DateTime.Now:yyyy-MM-dd}.log");

    /// <summary>
    /// قراءة سجل اليوم
    /// </summary>
    public string? GetTodayLog()
    {
        var logFile = TodayLogFile;
        if (File.Exists(logFile))
        {
            return File.ReadAllText(logFile);
        }
        return null;
    }

    /// <summary>
    /// حذف السجلات القديمة (أقدم من عدد أيام معين)
    /// </summary>
    public void CleanOldLogs(int daysToKeep = 30)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            var logFiles = Directory.GetFiles(_logFolder, "error_*.log");

            foreach (var file in logFiles)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    File.Delete(file);
                }
            }
        }
        catch { }
    }
}

/// <summary>
/// Extension methods لتسهيل التسجيل
/// </summary>
public static class ErrorLogExtensions
{
    public static void LogError(this Exception ex, string? additionalInfo = null)
    {
        ErrorLogService.Instance.LogError(ex, additionalInfo);
    }
}
