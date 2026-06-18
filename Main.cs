using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using System.Text.Json;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace SmartRestart;

public class SmartRestartPlugin : BasePlugin
{
    public override string ModuleName => "Smart Restart Plugin";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Dipsy";

    private SmartRestartConfig _config = null!;
    private LanguageConfig _lang = null!;
    private string _configPath = "";
    private string _langPath = "";
    private DateTime _serverStartTime;
    private DateTime? _lastPlayerLeaveTime = null;
    private DateTime? _lastEmptyRestart = null;
    private int _peakPlayerCount = 0;
    private Dictionary<ulong, DateTime> _playerJoinTimes = new();
    private List<TimeSpan> _completedSessions = new();
    private DateTime? _lastSignificantActivity = null;
    private Timer? _emptyServerTimer = null;
    private Timer? _scheduledRestartTimer = null;
    private DateTime? _nextScheduledRestart = null;
    private Timer? _pendingRestartTimer = null;
    private string _pendingRestartReason = "";
    private List<Timer> _warningTimers = new();
    private DatabaseManager? _databaseManager = null;
    private DiscordWebhookManager? _discordWebhook = null;
    private DateTime? _countdownStartTime = null;
    private int _countdownDuration = 0;

    private Events.PlayerEventHandler? _playerEventHandler = null;

    // Public properties for event handler access
    public DateTime ServerStartTime => _serverStartTime;
    public DateTime? LastPlayerLeaveTime { get => _lastPlayerLeaveTime; set => _lastPlayerLeaveTime = value; }
    public DateTime? LastEmptyRestart { get => _lastEmptyRestart; set => _lastEmptyRestart = value; }
    public int PeakPlayerCount { get => _peakPlayerCount; set => _peakPlayerCount = value; }
    public Dictionary<ulong, DateTime> PlayerJoinTimes => _playerJoinTimes;
    public List<TimeSpan> CompletedSessions => _completedSessions;
    public DateTime? LastSignificantActivity { get => _lastSignificantActivity; set => _lastSignificantActivity = value; }
    public Timer? EmptyServerTimer { get => _emptyServerTimer; set => _emptyServerTimer = value; }
    public DateTime? NextScheduledRestart => _nextScheduledRestart;
    public Timer? PendingRestartTimer { get => _pendingRestartTimer; set => _pendingRestartTimer = value; }
    public string PendingRestartReason { get => _pendingRestartReason; set => _pendingRestartReason = value; }
    public DateTime? CountdownStartTime => _countdownStartTime;
    public int CountdownDuration => _countdownDuration;
    public DiscordWebhookManager? DiscordWebhook => _discordWebhook;

    public override void Load(bool hotReload)
    {
        _serverStartTime = DateTime.Now;
        _configPath = Path.Combine(ModuleDirectory, "config.json");
        LoadConfig();

        // Load language file based on config
        _langPath = Path.Combine(ModuleDirectory, "lang", $"{_config.Language}.json");
        LoadLanguage();

        // Initialize Discord webhook manager
        _discordWebhook = new DiscordWebhookManager(_config, GetServerHostname, _serverStartTime);

        // Initialize database manager if enabled
        if (_config.Database.Enabled)
        {
            string hostname = GetServerHostname();
            _databaseManager = new DatabaseManager(_config.Database, hostname);
            _ = InitializeDatabaseAsync();
        }

        // Initialize event handler
        _playerEventHandler = new Events.PlayerEventHandler(
            this,
            _config,
            _lang,
            (delay, action) => AddTimer(delay, action),
            (delay, action, flags) => AddTimer(delay, action, flags)
        );

        RegisterEventHandler<EventPlayerDisconnect>(_playerEventHandler.OnPlayerDisconnect);
        RegisterEventHandler<EventPlayerConnectFull>(_playerEventHandler.OnPlayerConnect);

        // Start scheduled restart checker - check immediately then every 60 seconds
        AddTimer(1.0f, CheckScheduledRestarts); // Run once immediately
        AddTimer(60.0f, CheckScheduledRestarts, TimerFlags.REPEAT); // Then repeat every minute

        Console.WriteLine($"[SmartRestart] Plugin loaded. Auto-restart: {_config.EnableAutoRestart}");
        Console.WriteLine($"[SmartRestart] Current server time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        // Log scheduled restarts
        if (_config.ScheduledRestarts != null && _config.ScheduledRestarts.Any(r => r.Enabled))
        {
            Console.WriteLine($"[SmartRestart] Scheduled restarts configured:");
            foreach (var restart in _config.ScheduledRestarts.Where(r => r.Enabled))
            {
                Console.WriteLine($"[SmartRestart]   - {restart.Hour:D2}:{restart.Minute:D2} - {restart.Description}");
            }

            // Show next scheduled restart
            var now = DateTime.Now;
            var nextRestart = _config.ScheduledRestarts
                .Where(r => r.Enabled)
                .Select(r => new DateTime(now.Year, now.Month, now.Day, r.Hour, r.Minute, 0))
                .Select(dt => dt < now ? dt.AddDays(1) : dt)
                .OrderBy(dt => dt)
                .FirstOrDefault();

            if (nextRestart != default(DateTime))
            {
                var timeUntil = nextRestart - now;
                Console.WriteLine($"[SmartRestart] Next scheduled restart: {nextRestart:yyyy-MM-dd HH:mm} (in {timeUntil.TotalHours:F1} hours / {timeUntil.TotalMinutes:F0} minutes)");
            }
        }
        else
        {
            Console.WriteLine($"[SmartRestart] No scheduled restarts configured.");
        }

        // Send server online notification if webhook enabled
        if (!hotReload && _config.DiscordWebhook.Enabled && _config.DiscordWebhook.OnlineEmbed.Enabled)
        {
            AddTimer(5.0f, () => _ = _discordWebhook?.SendServerOnlineEmbed());
        }
    }

    private string GetServerHostname()
    {
        try
        {
            // Get hostname from ConVar
            var hostnameConVar = ConVar.Find("hostname");
            if (hostnameConVar != null)
            {
                string hostname = hostnameConVar.StringValue ?? "Unknown Server";
                return hostname.Trim();
            }

            Console.WriteLine("[SmartRestart] WARNING: Could not find hostname ConVar");
            return "Unknown Server";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SmartRestart] Error getting hostname: {ex.Message}");
            return "Unknown Server";
        }
    }

    private async Task InitializeDatabaseAsync()
    {
        if (_databaseManager == null)
            return;

        try
        {
            bool success = await _databaseManager.InitializeAsync();
            if (!success)
            {
                Console.WriteLine("[SmartRestart] Database initialization failed. !serverrestart command will not work.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SmartRestart] Database initialization error: {ex.Message}");
        }
    }

    private void LoadConfig()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                string json = File.ReadAllText(_configPath);
                var options = new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
                _config = JsonSerializer.Deserialize<SmartRestartConfig>(json, options) ?? new SmartRestartConfig();
                Console.WriteLine($"[SmartRestart] Configuration loaded from {_configPath}");
            }
            else
            {
                _config = new SmartRestartConfig();
                Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
                File.WriteAllText(_configPath, GenerateConfigWithComments(_config));
                Console.WriteLine($"[SmartRestart] Default configuration created at {_configPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SmartRestart] Error loading config: {ex.Message}");
            _config = new SmartRestartConfig();
        }
    }

    private string GenerateConfigWithComments(SmartRestartConfig config)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  // Language settings");
        sb.AppendLine($"  \"Language\": \"{config.Language}\", // Available: en, ar, es, fr, de, he");
        sb.AppendLine();
        sb.AppendLine("  // Auto-restart settings");
        sb.AppendLine($"  \"EnableAutoRestart\": {config.EnableAutoRestart.ToString().ToLower()}, // Restart server automatically when empty");
        sb.AppendLine($"  \"DelayAfterLastPlayerLeaves\": {config.DelayAfterLastPlayerLeaves}, // Seconds to wait after last player leaves");
        sb.AppendLine($"  \"MinimumUptimeMinutes\": {config.MinimumUptimeMinutes}, // Minimum uptime (minutes) for MANUAL restarts - prevents accidental rapid restarts");
        sb.AppendLine($"  \"MinimumUptimeForEmptyRestartHours\": {config.MinimumUptimeForEmptyRestartHours}, // Minimum uptime (hours) for AUTO empty-server restarts - prevents spam");
        sb.AppendLine();
        sb.AppendLine("  // Smart empty restart settings");
        sb.AppendLine($"  \"EmptyRestartCooldownMinutes\": {config.EmptyRestartCooldownMinutes}, // Cooldown between empty restarts - prevents restart spam");
        sb.AppendLine($"  \"MinimumPlayersBeforeEmptyRestart\": {config.MinimumPlayersBeforeEmptyRestart}, // Server must have had this many players before restarting when empty");
        sb.AppendLine($"  \"RequirePlayerActivityForEmptyRestart\": {config.RequirePlayerActivityForEmptyRestart.ToString().ToLower()}, // Only restart if server had player activity since last restart");
        sb.AppendLine();
        sb.AppendLine("  // Advanced smart restart (time-aware + session-based)");
        sb.AppendLine("  \"SmartEmptyRestart\": {");
        sb.AppendLine($"    \"Enabled\": {config.SmartEmptyRestart.Enabled.ToString().ToLower()}, // Enable advanced smart restart logic");
        sb.AppendLine($"    \"Strategy\": \"{config.SmartEmptyRestart.Strategy}\", // Options: Immediate, Smart");
        sb.AppendLine("    \"PeakHours\": {");
        sb.AppendLine($"      \"Enabled\": {config.SmartEmptyRestart.PeakHours.Enabled.ToString().ToLower()},");
        sb.AppendLine($"      \"StartHour\": {config.SmartEmptyRestart.PeakHours.StartHour}, // 24-hour format");
        sb.AppendLine($"      \"EndHour\": {config.SmartEmptyRestart.PeakHours.EndHour},");
        sb.AppendLine($"      \"DelayMinutes\": {config.SmartEmptyRestart.PeakHours.DelayMinutes}, // Delay during peak hours");
        sb.AppendLine($"      \"OffPeakDelayMinutes\": {config.SmartEmptyRestart.PeakHours.OffPeakDelayMinutes} // Delay during off-peak");
        sb.AppendLine("    },");
        sb.AppendLine("    \"SessionBased\": {");
        sb.AppendLine($"      \"Enabled\": {config.SmartEmptyRestart.SessionBased.Enabled.ToString().ToLower()},");
        sb.AppendLine($"      \"MinimumSessionLengthMinutes\": {config.SmartEmptyRestart.SessionBased.MinimumSessionLengthMinutes}, // Minimum session to count");
        sb.AppendLine($"      \"MinimumTotalPlaytimeMinutes\": {config.SmartEmptyRestart.SessionBased.MinimumTotalPlaytimeMinutes}, // Total playtime required");
        sb.AppendLine($"      \"RecentActivityWindowMinutes\": {config.SmartEmptyRestart.SessionBased.RecentActivityWindowMinutes} // Wait if recent activity");
        sb.AppendLine("    },");
        sb.AppendLine($"    \"MaximumEmptyWaitMinutes\": {config.SmartEmptyRestart.MaximumEmptyWaitMinutes} // Never wait longer than this");
        sb.AppendLine("  },");
        sb.AppendLine();
        sb.AppendLine("  // Scheduled restarts");
        sb.AppendLine("  \"ScheduledRestarts\": [");
        for (int i = 0; i < config.ScheduledRestarts.Count; i++)
        {
            var restart = config.ScheduledRestarts[i];
            sb.AppendLine("    {");
            sb.AppendLine($"      \"Enabled\": {restart.Enabled.ToString().ToLower()},");
            sb.AppendLine($"      \"Hour\": {restart.Hour}, // 24-hour format (0-23)");
            sb.AppendLine($"      \"Minute\": {restart.Minute},");
            sb.AppendLine($"      \"Description\": \"{restart.Description} ({restart.Hour:D2}:{restart.Minute:D2})\"");
            sb.AppendLine(i < config.ScheduledRestarts.Count - 1 ? "    }," : "    }");
        }
        sb.AppendLine("  ],");
        sb.AppendLine();
        sb.AppendLine("  // Warning messages before restart");
        sb.AppendLine("  \"WarningMessages\": {");
        sb.AppendLine($"    \"Enabled\": {config.WarningMessages.Enabled.ToString().ToLower()},");
        sb.Append("    \"WarningTimes\": [");
        sb.Append(string.Join(", ", config.WarningMessages.WarningTimes));
        sb.AppendLine("], // Seconds before restart to warn players");
        sb.AppendLine($"    \"ShowCenterAlert\": {config.WarningMessages.ShowCenterAlert.ToString().ToLower()}, // Show large center-screen alerts");
        sb.AppendLine($"    \"CenterAlertDuration\": {config.WarningMessages.CenterAlertDuration} // How long center alerts stay visible (seconds)");
        sb.AppendLine("  },");
        sb.AppendLine();
        sb.AppendLine("  // Restart command (quit for Pelican/Pterodactyl, exit, or _restart)");
        sb.AppendLine($"  \"RestartCommand\": \"{config.RestartCommand}\",");
        sb.AppendLine();
        sb.AppendLine("  // Chat message prefix (supports color tags like {gold}, {blue}, {red}, etc.)");
        sb.AppendLine($"  \"ChatPrefix\": \"{config.ChatPrefix}\",");
        sb.AppendLine();
        sb.AppendLine("  // Discord webhook notifications");
        sb.AppendLine("  \"DiscordWebhook\": {");
        sb.AppendLine($"    \"Enabled\": {config.DiscordWebhook.Enabled.ToString().ToLower()},");
        sb.AppendLine($"    \"WebhookUrl\": \"{config.DiscordWebhook.WebhookUrl}\",");
        sb.AppendLine($"    \"EmbedStyle\": \"{config.DiscordWebhook.EmbedStyle}\", // Options: simple, detailed, professional");
        sb.AppendLine($"    \"FooterImageUrl\": \"{config.DiscordWebhook.FooterImageUrl}\",");
        sb.AppendLine("    \"RestartEmbed\": {");
        sb.AppendLine($"      \"Enabled\": {config.DiscordWebhook.RestartEmbed.Enabled.ToString().ToLower()},");
        sb.AppendLine($"      \"Color\": \"{config.DiscordWebhook.RestartEmbed.Color}\", // RGB format: R, G, B");
        sb.AppendLine($"      \"Title\": \"{config.DiscordWebhook.RestartEmbed.Title}\",");
        sb.AppendLine($"      \"ShowUptime\": {config.DiscordWebhook.RestartEmbed.ShowUptime.ToString().ToLower()},");
        sb.AppendLine($"      \"ShowPlayers\": {config.DiscordWebhook.RestartEmbed.ShowPlayers.ToString().ToLower()},");
        sb.AppendLine($"      \"ShowReason\": {config.DiscordWebhook.RestartEmbed.ShowReason.ToString().ToLower()},");
        sb.AppendLine($"      \"ShowEstimatedDowntime\": {config.DiscordWebhook.RestartEmbed.ShowEstimatedDowntime.ToString().ToLower()},");
        sb.AppendLine($"      \"EstimatedDowntimeSeconds\": {config.DiscordWebhook.RestartEmbed.EstimatedDowntimeSeconds}");
        sb.AppendLine("    },");
        sb.AppendLine("    \"OnlineEmbed\": {");
        sb.AppendLine($"      \"Enabled\": {config.DiscordWebhook.OnlineEmbed.Enabled.ToString().ToLower()},");
        sb.AppendLine($"      \"Color\": \"{config.DiscordWebhook.OnlineEmbed.Color}\", // RGB format: R, G, B");
        sb.AppendLine($"      \"Title\": \"{config.DiscordWebhook.OnlineEmbed.Title}\",");
        sb.AppendLine($"      \"ShowDowntime\": {config.DiscordWebhook.OnlineEmbed.ShowDowntime.ToString().ToLower()},");
        sb.AppendLine($"      \"ShowMap\": {config.DiscordWebhook.OnlineEmbed.ShowMap.ToString().ToLower()},");
        sb.AppendLine($"      \"ShowConnectInfo\": {config.DiscordWebhook.OnlineEmbed.ShowConnectInfo.ToString().ToLower()}");
        sb.AppendLine("    },");
        sb.AppendLine("    \"ManualEmbed\": {");
        sb.AppendLine($"      \"Enabled\": {config.DiscordWebhook.ManualEmbed.Enabled.ToString().ToLower()},");
        sb.AppendLine($"      \"Color\": \"{config.DiscordWebhook.ManualEmbed.Color}\", // RGB format: R, G, B");
        sb.AppendLine($"      \"Title\": \"{config.DiscordWebhook.ManualEmbed.Title}\",");
        sb.AppendLine($"      \"ShowUptime\": {config.DiscordWebhook.ManualEmbed.ShowUptime.ToString().ToLower()},");
        sb.AppendLine($"      \"ShowPlayers\": {config.DiscordWebhook.ManualEmbed.ShowPlayers.ToString().ToLower()},");
        sb.AppendLine($"      \"ShowAdmin\": {config.DiscordWebhook.ManualEmbed.ShowAdmin.ToString().ToLower()},");
        sb.AppendLine($"      \"ShowEstimatedDowntime\": {config.DiscordWebhook.ManualEmbed.ShowEstimatedDowntime.ToString().ToLower()},");
        sb.AppendLine($"      \"EstimatedDowntimeSeconds\": {config.DiscordWebhook.ManualEmbed.EstimatedDowntimeSeconds}");
        sb.AppendLine("    },");
        sb.AppendLine($"    \"SendOnScheduledRestart\": {config.DiscordWebhook.SendOnScheduledRestart.ToString().ToLower()},");
        sb.AppendLine($"    \"SendOnManualRestart\": {config.DiscordWebhook.SendOnManualRestart.ToString().ToLower()},");
        sb.AppendLine($"    \"SendOnEmptyServerRestart\": {config.DiscordWebhook.SendOnEmptyServerRestart.ToString().ToLower()},");
        sb.AppendLine($"    \"SendWarnings\": {config.DiscordWebhook.SendWarnings.ToString().ToLower()}");
        sb.AppendLine("  },");
        sb.AppendLine();
        sb.AppendLine("  // SimpleAdmin database integration for permission checks");
        sb.AppendLine("  \"Database\": {");
        sb.AppendLine($"    \"Enabled\": {config.Database.Enabled.ToString().ToLower()}, // Enable to use SimpleAdmin permissions for !serverrestart command");
        sb.AppendLine($"    \"Host\": \"{config.Database.Host}\",");
        sb.AppendLine($"    \"Port\": {config.Database.Port},");
        sb.AppendLine($"    \"Database\": \"{config.Database.Database}\",");
        sb.AppendLine($"    \"Username\": \"{config.Database.Username}\",");
        sb.AppendLine($"    \"Password\": \"{config.Database.Password}\",");
        sb.AppendLine($"    \"RequiredPermission\": \"{config.Database.RequiredPermission}\" // Permission flag checked in sa_admins_groups and sa_admins tables");
        sb.AppendLine("  }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private void LoadLanguage()
    {
        try
        {
            if (File.Exists(_langPath))
            {
                string json = File.ReadAllText(_langPath);
                _lang = JsonSerializer.Deserialize<LanguageConfig>(json) ?? new LanguageConfig();
                Console.WriteLine($"[SmartRestart] Language file loaded from {_langPath}");
            }
            else
            {
                _lang = new LanguageConfig();
                string json = JsonSerializer.Serialize(_lang, new JsonSerializerOptions { WriteIndented = true });
                Directory.CreateDirectory(Path.GetDirectoryName(_langPath)!);
                File.WriteAllText(_langPath, json);
                Console.WriteLine($"[SmartRestart] Default language file created at {_langPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SmartRestart] Error loading language file: {ex.Message}");
            _lang = new LanguageConfig();
        }
    }

    public string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalDays >= 1)
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
        else if (uptime.TotalHours >= 1)
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
        else
            return $"{uptime.Minutes}m {uptime.Seconds}s";
    }

    public bool ShouldScheduleEmptyRestart()
    {
        if (!_config.SmartEmptyRestart.Enabled || _config.SmartEmptyRestart.Strategy == "Immediate")
        {
            return true; // Simple immediate restart
        }

        // Check 1: Cooldown period - prevent too frequent empty restarts
        if (_lastEmptyRestart != null)
        {
            var timeSinceLastRestart = DateTime.Now - _lastEmptyRestart.Value;
            if (timeSinceLastRestart.TotalMinutes < _config.EmptyRestartCooldownMinutes)
            {
                var remainingCooldown = _config.EmptyRestartCooldownMinutes - timeSinceLastRestart.TotalMinutes;
                Console.WriteLine($"[SmartRestart] Empty restart skipped: Cooldown active ({remainingCooldown:F1} minutes remaining).");
                return false;
            }
        }

        // Check 2: Require player activity - server must have had minimum players
        if (_config.RequirePlayerActivityForEmptyRestart)
        {
            if (_peakPlayerCount < _config.MinimumPlayersBeforeEmptyRestart)
            {
                Console.WriteLine($"[SmartRestart] Empty restart skipped: No player activity (peak: {_peakPlayerCount}, required: {_config.MinimumPlayersBeforeEmptyRestart}).");
                _peakPlayerCount = 0; // Reset since we're skipping
                return false;
            }
        }

        // Check 3: Session-based logic
        if (_config.SmartEmptyRestart.SessionBased.Enabled)
        {
            var totalPlaytime = _completedSessions.Sum(s => s.TotalMinutes);
            if (totalPlaytime < _config.SmartEmptyRestart.SessionBased.MinimumTotalPlaytimeMinutes)
            {
                Console.WriteLine($"[SmartRestart] Empty restart skipped: Insufficient playtime ({totalPlaytime:F1}/{_config.SmartEmptyRestart.SessionBased.MinimumTotalPlaytimeMinutes} minutes).");
                return false;
            }
        }

        // Check 4: Recent activity window
        if (_config.SmartEmptyRestart.SessionBased.Enabled && _lastSignificantActivity != null)
        {
            var timeSinceActivity = DateTime.Now - _lastSignificantActivity.Value;
            if (timeSinceActivity.TotalMinutes < _config.SmartEmptyRestart.SessionBased.RecentActivityWindowMinutes)
            {
                Console.WriteLine($"[SmartRestart] Empty restart delayed: Recent activity detected ({timeSinceActivity.TotalMinutes:F1} minutes ago).");
                return false;
            }
        }

        // Check 5: Avoid restarting right before a scheduled restart
        if (_nextScheduledRestart != null)
        {
            var timeUntilScheduled = _nextScheduledRestart.Value - DateTime.Now;
            if (timeUntilScheduled.TotalMinutes < 30) // Within 30 minutes of scheduled restart
            {
                Console.WriteLine($"[SmartRestart] Empty restart skipped: Scheduled restart in {timeUntilScheduled.TotalMinutes:F0} minutes.");
                return false;
            }
        }

        return true;
    }

    public int CalculateSmartDelay()
    {
        if (!_config.SmartEmptyRestart.Enabled || _config.SmartEmptyRestart.Strategy == "Immediate")
        {
            return _config.DelayAfterLastPlayerLeaves;
        }

        // Time-of-day aware delay
        if (_config.SmartEmptyRestart.PeakHours.Enabled)
        {
            var currentHour = DateTime.Now.Hour;
            var isPeakTime = false;

            // Handle peak hours that may span midnight
            if (_config.SmartEmptyRestart.PeakHours.StartHour <= _config.SmartEmptyRestart.PeakHours.EndHour)
            {
                isPeakTime = currentHour >= _config.SmartEmptyRestart.PeakHours.StartHour && 
                             currentHour <= _config.SmartEmptyRestart.PeakHours.EndHour;
            }
            else
            {
                // Peak hours span midnight (e.g., 22:00 to 02:00)
                isPeakTime = currentHour >= _config.SmartEmptyRestart.PeakHours.StartHour || 
                             currentHour <= _config.SmartEmptyRestart.PeakHours.EndHour;
            }

            var delayMinutes = isPeakTime 
                ? _config.SmartEmptyRestart.PeakHours.DelayMinutes 
                : _config.SmartEmptyRestart.PeakHours.OffPeakDelayMinutes;

            // Cap at maximum wait time
            delayMinutes = Math.Min(delayMinutes, _config.SmartEmptyRestart.MaximumEmptyWaitMinutes);

            var timeType = isPeakTime ? "peak hours" : "off-peak hours";
            Console.WriteLine($"[SmartRestart] Smart delay: {delayMinutes} minutes ({timeType}, current hour: {currentHour:D2}:00)");

            return delayMinutes * 60; // Convert to seconds
        }

        return _config.DelayAfterLastPlayerLeaves;
    }

    private void CheckScheduledRestarts()
    {
        if (_config.ScheduledRestarts == null || _config.ScheduledRestarts.Count == 0)
            return;

        DateTime now = DateTime.Now;

        // Get the maximum warning time to know when to trigger early
        var maxWarningTime = _config.WarningMessages.Enabled && _config.WarningMessages.WarningTimes.Count > 0
            ? _config.WarningMessages.WarningTimes.Max()
            : 60; // Default to 1 minute if no warnings configured

        var triggerWindowMinutes = (maxWarningTime / 60.0) + 1; // Convert to minutes and add 1 minute buffer

        foreach (var restart in _config.ScheduledRestarts.Where(r => r.Enabled))
        {
            DateTime scheduledTime = new DateTime(now.Year, now.Month, now.Day, restart.Hour, restart.Minute, 0);

            // If scheduled time is in the past today, check for tomorrow
            if (scheduledTime < now)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            // Check if we should trigger a restart
            TimeSpan timeUntilRestart = scheduledTime - now;

            // Trigger if within warning window and not already scheduled
            if (timeUntilRestart.TotalMinutes <= triggerWindowMinutes && timeUntilRestart.TotalSeconds > 0)
            {
                // Check if this restart hasn't been triggered yet
                if (_nextScheduledRestart == null || Math.Abs((_nextScheduledRestart.Value - scheduledTime).TotalMinutes) > 1)
                {
                    _nextScheduledRestart = scheduledTime;
                    var secondsUntilRestart = (int)timeUntilRestart.TotalSeconds;

                    Console.WriteLine($"[SmartRestart] ⚠️ RESTART TRIGGERED: {restart.Description} scheduled for {scheduledTime:HH:mm} (in {timeUntilRestart.TotalMinutes:F1} minutes / {secondsUntilRestart} seconds)");
                    InitiateScheduledRestart(secondsUntilRestart, restart.Description);
                }
            }
            // Only log upcoming restarts once when they enter 30-minute window
            else if (timeUntilRestart.TotalMinutes <= 30 && timeUntilRestart.TotalMinutes > triggerWindowMinutes)
            {
                // Only log if this is a new notification (not already logged in previous minute)
                if (_nextScheduledRestart == null || Math.Abs((_nextScheduledRestart.Value - scheduledTime).TotalMinutes) > 1)
                {
                    Console.WriteLine($"[SmartRestart] Upcoming: {restart.Description} at {scheduledTime:HH:mm} (in {timeUntilRestart.TotalMinutes:F0} minutes)");
                }
            }
        }
    }

    private void InitiateScheduledRestart(int secondsUntilRestart, string description)
    {
        var playerCount = Utilities.GetPlayers().Count(p => p?.IsValid == true && !p.IsBot);

        Console.WriteLine($"[SmartRestart] Scheduled restart initiating: {description} in {secondsUntilRestart} seconds ({secondsUntilRestart / 60.0:F1} minutes).");

        if (playerCount == 0)
        {
            // Server empty, restart at scheduled time but save info for late joiners
            Console.WriteLine($"[SmartRestart] Server is empty, restart will occur at scheduled time.");
            Console.WriteLine($"[SmartRestart] Warnings will start if players join before restart.");

            _pendingRestartReason = description;

            if (_config.DiscordWebhook.SendOnScheduledRestart)
            {
                _ = _discordWebhook?.SendRestartEmbed(description, "Scheduled Restart");
            }

            // Store the restart timer so we can cancel it if players join
            _pendingRestartTimer = AddTimer(Math.Max(1, secondsUntilRestart), () => 
            {
                _pendingRestartTimer = null;
                _pendingRestartReason = "";
                PerformRestart(sendDiscordMessage: false, reason: description, initiator: "Scheduled Restart");
            });
        }
        else
        {
            // Players online, send warnings and schedule restart
            Console.WriteLine($"[SmartRestart] {playerCount} player(s) online, sending warnings before restart.");

            if (_config.DiscordWebhook.SendOnScheduledRestart && secondsUntilRestart > 30)
            {
                var timeFormatted = LanguageConfig.FormatTime(secondsUntilRestart);
                _ = _discordWebhook?.SendDiscordWebhook($"⏰ Scheduled Restart: {description}", 
                    $"Restart in: {timeFormatted}\nPlayers online: {playerCount}", 
                    1838576); // RGB(28, 109, 240) - Blue
            }
            ScheduleRestartWithWarnings(totalSeconds: secondsUntilRestart, reason: description, initiator: "Scheduled Restart");
        }
    }

    public void ScheduleRestartWithWarnings(int totalSeconds, string reason = "Scheduled restart", string initiator = "System")
    {
        Console.WriteLine($"[SmartRestart] ScheduleRestartWithWarnings called: totalSeconds={totalSeconds}, reason={reason}");

        // Clear any existing warning timers
        foreach (var timer in _warningTimers)
        {
            timer?.Kill();
        }
        _warningTimers.Clear();

        if (!_config.WarningMessages.Enabled)
        {
            Console.WriteLine("[SmartRestart] Warning messages are DISABLED in config!");
            // No warnings, just restart
            AddTimer(totalSeconds, () => PerformRestart(sendDiscordMessage: true, reason: reason, initiator: initiator));
            return;
        }

        Console.WriteLine($"[SmartRestart] Warnings enabled. ShowCenterAlert={_config.WarningMessages.ShowCenterAlert}");
        Console.WriteLine($"[SmartRestart] Configured warning times: {string.Join(", ", _config.WarningMessages.WarningTimes)}");

        // Schedule chat warnings at configured times (5 min, 3 min, 2 min, 1 min, etc.)
        int scheduledChatWarnings = 0;
        foreach (var warningTime in _config.WarningMessages.WarningTimes.OrderByDescending(t => t))
        {
            if (warningTime < totalSeconds)
            {
                float delay = totalSeconds - warningTime;
                var timer = AddTimer(delay, () =>
                {
                    var timeFormatted = LanguageConfig.FormatTime(warningTime);

                    // Build message with custom prefix from config
                    var prefix = LanguageConfig.ProcessColors(_config.ChatPrefix);
                    var message = _lang.RestartWarning.Replace("{time}", timeFormatted);
                    var fullMessage = $" {prefix} {LanguageConfig.ProcessColors(message)}";

                    Console.WriteLine($"[SmartRestart] Sending chat warning: {timeFormatted} left");
                    Server.PrintToChatAll(fullMessage);
                });
                _warningTimers.Add(timer);
                scheduledChatWarnings++;
            }
        }
        Console.WriteLine($"[SmartRestart] Scheduled {scheduledChatWarnings} chat warnings");

        // Schedule continuous center alerts for last 30 seconds
        if (_config.WarningMessages.ShowCenterAlert)
        {
            // Start the countdown when there are 30 seconds left
            if (totalSeconds >= 30)
            {
                float delayUntilCountdown = totalSeconds - 30;

                var countdownTimer = AddTimer(delayUntilCountdown, () =>
                {
                    Console.WriteLine($"[SmartRestart] Starting 30-second center alert countdown");
                    StartContinuousCenterCountdown(30);
                });
                _warningTimers.Add(countdownTimer);
                Console.WriteLine($"[SmartRestart] Center countdown will start in {delayUntilCountdown} seconds");
            }
            else
            {
                // Less than 30 seconds total, start countdown immediately
                Console.WriteLine($"[SmartRestart] Starting immediate center alert countdown for {totalSeconds} seconds");
                StartContinuousCenterCountdown(totalSeconds);
            }
        }
        else
        {
            Console.WriteLine("[SmartRestart] Center alerts are DISABLED in config!");
        }

        // Final restart with proper reason
        AddTimer(totalSeconds, () => PerformRestart(sendDiscordMessage: true, reason: reason, initiator: initiator));
        Console.WriteLine($"[SmartRestart] Final restart scheduled in {totalSeconds} seconds ({totalSeconds / 60.0:F1} minutes)");
    }

    public bool CanRestart(bool isManualRestart = false)
    {
        // For manual restarts (command), check MinimumUptimeMinutes
        if (isManualRestart)
        {
            TimeSpan uptime = DateTime.Now - _serverStartTime;
            if (uptime.TotalMinutes < _config.MinimumUptimeMinutes)
            {
                Console.WriteLine($"[SmartRestart] Manual restart blocked: Minimum uptime not met ({uptime.TotalMinutes:F1}min/{_config.MinimumUptimeMinutes}min).");
                return false;
            }
        }

        return true;
    }

    public void PerformRestart(bool sendDiscordMessage = true, string reason = "Server maintenance", string initiator = "System")
    {
        Console.WriteLine($"[SmartRestart] ========== PERFORMING RESTART ==========");
        Console.WriteLine($"[SmartRestart] Reason: {reason}");
        Console.WriteLine($"[SmartRestart] Initiator: {initiator}");

        // Send Discord notification first (fire and forget, but give it a moment)
        if (sendDiscordMessage && _config.DiscordWebhook.Enabled && _config.DiscordWebhook.RestartEmbed.Enabled)
        {
            try
            {
                Console.WriteLine($"[SmartRestart] Sending Discord restart notification...");
                _ = _discordWebhook?.SendRestartEmbed(reason, initiator);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SmartRestart] Discord notification failed (non-fatal): {ex.Message}");
            }
        }

        // Send chat message to notify players
        try
        {
            var prefix = LanguageConfig.ProcessColors(_config.ChatPrefix);
            var message = LanguageConfig.ProcessColors(_lang.RestartingNow);
            Server.PrintToChatAll($" {prefix} {message}");
            Console.WriteLine($"[SmartRestart] Chat notification sent to all players");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SmartRestart] Chat notification failed (non-fatal): {ex.Message}");
        }

        // Clean up active timers and warnings to prevent interference
        try
        {
            Console.WriteLine($"[SmartRestart] Cleaning up active timers...");
            foreach (var timer in _warningTimers)
            {
                try
                {
                    timer?.Kill();
                }
                catch { }
            }
            _warningTimers.Clear();

            _emptyServerTimer?.Kill();
            _emptyServerTimer = null;

            _scheduledRestartTimer?.Kill();
            _scheduledRestartTimer = null;

            _pendingRestartTimer?.Kill();
            _pendingRestartTimer = null;

            Console.WriteLine($"[SmartRestart] All timers cleaned up");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SmartRestart] Timer cleanup failed (non-fatal): {ex.Message}");
        }

        Console.WriteLine($"[SmartRestart] Executing restart command: {_config.RestartCommand}");

        // Execute restart command with minimal delay - server.ExecuteCommand is synchronous but safer with brief delay
        AddTimer(1.0f, () =>
        {
            try
            {
                Console.WriteLine($"[SmartRestart] ========== EXECUTING: {_config.RestartCommand} ==========");
                Server.ExecuteCommand(_config.RestartCommand);
                Console.WriteLine($"[SmartRestart] Restart command executed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SmartRestart] ERROR executing restart command: {ex.Message}");
                Console.WriteLine($"[SmartRestart] Stack trace: {ex.StackTrace}");
            }
        });
    }

    private void StartContinuousCenterCountdown(float totalSeconds)
    {
        _countdownStartTime = DateTime.Now;
        _countdownDuration = (int)Math.Ceiling(totalSeconds);

        Console.WriteLine($"[SmartRestart] Starting center countdown for {_countdownDuration} seconds");

        // Show initial message immediately
        ShowCountdownToPlayers(_countdownDuration);

        // Create a repeating timer that updates every 1 second
        var countdownTimer = AddTimer(1.0f, () =>
        {
            if (_countdownStartTime == null) return;

            var elapsed = (DateTime.Now - _countdownStartTime.Value).TotalSeconds;
            var secondsRemaining = _countdownDuration - (int)elapsed;

            if (secondsRemaining > 0)
            {
                ShowCountdownToPlayers(secondsRemaining);

                if (secondsRemaining % 5 == 0 || secondsRemaining <= 5)
                {
                    Console.WriteLine($"[SmartRestart] Countdown: {secondsRemaining}s remaining");
                }
            }
            else
            {
                _countdownStartTime = null;
                Console.WriteLine($"[SmartRestart] Countdown completed");
            }
        }, TimerFlags.REPEAT);

        _warningTimers.Add(countdownTimer);

        // Add a cleanup timer
        var cleanupTimer = AddTimer(totalSeconds + 1.0f, () =>
        {
            countdownTimer?.Kill();
            _countdownStartTime = null;
        });

        _warningTimers.Add(cleanupTimer);
    }

    private void ShowCountdownToPlayers(int secondsRemaining)
    {
        var timeFormatted = secondsRemaining == 1 ? "1 SECOND" : $"{secondsRemaining} SECONDS";

        // Try PrintToCenterAlert instead - it's designed for longer messages
        var message = $"⚠️ SERVER RESTART ⚠️\n{timeFormatted}";

        var players = Utilities.GetPlayers().Where(p => p?.IsValid == true && !p.IsBot);

        foreach (var player in players)
        {
            if (player?.IsValid == true)
            {
                // PrintToCenterAlert is designed for alerts and might be more stable
                player.PrintToCenterAlert(message);
            }
        }
    }

    private void ShowCenterAlert(string message, float durationSeconds)
    {
        var players = Utilities.GetPlayers().Where(p => p?.IsValid == true && !p.IsBot);

        // Format message with HTML for larger red text
        var htmlMessage = $"<font class='fontSize-l' color='red'>{message}</font>";

        foreach (var player in players)
        {
            if (player?.IsValid == true && player.PawnIsAlive)
            {
                // PrintToCenterHtml duration parameter is in seconds, but as int
                // Round up to ensure at least 2 seconds visibility
                player.PrintToCenterHtml(htmlMessage, Math.Max(2, (int)Math.Ceiling(durationSeconds)));
            }
        }

        Console.WriteLine($"[SmartRestart] Center alert shown: {message} (duration: {durationSeconds}s)");
    }

    [ConsoleCommand("css_smartrestart", "Schedules a server restart")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/root")]
    public void OnSmartRestartCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (!CanRestart(isManualRestart: true))
        {
            command.ReplyToCommand($"[SmartRestart] Cannot restart: Minimum uptime of {_config.MinimumUptimeMinutes} minutes not met.");
            return;
        }

        var playerCount = Utilities.GetPlayers().Count(p => p?.IsValid == true && !p.IsBot);
        var adminName = player?.PlayerName ?? "Console";

        if (playerCount == 0)
        {
            command.ReplyToCommand("[SmartRestart] Server is empty. Restarting immediately...");
            if (_config.DiscordWebhook.SendOnManualRestart)
            {
                _ = _discordWebhook?.SendRestartEmbed($"Manual restart by {adminName}", "Manual Restart");
            }
            PerformRestart(sendDiscordMessage: false); // Don't send duplicate
        }
        else
        {
            command.ReplyToCommand("[SmartRestart] Server restart scheduled in 5 minutes!");
            if (_config.DiscordWebhook.SendOnManualRestart)
            {
                _ = _discordWebhook?.SendDiscordWebhook($"🔧 Manual Restart by {adminName}", 
                    $"Restart in: 5 minutes\nPlayers online: {playerCount}", 
                    1838576); // RGB(28, 109, 240) - Blue
            }
            ScheduleRestartWithWarnings(totalSeconds: 300, reason: $"Manual restart by {adminName}", initiator: "Manual Restart");
        }
    }

    [ConsoleCommand("css_smartrestart_reload", "Reloads the SmartRestart configuration")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/root")]
    public void OnReloadConfigCommand(CCSPlayerController? player, CommandInfo command)
    {
        LoadConfig();
        LoadLanguage();

        // Reinitialize database manager if enabled
        if (_config.Database.Enabled)
        {
            string hostname = GetServerHostname();
            _databaseManager = new DatabaseManager(_config.Database, hostname);
            _ = InitializeDatabaseAsync();
        }
        else
        {
            _databaseManager = null;
        }

        command.ReplyToCommand("[SmartRestart] Configuration and language files reloaded!");
    }

    [ConsoleCommand("css_serverrestart", "Restart the server (requires SimpleAdmin permission)")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnRestartChatCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
        {
            command.ReplyToCommand("[SmartRestart] This command can only be used by players.");
            return;
        }

        // Check if database is enabled
        if (!_config.Database.Enabled || _databaseManager == null)
        {
            command.ReplyToCommand(" {red}[SmartRestart]{default} Database integration is not enabled.");
            Console.WriteLine("[SmartRestart] Player attempted to use !restart but database is not enabled.");
            return;
        }

        // Get player's SteamID
        var steamId = player.SteamID.ToString();
        var playerName = player.PlayerName;

        // Check permission asynchronously with ConfigureAwait(false) to avoid sync context issues
        _ = Task.Run(async () =>
        {
            try
            {
                Console.WriteLine($"[SmartRestart] Checking permission for {playerName} (SteamID: {steamId})");

                bool hasPermission = false;
                try
                {
                    hasPermission = await _databaseManager.CheckPlayerPermissionAsync(steamId).ConfigureAwait(false);
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"[SmartRestart] Database exception: {dbEx.Message}");
                    hasPermission = false;
                }

                Console.WriteLine($"[SmartRestart] Permission check result: {hasPermission}");

                // Execute on next tick to avoid threading issues
                Server.NextFrame(() =>
                {
                    // Re-validate player is still connected
                    if (player == null || !player.IsValid || !player.PlayerPawn.IsValid)
                    {
                        Console.WriteLine($"[SmartRestart] Player disconnected before permission check completed.");
                        return;
                    }

                    if (!hasPermission)
                    {
                        player.PrintToChat($" {LanguageConfig.ProcessColors(_config.ChatPrefix)} {LanguageConfig.ProcessColors("{red}You don't have permission to use this command.{default}")}");
                        Console.WriteLine($"[SmartRestart] {player.PlayerName} ({steamId}) attempted to restart but lacks permission.");
                        return;
                    }

                    // Check manual restart minimum uptime
                    if (!CanRestart(isManualRestart: true))
                    {
                        player.PrintToChat($" {LanguageConfig.ProcessColors(_config.ChatPrefix)} {LanguageConfig.ProcessColors($"{{red}}Cannot restart: Minimum uptime of {_config.MinimumUptimeMinutes} minutes not met.{{default}}")}");
                        return;
                    }

                    // Player has permission, proceed with restart
                    var playerCount = Utilities.GetPlayers().Count(p => p?.IsValid == true && !p.IsBot);

                    if (playerCount <= 1) // Only the admin
                    {
                        player.PrintToChat($" {LanguageConfig.ProcessColors(_config.ChatPrefix)} {LanguageConfig.ProcessColors("{green}Server is empty. Restarting immediately...{default}")}");
                        if (_config.DiscordWebhook.SendOnManualRestart)
                        {
                            _ = _discordWebhook?.SendRestartEmbed($"Manual restart by {player.PlayerName}", "Manual Restart", isManual: true);
                        }
                        PerformRestart(sendDiscordMessage: false); // Don't send duplicate
                    }
                    else
                    {
                        player.PrintToChat($" {LanguageConfig.ProcessColors(_config.ChatPrefix)} {LanguageConfig.ProcessColors("{green}Server restart scheduled in 5 minutes!{default}")}");
                        Server.PrintToChatAll($" {LanguageConfig.ProcessColors(_config.ChatPrefix)} {LanguageConfig.ProcessColors($"{{yellow}}Server restart initiated by {player.PlayerName}{{default}}")}");

                        if (_config.DiscordWebhook.SendOnManualRestart)
                        {
                            _ = _discordWebhook?.SendRestartEmbed($"Manual restart by {player.PlayerName}", "Manual Restart", isManual: true);
                        }
                        ScheduleRestartWithWarnings(totalSeconds: 300, reason: $"Manual restart by {player.PlayerName}", initiator: "Manual Restart");
                    }

                    Console.WriteLine($"[SmartRestart] {player.PlayerName} ({steamId}) initiated a server restart.");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SmartRestart] Error checking permission: {ex.Message}");
                Server.NextFrame(() =>
                {
                    if (player != null && player.IsValid && player.PlayerPawn.IsValid)
                    {
                        player.PrintToChat($" {LanguageConfig.ProcessColors(_config.ChatPrefix)} {LanguageConfig.ProcessColors("{red}An error occurred. Please contact an administrator.{default}")}");
                    }
                });
            }
        });
    }

    public override void Unload(bool hotReload)
    {
        _emptyServerTimer?.Kill();
        _scheduledRestartTimer?.Kill();
        foreach (var timer in _warningTimers)
        {
            timer?.Kill();
        }
        _warningTimers.Clear();
    }
}
