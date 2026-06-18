using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text.Json;
using System.Net.Http;
using System.Text;

namespace SmartRestart;

public class DiscordWebhookManager
{
    private readonly SmartRestartConfig _config;
    private readonly Func<string> _getServerHostname;
    private static readonly HttpClient _httpClient = new();
    private DateTime? _restartInitiatedTime = null;
    private int _playersBeforeRestart = 0;
    private readonly DateTime _serverStartTime;

    public DiscordWebhookManager(SmartRestartConfig config, Func<string> getServerHostname, DateTime serverStartTime)
    {
        _config = config;
        _getServerHostname = getServerHostname;
        _serverStartTime = serverStartTime;
    }

    public async Task SendDiscordWebhook(string title, string description, int? color = null)
    {
        if (!_config.DiscordWebhook.Enabled || string.IsNullOrEmpty(_config.DiscordWebhook.WebhookUrl))
            return;

        try
        {
            var embed = new
            {
                title = title,
                description = description,
                color = color ?? 3447003,
                timestamp = DateTime.UtcNow.ToString("o"),
                footer = new
                {
                    text = _getServerHostname(),
                    icon_url = _config.DiscordWebhook.FooterImageUrl
                }
            };

            var payload = new
            {
                username = "SmartRestart",
                embeds = new[] { embed }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_config.DiscordWebhook.WebhookUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[SmartRestart] Discord webhook failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SmartRestart] Error sending Discord webhook: {ex.Message}");
        }
    }

    public async Task SendRestartEmbed(string reason, string initiatedBy, bool isManual = false)
    {
        var embedConfig = isManual && _config.DiscordWebhook.ManualEmbed.Enabled 
            ? (dynamic)_config.DiscordWebhook.ManualEmbed 
            : (dynamic)_config.DiscordWebhook.RestartEmbed;

        if (!_config.DiscordWebhook.Enabled || !embedConfig.Enabled)
            return;

        _restartInitiatedTime = DateTime.Now;
        _playersBeforeRestart = Utilities.GetPlayers().Count(p => p?.IsValid == true && !p.IsBot);

        var uptime = DateTime.Now - _serverStartTime;
        var uptimeFormatted = FormatUptime(uptime);
        var estimatedDowntime = isManual 
            ? _config.DiscordWebhook.ManualEmbed.EstimatedDowntimeSeconds 
            : _config.DiscordWebhook.RestartEmbed.EstimatedDowntimeSeconds;

        string description = _config.DiscordWebhook.EmbedStyle.ToLower() switch
        {
            "simple" => BuildSimpleRestartEmbed(uptimeFormatted, reason, initiatedBy, estimatedDowntime),
            "professional" => BuildProfessionalRestartEmbed(uptimeFormatted, reason, initiatedBy, estimatedDowntime),
            _ => BuildDetailedRestartEmbed(uptimeFormatted, reason, initiatedBy, estimatedDowntime)
        };

        await SendDiscordWebhook(
            embedConfig.Title,
            description,
            embedConfig.GetColorDecimal()
        );
    }

    public async Task SendServerOnlineEmbed()
    {
        if (!_config.DiscordWebhook.Enabled || !_config.DiscordWebhook.OnlineEmbed.Enabled)
            return;

        var downtime = _restartInitiatedTime.HasValue
            ? DateTime.Now - _restartInitiatedTime.Value
            : TimeSpan.Zero;

        var downtimeFormatted = _restartInitiatedTime.HasValue
            ? $"{(int)downtime.TotalMinutes}m {downtime.Seconds}s"
            : "Unknown";

        var currentMap = Server.MapName ?? "Unknown";
        var maxPlayers = Server.MaxPlayers;
        string? connectInfo = null;

        string description = _config.DiscordWebhook.EmbedStyle.ToLower() switch
        {
            "simple" => BuildSimpleOnlineEmbed(downtimeFormatted, currentMap, maxPlayers, connectInfo),
            "professional" => BuildProfessionalOnlineEmbed(downtimeFormatted, currentMap, maxPlayers, connectInfo),
            _ => BuildDetailedOnlineEmbed(downtimeFormatted, currentMap, maxPlayers, connectInfo)
        };

        await SendDiscordWebhook(
            _config.DiscordWebhook.OnlineEmbed.Title,
            description,
            _config.DiscordWebhook.OnlineEmbed.GetColorDecimal()
        );

        _restartInitiatedTime = null;
    }

    private string BuildSimpleRestartEmbed(string uptime, string reason, string initiatedBy, int estimatedDowntime)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"⏰ **Restart Time:** <t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:F>");

        if (_config.DiscordWebhook.RestartEmbed.ShowUptime)
            sb.AppendLine($"⚡ **Uptime:** {uptime}");

        if (_config.DiscordWebhook.RestartEmbed.ShowPlayers && _playersBeforeRestart > 0)
            sb.AppendLine($"👥 **Players Online:** {_playersBeforeRestart}");

        if (_config.DiscordWebhook.RestartEmbed.ShowReason)
            sb.AppendLine($"📋 **Reason:** {reason}");

        sb.AppendLine($"🔧 **Restart Type:** {initiatedBy}");

        if (_config.DiscordWebhook.RestartEmbed.ShowEstimatedDowntime)
            sb.AppendLine($"\n⏳ **Expected downtime:** ~{estimatedDowntime / 60} minutes");

        return sb.ToString();
    }

    private string BuildDetailedRestartEmbed(string uptime, string reason, string initiatedBy, int estimatedDowntime)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine($"⏰ **Restart Initiated:** <t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:F>");

        if (_config.DiscordWebhook.RestartEmbed.ShowUptime)
            sb.AppendLine($"⚡ **Server Uptime:** {uptime}");

        if (_config.DiscordWebhook.RestartEmbed.ShowPlayers && _playersBeforeRestart > 0)
            sb.AppendLine($"👥 **Players Online:** {_playersBeforeRestart}");

        if (_config.DiscordWebhook.RestartEmbed.ShowReason)
            sb.AppendLine($"📋 **Restart Reason:** {reason}");

        sb.AppendLine($"🔧 **Initiated By:** {initiatedBy}");

        if (_config.DiscordWebhook.RestartEmbed.ShowEstimatedDowntime)
        {
            sb.AppendLine();
            sb.AppendLine($"⏱️ **Expected Downtime:** ~{estimatedDowntime / 60} minutes");
            sb.AppendLine($"🕒 **ETA Back Online:** <t:{DateTimeOffset.Now.AddSeconds(estimatedDowntime).ToUnixTimeSeconds()}:t>");
        }

        sb.AppendLine();

        return sb.ToString();
    }

    private string BuildProfessionalRestartEmbed(string uptime, string reason, string initiatedBy, int estimatedDowntime)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```");
        sb.AppendLine("═════════════════════");
        sb.AppendLine("   SERVER RESTARTING  ");
        sb.AppendLine("═════════════════════```");
        sb.AppendLine();

        sb.AppendLine("**📊 Server Status**");
        if (_config.DiscordWebhook.RestartEmbed.ShowUptime)
            sb.AppendLine($"├─ Uptime: `{uptime}`");

        if (_config.DiscordWebhook.RestartEmbed.ShowPlayers && _playersBeforeRestart > 0)
            sb.AppendLine($"├─ Players Online: `{_playersBeforeRestart}`");

        sb.AppendLine($"└─ Status: `Shutting down...`");
        sb.AppendLine();

        sb.AppendLine("**📋 Restart Information**");
        if (_config.DiscordWebhook.RestartEmbed.ShowReason)
            sb.AppendLine($"├─ Reason: `{reason}`");

        sb.AppendLine($"├─ Type: `{initiatedBy}`");
        sb.AppendLine($"├─ Time: <t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:T>");

        if (_config.DiscordWebhook.RestartEmbed.ShowEstimatedDowntime)
            sb.AppendLine($"└─ Downtime: `~{estimatedDowntime / 60} min`");

        return sb.ToString();
    }

    private string BuildSimpleOnlineEmbed(string downtime, string map, int maxPlayers, string? connectInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"✅ **Server Status:** Online");

        if (_config.DiscordWebhook.OnlineEmbed.ShowDowntime && _restartInitiatedTime.HasValue)
            sb.AppendLine($"⏱️ **Downtime:** {downtime}");

        if (_config.DiscordWebhook.OnlineEmbed.ShowMap)
            sb.AppendLine($"🗺️ **Map:** {map}");

        sb.AppendLine($"👥 **Slots:** 0/{maxPlayers}");

        if (_config.DiscordWebhook.OnlineEmbed.ShowConnectInfo && !string.IsNullOrEmpty(connectInfo))
            sb.AppendLine($"\n🔗 **Connect:** `{connectInfo}`");

        return sb.ToString();
    }

    private string BuildDetailedOnlineEmbed(string downtime, string map, int maxPlayers, string? connectInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine();

        if (_config.DiscordWebhook.OnlineEmbed.ShowDowntime && _restartInitiatedTime.HasValue)
        {
            sb.AppendLine($"⏱️ **Restart Completed:** <t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:T>");
            sb.AppendLine($"🕒 **Total Downtime:** {downtime}");
            sb.AppendLine();
        }

        if (_config.DiscordWebhook.OnlineEmbed.ShowMap)
        {
            sb.AppendLine("ℹ️ **Server Information**");
            sb.AppendLine($"├─ Map: `{map}`");
            sb.AppendLine($"├─ Slots: `0/{maxPlayers}` (Available)");
            sb.AppendLine($"└─ Status: 🟢 **Running**");
            sb.AppendLine();
        }

        if (_config.DiscordWebhook.OnlineEmbed.ShowConnectInfo && !string.IsNullOrEmpty(connectInfo))
        {
            sb.AppendLine($"🔗 **Connect Now**");
            sb.AppendLine($"`{connectInfo}`");
            sb.AppendLine();
        }

        sb.AppendLine();

        return sb.ToString();
    }

    private string BuildProfessionalOnlineEmbed(string downtime, string map, int maxPlayers, string? connectInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```");
        sb.AppendLine("═════════════════════");
        sb.AppendLine("   SERVER RESTORED    ");
        sb.AppendLine("═════════════════════```");
        sb.AppendLine();

        if (_config.DiscordWebhook.OnlineEmbed.ShowDowntime && _restartInitiatedTime.HasValue)
        {
            sb.AppendLine("**📊 Performance Metrics**");
            sb.AppendLine($"├─ Downtime: `{downtime}`");
            sb.AppendLine($"├─ Status: `🟢 Online`");
            sb.AppendLine($"└─ Response: `Optimal`");
            sb.AppendLine();
        }

        if (_config.DiscordWebhook.OnlineEmbed.ShowMap)
        {
            sb.AppendLine("**🎮 Server Ready**");
            sb.AppendLine($"├─ Map: `{map}`");
            sb.AppendLine($"├─ Players: `0/{maxPlayers}`");
            sb.AppendLine($"└─ Slots: `Available`");
            sb.AppendLine();
        }

        if (_config.DiscordWebhook.OnlineEmbed.ShowConnectInfo && !string.IsNullOrEmpty(connectInfo))
        {
            sb.AppendLine($"**🔗 Connection**");
            sb.AppendLine($"`{connectInfo}`");
        }

        return sb.ToString();
    }

    private string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalDays >= 1)
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
        else if (uptime.TotalHours >= 1)
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
        else
            return $"{uptime.Minutes}m {uptime.Seconds}s";
    }
}
