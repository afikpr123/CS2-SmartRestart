using System;
using System.IO;
using System.Text;

namespace SmartRestart;

public class Logger
{
    private readonly string _logDirectory;
    private readonly string _logFilePath;
    private readonly object _lockObject = new();
    private bool _fileLoggingEnabled = true;

    public string LogDirectory => _logDirectory;
    public bool DebugEnabled { get; set; } = false;

    public Logger(string? moduleDirectory = null)
    {
        var candidateRoots = new List<string?>
        {
            moduleDirectory,
            AppContext.BaseDirectory,
            AppDomain.CurrentDomain.BaseDirectory,
            Directory.GetCurrentDirectory()
        };

        string? baseDir = candidateRoots.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p));

        if (string.IsNullOrWhiteSpace(baseDir))
        {
            baseDir = "/home/container/game/csgo/addons/counterstrikesharp/plugins/SmartRestart";
        }

        string resolvedLogsDirectory = ResolveLogsDirectory(baseDir);

        _logDirectory = resolvedLogsDirectory;

        try
        {
            Directory.CreateDirectory(_logDirectory);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SmartRestart] ERROR: Could not create/access log directory '{_logDirectory}': {ex.Message}");
            _fileLoggingEnabled = false;
        }

        // Set log file with today's date
        string fileName = $"SmartRestart_{DateTime.Now:yyyy-MM-dd}.log";
        _logFilePath = Path.Combine(_logDirectory, fileName);

        // Write initial log line
        try
        {
            lock (_lockObject)
            {
                if (_fileLoggingEnabled)
                {
                    File.AppendAllText(_logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [INFO] ========== SmartRestart Plugin Logging Started ==========\r\n");
                    File.AppendAllText(_logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [INFO] Log file location: {_logFilePath}\r\n");
                }
            }
        }
        catch (Exception ex)
        {
            _fileLoggingEnabled = false;
            Console.WriteLine($"[SmartRestart] ERROR: Failed to write initial log entry: {ex.Message}");
        }
    }

    private static string ResolveLogsDirectory(string baseDir)
    {
        DirectoryInfo? currentDir;
        try
        {
            currentDir = new DirectoryInfo(baseDir);
        }
        catch
        {
            return "/home/container/game/csgo/addons/counterstrikesharp/logs";
        }

        while (currentDir != null && !currentDir.Name.Equals("counterstrikesharp", StringComparison.OrdinalIgnoreCase))
        {
            currentDir = currentDir.Parent;
        }

        if (currentDir != null)
        {
            return Path.Combine(currentDir.FullName, "logs");
        }

        // Linux dedicated server common paths
        string[] knownPaths =
        {
            "/home/container/game/csgo/addons/counterstrikesharp/logs",
            "/game/csgo/addons/counterstrikesharp/logs"
        };

        foreach (var path in knownPaths)
        {
            try
            {
                var parent = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(parent) && Directory.Exists(parent))
                    return path;
            }
            catch
            {
                // ignore
            }
        }

        return Path.Combine(baseDir, "logs");
    }

    public void Log(string message)
    {
        LogInternal(message, LogLevel.Info);
    }

    public void LogError(string message)
    {
        LogInternal(message, LogLevel.Error);
    }

    public void LogWarning(string message)
    {
        LogInternal(message, LogLevel.Warning);
    }

    public void LogDebug(string message)
    {
        if (!DebugEnabled)
            return;

        LogInternal(message, LogLevel.Debug);
    }

    public void LogRestart(string reason, string details = "")
    {
        string message = $"RESTART EVENT: {reason}";
        if (!string.IsNullOrEmpty(details))
            message += $" | {details}";
        LogInternal(message, LogLevel.Info);
    }

    private void LogInternal(string message, LogLevel level)
    {
        try
        {
            lock (_lockObject)
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string levelStr = level.ToString().ToUpper();
                string logMessage = $"[{timestamp}] [{levelStr}] {message}";

                // Write to file
                if (_fileLoggingEnabled)
                {
                    File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
                }

                // Keep console output concise and avoid debug spam.
                if (level != LogLevel.Debug)
                {
                    var prefix = level switch
                    {
                        LogLevel.Warning => "[WARN] ",
                        LogLevel.Error => "[ERROR] ",
                        _ => string.Empty
                    };

                    Console.WriteLine($"[SmartRestart] {prefix}{message}");
                }
            }
        }
        catch (Exception ex)
        {
            _fileLoggingEnabled = false;
            Console.WriteLine($"[SmartRestart] LOGGING ERROR (file disabled): {ex.Message}");
        }
    }

    private enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}
