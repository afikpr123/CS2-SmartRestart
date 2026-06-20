using System.Text;

namespace SmartRestart;

internal static class LoadConsole
{
    private const string Blue = "\u001b[36m";
    private const string Green = "\u001b[32m";
    private const string Red = "\u001b[31m";
    private const string Yellow = "\u001b[33m";
    private const string Reset = "\u001b[0m";

    public static void WritePluginLoad(SmartRestartConfig config, string configPath, string languagePath, string logDirectory)
    {
        var enabledRestarts = config.ScheduledRestarts?.Count(r => r.Enabled) ?? 0;
        var autoEmptyEnabled = config.EnableAutoRestart && config.EmptyServerBehavior.Enabled;
        var webhookEnabled = config.DiscordWebhook.Enabled;
        var databaseEnabled = config.Database.Enabled;
        var databaseDetails = databaseEnabled
            ? $"Enabled ({config.Database.Host}:{config.Database.Port} / {config.Database.Database} / {config.Database.Username})"
            : "Disabled";
        var now = DateTime.Now;

        var (nextRestartText, nextRestartInText) = GetNextRestartText(config, now, enabledRestarts);

        var sb = new StringBuilder();

        AppendBlueLine(sb, "====================================================");
        AppendBlueLine(sb, " SMARTRESTART STARTUP");
        AppendBlueLine(sb, "====================================================");
        AppendBlueLine(sb, $" Time: {now:yyyy-MM-dd HH:mm:ss}");
        AppendBlueLine(sb, "----------------------------------------------------");
        AppendBlueLine(sb, "| Checklist Item               | Status | Details");
        AppendBlueLine(sb, "----------------------------------------------------");
        AppendStatusLine(sb, "Logger ready", true, logDirectory);
        AppendStatusLine(sb, "Config loaded", true, configPath);
        AppendStatusLine(sb, "Language loaded", true, $"{config.Language} ({languagePath})");
        AppendStatusLine(sb, "Event handlers", true, "Connect/Disconnect");
        AppendStatusLine(sb, "Scheduler timer", true, "Every 60s");
        AppendStatusLine(sb, "Auto-empty map refresh", autoEmptyEnabled, $"Delay: {config.EmptyServerBehavior.DelaySeconds}s");
        AppendStatusLine(sb, "Scheduled restarts", enabledRestarts > 0, $"Count: {enabledRestarts}");
        AppendStatusLine(sb, "Discord webhook", webhookEnabled, webhookEnabled ? "Enabled" : "Disabled");
        AppendStatusLine(sb, "Database integration", databaseEnabled, databaseDetails);
        AppendStatusLine(sb, "Debug logging", config.Logging.DebugEnabled, config.Logging.DebugEnabled ? "Enabled" : "Disabled");
        AppendBlueLine(sb, "----------------------------------------------------");

        sb.AppendLine();
        AppendScheduledRestarts(sb, config, enabledRestarts);

        sb.AppendLine();
        AppendGreenLine(sb, $"Next scheduled restart: {nextRestartText} ({nextRestartInText})");
        AppendGreenLine(sb, "SmartRestart ready.");

        Console.WriteLine(sb.ToString());
    }

    private static (string NextRestartText, string NextRestartInText) GetNextRestartText(
        SmartRestartConfig config,
        DateTime now,
        int enabledRestarts)
    {
        if (enabledRestarts <= 0)
            return ("N/A", "-");

        var nextRestart = config.ScheduledRestarts
            .Where(r => r.Enabled)
            .Select(r => new DateTime(now.Year, now.Month, now.Day, r.Hour, r.Minute, 0))
            .Select(dt => dt < now ? dt.AddDays(1) : dt)
            .OrderBy(dt => dt)
            .FirstOrDefault();

        if (nextRestart == default)
            return ("N/A", "-");

        var timeUntil = nextRestart - now;
        return (nextRestart.ToString("yyyy-MM-dd HH:mm"), $"{timeUntil.TotalMinutes:F0} min");
    }

    private static void AppendScheduledRestarts(StringBuilder sb, SmartRestartConfig config, int enabledRestarts)
    {
        if (enabledRestarts <= 0)
        {
            AppendBlueLine(sb, "Scheduled Restarts: None Configured");
            return;
        }

        AppendBlueLine(sb, "Scheduled Restarts");
        AppendBlueLine(sb, "+-------+--------------------------------------+");
        AppendBlueLine(sb, "| Time  | Description                          |");
        AppendBlueLine(sb, "+-------+--------------------------------------+");

        foreach (var restart in config.ScheduledRestarts
            .Where(r => r.Enabled)
            .OrderBy(r => r.Hour)
            .ThenBy(r => r.Minute))
        {
            AppendBlueLine(sb, $"| {restart.Hour:D2}:{restart.Minute:D2} | {restart.Description,-36} |");
        }

        AppendBlueLine(sb, "+-------+--------------------------------------+");
    }

    private static void AppendStatusLine(StringBuilder sb, string item, bool success, string details, bool isError = false)
    {
        var symbol = success ? $"{Green}✔{Blue}      " : $"{Red}✖{Blue}      ";
        var detailText = isError || details.Contains("error", StringComparison.OrdinalIgnoreCase)
            ? $"{Yellow}{details}{Blue}"
            : details;

        sb.Append(Blue);
        sb.Append($"| {item,-28} | {symbol} | {detailText}");
        sb.AppendLine(Reset);
    }

    private static void AppendBlueLine(StringBuilder sb, string text)
    {
        sb.Append(Blue);
        sb.Append(text);
        sb.AppendLine(Reset);
    }

    private static void AppendGreenLine(StringBuilder sb, string text)
    {
        sb.Append(Green);
        sb.Append(text);
        sb.AppendLine(Reset);
    }
}
