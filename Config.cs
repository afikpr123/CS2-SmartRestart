using System.Text.Json.Serialization;

namespace SmartRestart;

public class SmartRestartConfig
{
    [JsonPropertyName("Language")]
    public string Language { get; set; } = "en"; // Available: en, he

    [JsonPropertyName("EnableAutoRestart")]
    public bool EnableAutoRestart { get; set; } = true;

    [JsonPropertyName("MinimumUptimeMinutes")]
    public int MinimumUptimeMinutes { get; set; } = 5;

    [JsonPropertyName("ScheduledRestarts")]
    public List<ScheduledRestart> ScheduledRestarts { get; set; } = new();

    [JsonPropertyName("EmptyServerBehavior")]
    public EmptyServerBehaviorConfig EmptyServerBehavior { get; set; } = new();

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

    [JsonPropertyName("Logging")]
    public LoggingConfig Logging { get; set; } = new();
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

    [JsonPropertyName("PermissionCacheSeconds")]
    public int PermissionCacheSeconds { get; set; } = 60;
}

public class LoggingConfig
{
    [JsonPropertyName("DebugEnabled")]
    public bool DebugEnabled { get; set; } = false;
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

public class EmptyServerBehaviorConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("DelaySeconds")]
    public int DelaySeconds { get; set; } = 180;

    [JsonPropertyName("ExecuteOnceUntilPlayerJoins")]
    public bool ExecuteOnceUntilPlayerJoins { get; set; } = true;

    [JsonPropertyName("RepeatWhileStillEmpty")]
    public bool RepeatWhileStillEmpty { get; set; } = false;

    [JsonPropertyName("RepeatIntervalSeconds")]
    public int RepeatIntervalSeconds { get; set; } = 28800;

    [JsonPropertyName("SkipIfScheduledRestartWithinMinutes")]
    public int SkipIfScheduledRestartWithinMinutes { get; set; } = 15;
}

