using System.Text.Json.Serialization;

namespace SmartRestart;

public class SmartRestartConfig
{
    [JsonPropertyName("Language")]
    public string Language { get; set; } = "en"; // Available: en, ar, es, fr, de, he

    [JsonPropertyName("EnableAutoRestart")]
    public bool EnableAutoRestart { get; set; } = true;

    [JsonPropertyName("DelayAfterLastPlayerLeaves")]
    public int DelayAfterLastPlayerLeaves { get; set; } = 60;

    [JsonPropertyName("MinimumUptimeMinutes")]
    public int MinimumUptimeMinutes { get; set; } = 5;

    [JsonPropertyName("MinimumUptimeForEmptyRestartHours")]
    public int MinimumUptimeForEmptyRestartHours { get; set; } = 4;

    [JsonPropertyName("EmptyRestartCooldownMinutes")]
    public int EmptyRestartCooldownMinutes { get; set; } = 30;

    [JsonPropertyName("MinimumPlayersBeforeEmptyRestart")]
    public int MinimumPlayersBeforeEmptyRestart { get; set; } = 1;

    [JsonPropertyName("RequirePlayerActivityForEmptyRestart")]
    public bool RequirePlayerActivityForEmptyRestart { get; set; } = true;

    [JsonPropertyName("SmartEmptyRestart")]
    public SmartEmptyRestartConfig SmartEmptyRestart { get; set; } = new();

    [JsonPropertyName("ScheduledRestarts")]
    public List<ScheduledRestart> ScheduledRestarts { get; set; } = new();

    [JsonPropertyName("WarningMessages")]
    public WarningMessagesConfig WarningMessages { get; set; } = new();

    [JsonPropertyName("RestartCommand")]
    public string RestartCommand { get; set; } = "quit";

    [JsonPropertyName("ChatPrefix")]
    public string ChatPrefix { get; set; } = "[{gold}SmartRestart{default}]";

    [JsonPropertyName("DiscordWebhook")]
    public DiscordWebhookConfig DiscordWebhook { get; set; } = new();

    [JsonPropertyName("Database")]
    public DatabaseConfig Database { get; set; } = new();
}

public class ScheduledRestart
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("Hour")]
    public int Hour { get; set; }

    [JsonPropertyName("Minute")]
    public int Minute { get; set; }

    [JsonPropertyName("Description")]
    public string Description { get; set; } = "";
}

public class WarningMessagesConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("WarningTimes")]
    public List<int> WarningTimes { get; set; } = new() { 300, 240, 180, 120, 60, 30 }; // Chat warnings: 5min, 4min, 3min, 2min, 1min, 30sec

    [JsonPropertyName("ShowCenterAlert")]
    public bool ShowCenterAlert { get; set; } = true;

    [JsonPropertyName("CenterAlertDuration")]
    public int CenterAlertDuration { get; set; } = 5; // Not used for countdown (kept for compatibility)
}

public class DiscordWebhookConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("WebhookUrl")]
    public string WebhookUrl { get; set; } = "";

    [JsonPropertyName("EmbedStyle")]
    public string EmbedStyle { get; set; } = "detailed"; // simple, detailed, professional

    [JsonPropertyName("FooterImageUrl")]
    public string FooterImageUrl { get; set; } = "https://png.pngtree.com/png-vector/20250826/ourmid/pngtree-rack-illustration-isometric-server-png-image_17213766.webp";

    [JsonPropertyName("RestartEmbed")]
    public RestartEmbedConfig RestartEmbed { get; set; } = new();

    [JsonPropertyName("OnlineEmbed")]
    public OnlineEmbedConfig OnlineEmbed { get; set; } = new();

    [JsonPropertyName("ManualEmbed")]
    public ManualEmbedConfig ManualEmbed { get; set; } = new();

    [JsonPropertyName("SendOnScheduledRestart")]
    public bool SendOnScheduledRestart { get; set; } = true;

    [JsonPropertyName("SendOnManualRestart")]
    public bool SendOnManualRestart { get; set; } = true;

    [JsonPropertyName("SendOnEmptyServerRestart")]
    public bool SendOnEmptyServerRestart { get; set; } = true;

    [JsonPropertyName("SendWarnings")]
    public bool SendWarnings { get; set; } = false;
}

public class RestartEmbedConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("Color")]
    public string Color { get; set; } = "231, 76, 60"; // RGB format: R, G, B (Orange/Red)

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "🔄 Server Restarting";

    [JsonPropertyName("ShowUptime")]
    public bool ShowUptime { get; set; } = true;

    [JsonPropertyName("ShowPlayers")]
    public bool ShowPlayers { get; set; } = true;

    [JsonPropertyName("ShowReason")]
    public bool ShowReason { get; set; } = true;

    [JsonPropertyName("ShowEstimatedDowntime")]
    public bool ShowEstimatedDowntime { get; set; } = true;

    [JsonPropertyName("EstimatedDowntimeSeconds")]
    public int EstimatedDowntimeSeconds { get; set; } = 120;

    public int GetColorDecimal()
    {
        try
        {
            var parts = Color.Split(',').Select(p => int.Parse(p.Trim())).ToArray();
            if (parts.Length == 3)
                return (parts[0] << 16) + (parts[1] << 8) + parts[2];
        }
        catch { }
        return 15158332; // Default orange/red
    }
}

public class OnlineEmbedConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("Color")]
    public string Color { get; set; } = "46, 204, 113"; // RGB format: R, G, B (Green)

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "✅ Server Back Online!";

    [JsonPropertyName("ShowDowntime")]
    public bool ShowDowntime { get; set; } = true;

    [JsonPropertyName("ShowMap")]
    public bool ShowMap { get; set; } = true;

    [JsonPropertyName("ShowConnectInfo")]
    public bool ShowConnectInfo { get; set; } = true;

    public int GetColorDecimal()
    {
        try
        {
            var parts = Color.Split(',').Select(p => int.Parse(p.Trim())).ToArray();
            if (parts.Length == 3)
                return (parts[0] << 16) + (parts[1] << 8) + parts[2];
        }
        catch { }
        return 3066993; // Default green
    }
}

public class DatabaseConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("Host")]
    public string Host { get; set; } = "localhost";

    [JsonPropertyName("Port")]
    public int Port { get; set; } = 3306;

    [JsonPropertyName("Database")]
    public string Database { get; set; } = "cs2_simpleadmin";

    [JsonPropertyName("Username")]
    public string Username { get; set; } = "root";

    [JsonPropertyName("Password")]
    public string Password { get; set; } = "";

    [JsonPropertyName("RequiredPermission")]
    public string RequiredPermission { get; set; } = "@css/smartrestart";
}

public class ManualEmbedConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("Color")]
    public string Color { get; set; } = "28, 109, 240"; // RGB format: R, G, B (Blue)

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "🔧 Manual Server Restart";

    [JsonPropertyName("ShowUptime")]
    public bool ShowUptime { get; set; } = true;

    [JsonPropertyName("ShowPlayers")]
    public bool ShowPlayers { get; set; } = true;

    [JsonPropertyName("ShowAdmin")]
    public bool ShowAdmin { get; set; } = true;

    [JsonPropertyName("ShowEstimatedDowntime")]
    public bool ShowEstimatedDowntime { get; set; } = true;

    [JsonPropertyName("EstimatedDowntimeSeconds")]
    public int EstimatedDowntimeSeconds { get; set; } = 120;

    public int GetColorDecimal()
    {
        try
        {
            var parts = Color.Split(',').Select(p => int.Parse(p.Trim())).ToArray();
            if (parts.Length == 3)
                return (parts[0] << 16) + (parts[1] << 8) + parts[2];
        }
        catch { }
        return 1857776; // Default blue (28, 109, 240)
    }
}

public class SmartEmptyRestartConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("Strategy")]
    public string Strategy { get; set; } = "Smart"; // Options: Immediate, Smart

    [JsonPropertyName("PeakHours")]
    public PeakHoursConfig PeakHours { get; set; } = new();

    [JsonPropertyName("SessionBased")]
    public SessionBasedConfig SessionBased { get; set; } = new();

    [JsonPropertyName("MaximumEmptyWaitMinutes")]
    public int MaximumEmptyWaitMinutes { get; set; } = 30;
}

public class PeakHoursConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("StartHour")]
    public int StartHour { get; set; } = 18; // 6 PM

    [JsonPropertyName("EndHour")]
    public int EndHour { get; set; } = 23; // 11 PM

    [JsonPropertyName("DelayMinutes")]
    public int DelayMinutes { get; set; } = 15;

    [JsonPropertyName("OffPeakDelayMinutes")]
    public int OffPeakDelayMinutes { get; set; } = 3;
}

public class SessionBasedConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("MinimumSessionLengthMinutes")]
    public int MinimumSessionLengthMinutes { get; set; } = 5;

    [JsonPropertyName("MinimumTotalPlaytimeMinutes")]
    public int MinimumTotalPlaytimeMinutes { get; set; } = 30;

    [JsonPropertyName("RecentActivityWindowMinutes")]
    public int RecentActivityWindowMinutes { get; set; } = 10;
}

