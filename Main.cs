using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using System.Text;
using System.Text.Json;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace SmartRestart;

public class SmartRestartPlugin : BasePlugin
{
    public override string ModuleName => "Smart Restart Plugin";
    public override string ModuleVersion => "1.0.2";
    public override string ModuleAuthor => "Dipsy";

    private SmartRestartConfig _config = null!;
    private LanguageConfig _lang = null!;
    private string _configPath = "";
    private string _langPath = "";
    private Logger _logger = null!;
    private DateTime _serverStartTime;
    private DateTime? _lastPlayerLeaveTime = null;
    private DateTime? _lastEmptyRestart = null;
    private int _peakPlayerCount = 0;
    private Dictionary<ulong, DateTime> _playerJoinTimes = new();
    private bool _emptyMapActionExecuted = false;
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
    public bool EmptyMapActionExecuted { get => _emptyMapActionExecuted; set => _emptyMapActionExecuted = value; }
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

        // Initialize logger first
        _logger = new Logger(ModuleDirectory);

        LoadConfig();
        _logger.DebugEnabled = _config.Logging.DebugEnabled;

        // Load language file based on config
        _langPath = Path.Combine(ModuleDirectory, "lang", $"{_config.Language}.json");
        LoadLanguage();

        // Initialize Discord webhook manager
        _discordWebhook = new DiscordWebhookManager(_config, GetServerHostname, _serverStartTime);

        // Initialize database manager if enabled
        if (_config.Database.Enabled)
        {
            string hostname = GetServerHostname();
            _databaseManager = new DatabaseManager(_config.Database, hostname, startupTableMode: true);
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
        _scheduledRestartTimer = AddTimer(60.0f, CheckScheduledRestarts, TimerFlags.REPEAT); // Then repeat every minute

        LoadConsole.WritePluginLoad(_config, _configPath, _langPath, _logger.LogDirectory);

        _logger?.LogDebug("Plugin load complete");

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
            }
            else
            {
                _config = new SmartRestartConfig();
                Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
                File.WriteAllText(_configPath, GenerateConfigWithComments(_config));
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
        sb.AppendLine("  // Auto behavior");
        sb.AppendLine($"  \"EnableAutoRestart\": {config.EnableAutoRestart.ToString().ToLower()}, // Enable auto behavior when server becomes empty");
        sb.AppendLine($"  \"MinimumUptimeMinutes\": {config.MinimumUptimeMinutes}, // Minimum uptime (minutes) for MANUAL restarts");
        sb.AppendLine();
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
        sb.AppendLine("  // Empty server behavior (no restart)");
        sb.AppendLine("  \"EmptyServerBehavior\": {");
        sb.AppendLine($"    \"Enabled\": {config.EmptyServerBehavior.Enabled.ToString().ToLower()}, // Change level to current map when empty");
        sb.AppendLine($"    \"DelaySeconds\": {config.EmptyServerBehavior.DelaySeconds}, // Wait before executing map command after server is empty");
        sb.AppendLine($"    \"ExecuteOnceUntilPlayerJoins\": {config.EmptyServerBehavior.ExecuteOnceUntilPlayerJoins.ToString().ToLower()}, // Avoid map-change spam while server remains empty");
        sb.AppendLine($"    \"RepeatWhileStillEmpty\": {config.EmptyServerBehavior.RepeatWhileStillEmpty.ToString().ToLower()}, // Repeat map refresh while still empty");
        sb.AppendLine($"    \"RepeatIntervalSeconds\": {config.EmptyServerBehavior.RepeatIntervalSeconds}, // Repeat interval when RepeatWhileStillEmpty is true");
        sb.AppendLine($"    \"SkipIfScheduledRestartWithinMinutes\": {config.EmptyServerBehavior.SkipIfScheduledRestartWithinMinutes} // Skip map-change if scheduled restart is close");
        sb.AppendLine("  },");
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
        sb.AppendLine($"    \"RequiredPermission\": \"{config.Database.RequiredPermission}\", // Permission flag checked in sa_admins_groups and sa_admins tables");
        sb.AppendLine($"    \"PermissionCacheSeconds\": {config.Database.PermissionCacheSeconds} // Cache command permission checks to reduce database load");
        sb.AppendLine("  },");
        sb.AppendLine();
        sb.AppendLine("  // Logging");
        sb.AppendLine("  \"Logging\": {");
        sb.AppendLine($"    \"DebugEnabled\": {config.Logging.DebugEnabled.ToString().ToLower()} // Enable only while troubleshooting; writes verbose debug logs");
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
            }
            else
            {
                _lang = new LanguageConfig();
                string json = JsonSerializer.Serialize(_lang, new JsonSerializerOptions { WriteIndented = true });
                Directory.CreateDirectory(Path.GetDirectoryName(_langPath)!);
                File.WriteAllText(_langPath, json);
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

    public int CountHumanPlayers()
    {
        return Utilities.GetPlayers().Count(p => p?.IsValid == true && !p.IsBot);
    }

    public void DebugLog(string message)
    {
        _logger?.LogDebug(message);
    }

    public string? GetCurrentMapName()
    {
        try
        {
            // Use the ConVar to get current map name
            var mapCvar = ConVar.Find("cs_map_name");
            if (mapCvar != null && !string.IsNullOrEmpty(mapCvar.StringValue))
            {
                return mapCvar.StringValue;
            }

            // Fallback: try to get from Server.MapName if available
            if (!string.IsNullOrEmpty(Server.MapName))
            {
                return Server.MapName;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"Failed to get current map name: {ex.Message}");
        }
        return null;
    }

    public bool IsWorkshopMap(string mapName)
    {
        return TryExtractWorkshopId(mapName, out _);
    }

    public bool TryExtractWorkshopId(string mapName, out string workshopId)
    {
        workshopId = string.Empty;
        if (string.IsNullOrWhiteSpace(mapName))
            return false;

        // workshop/123456789/map_name
        if (mapName.StartsWith("workshop/", StringComparison.OrdinalIgnoreCase))
        {
            var parts = mapName.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && long.TryParse(parts[1], out _))
            {
                workshopId = parts[1];
                return true;
            }
        }

        // pure workshop id
        if (long.TryParse(mapName, out _))
        {
            workshopId = mapName;
            return true;
        }

        return false;
    }

    public void ChangeToCurrentMap()
    {
        try
        {
            var currentMap = GetCurrentMapName();
            if (string.IsNullOrEmpty(currentMap))
            {
                _logger?.LogWarning("Could not determine current map name");
                Console.WriteLine("[SmartRestart] WARNING: Could not determine current map name");
                return;
            }

            _logger?.Log($"Changing level to current map: {currentMap}");
            Console.WriteLine($"[SmartRestart] Changing level to current map: {currentMap}");

            // Check if it's a workshop map
            if (TryExtractWorkshopId(currentMap, out var workshopId))
            {
                // Workshop map: use host_workshop_map command, then fallback to map command.
                AddTimer(_config.EmptyServerBehavior.DelaySeconds, () =>
                {
                    try
                    {
                        _logger?.Log($"Executing: host_workshop_map {workshopId}");
                        var command = $"host_workshop_map {workshopId}";
                        Server.ExecuteCommand(command);
                        _logger?.Log("Workshop map change command executed");
                        SendCommandExecutionDiscordStatus("Empty server map change", command, true);

                        // Fallback attempt for environments where workshop command may not resolve.
                        AddTimer(3.0f, () =>
                        {
                            try
                            {
                                var fallbackCommand = $"map {currentMap}";
                                _logger?.LogWarning($"Workshop fallback: executing {fallbackCommand}");
                                Server.ExecuteCommand(fallbackCommand);
                                SendCommandExecutionDiscordStatus("Workshop map fallback", fallbackCommand, true);
                            }
                            catch (Exception fallbackEx)
                            {
                                _logger?.LogError($"Workshop fallback map command failed: {fallbackEx.Message}");
                                SendCommandExecutionDiscordStatus("Workshop map fallback", $"map {currentMap}", false, fallbackEx.Message);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"Failed to execute workshop map change: {ex.Message}");
                        Console.WriteLine($"[SmartRestart] ERROR: Failed to execute workshop map change: {ex.Message}");
                        SendCommandExecutionDiscordStatus("Empty server map change", $"host_workshop_map {workshopId}", false, ex.Message);

                        // Requested fallback behavior
                        try
                        {
                            var fallbackCommand = $"map {currentMap}";
                            _logger?.LogWarning($"Falling back to map command: {fallbackCommand}");
                            Server.ExecuteCommand(fallbackCommand);
                            SendCommandExecutionDiscordStatus("Empty server map fallback", fallbackCommand, true);
                        }
                        catch (Exception fallbackEx)
                        {
                            _logger?.LogError($"Fallback map command also failed: {fallbackEx.Message}");
                            SendCommandExecutionDiscordStatus("Empty server map fallback", $"map {currentMap}", false, fallbackEx.Message);
                        }
                    }
                });
            }
            else
            {
                // Official map: use map command
                AddTimer(_config.EmptyServerBehavior.DelaySeconds, () =>
                {
                    try
                    {
                        var command = $"map {currentMap}";
                        _logger?.Log($"Executing: {command}");
                        Server.ExecuteCommand(command);
                        _logger?.Log("Map change command executed");
                        SendCommandExecutionDiscordStatus("Empty server map change", command, true);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"Failed to execute map change: {ex.Message}");
                        Console.WriteLine($"[SmartRestart] ERROR: Failed to execute map change: {ex.Message}");
                        SendCommandExecutionDiscordStatus("Empty server map change", $"map {currentMap}", false, ex.Message);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Exception in ChangeToCurrentMap: {ex.Message}");
            Console.WriteLine($"[SmartRestart] ERROR: Exception in ChangeToCurrentMap: {ex.Message}");
        }
    }

    public bool ShouldScheduleEmptyMapAction()
    {
        if (!_config.EnableAutoRestart || !_config.EmptyServerBehavior.Enabled)
        {
            return false;
        }

        // Do not spam empty-map action while server remains empty.
        if (_config.EmptyServerBehavior.ExecuteOnceUntilPlayerJoins &&
            !_config.EmptyServerBehavior.RepeatWhileStillEmpty &&
            _emptyMapActionExecuted)
        {
            return false;
        }

        // Skip map-change if a scheduled restart is close.
        var nextScheduledFromConfig = GetNextScheduledRestartFromConfig(DateTime.Now);
        if (nextScheduledFromConfig != null)
        {
            var timeUntilScheduled = nextScheduledFromConfig.Value - DateTime.Now;
            if (timeUntilScheduled.TotalMinutes > 0 &&
                timeUntilScheduled.TotalMinutes <= _config.EmptyServerBehavior.SkipIfScheduledRestartWithinMinutes)
            {
                Console.WriteLine($"[SmartRestart] Empty map change skipped: scheduled restart in {timeUntilScheduled.TotalMinutes:F0} minutes.");
                return false;
            }
        }

        return true;
    }

    private DateTime? GetNextScheduledRestartFromConfig(DateTime now)
    {
        if (_config.ScheduledRestarts == null || _config.ScheduledRestarts.Count == 0)
        {
            return null;
        }

        var next = _config.ScheduledRestarts
            .Where(r => r.Enabled)
            .Select(r => new DateTime(now.Year, now.Month, now.Day, r.Hour, r.Minute, 0))
            .Select(dt => dt <= now ? dt.AddDays(1) : dt)
            .OrderBy(dt => dt)
            .FirstOrDefault();

        return next == default ? null : next;
    }

    private void CheckScheduledRestarts()
    {
        if (_config.ScheduledRestarts == null || _config.ScheduledRestarts.Count == 0)
        {
            _logger?.LogDebug("No scheduled restarts configured");
            return;
        }

        DateTime now = DateTime.Now;
        _logger?.LogDebug($"Checking scheduled restarts. Current time: {now:yyyy-MM-dd HH:mm:ss}");

        // Get the maximum warning time to know when to trigger early
        var maxWarningTime = _config.WarningMessages.Enabled && _config.WarningMessages.WarningTimes.Count > 0
            ? _config.WarningMessages.WarningTimes.Max()
            : 60; // Default to 1 minute if no warnings configured

        var triggerWindowMinutes = (maxWarningTime / 60.0) + 1; // Convert to minutes and add 1 minute buffer
        _logger?.LogDebug($"Trigger window: {triggerWindowMinutes:F2} minutes (max warning: {maxWarningTime}s)");

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
            _logger?.LogDebug($"Restart '{restart.Description}' - Scheduled: {scheduledTime:yyyy-MM-dd HH:mm:ss} | Time until: {timeUntilRestart.TotalMinutes:F2} minutes");

            // Trigger if within warning window and not already scheduled
            if (timeUntilRestart.TotalMinutes <= triggerWindowMinutes && timeUntilRestart.TotalSeconds > 0)
            {
                _logger?.LogDebug($"Restart '{restart.Description}' - WITHIN TRIGGER WINDOW ({timeUntilRestart.TotalMinutes:F2} min <= {triggerWindowMinutes:F2} min)");

                // Check if this restart hasn't been triggered yet (compare by exact time, not fuzzy 1-minute window)
                if (_nextScheduledRestart == null || _nextScheduledRestart.Value.Date != scheduledTime.Date || 
                    _nextScheduledRestart.Value.Hour != scheduledTime.Hour || 
                    _nextScheduledRestart.Value.Minute != scheduledTime.Minute)
                {
                    _nextScheduledRestart = scheduledTime;
                    var secondsUntilRestart = (int)timeUntilRestart.TotalSeconds;

                    _logger?.LogRestart(restart.Description, $"Scheduled time: {scheduledTime:yyyy-MM-dd HH:mm:ss} | Delay: {secondsUntilRestart}s");
                    Console.WriteLine($"[SmartRestart] ⚠️ RESTART TRIGGERED: {restart.Description} scheduled for {scheduledTime:HH:mm} (in {timeUntilRestart.TotalMinutes:F1} minutes / {secondsUntilRestart} seconds)");
                    InitiateScheduledRestart(secondsUntilRestart, restart.Description);
                }
                else
                {
                    _logger?.LogDebug($"Restart '{restart.Description}' - Already triggered, skipping");
                }
            }
            // Only log upcoming restarts once when they enter 30-minute window
            else if (timeUntilRestart.TotalMinutes <= 30 && timeUntilRestart.TotalMinutes > triggerWindowMinutes)
            {
                // Only log if this is a new notification (not already logged in previous minute)
                if (_nextScheduledRestart == null || _nextScheduledRestart.Value.Date != scheduledTime.Date || 
                    _nextScheduledRestart.Value.Hour != scheduledTime.Hour || 
                    _nextScheduledRestart.Value.Minute != scheduledTime.Minute)
                {
                    _logger?.LogDebug($"Restart '{restart.Description}' upcoming in {timeUntilRestart.TotalMinutes:F0} minutes");
                    Console.WriteLine($"[SmartRestart] Upcoming: {restart.Description} at {scheduledTime:HH:mm} (in {timeUntilRestart.TotalMinutes:F0} minutes)");
                }
            }
            else
            {
                _logger?.LogDebug($"Restart '{restart.Description}' - Not yet in trigger window (in {timeUntilRestart.TotalMinutes:F2} min)");
            }
        }
    }

    private void InitiateScheduledRestart(int secondsUntilRestart, string description)
    {
        var playerCount = CountHumanPlayers();

        _logger?.LogRestart(description, $"Players: {playerCount} | Delay: {secondsUntilRestart}s ({secondsUntilRestart / 60.0:F1}m)");
        Console.WriteLine($"[SmartRestart] Scheduled restart initiating: {description} in {secondsUntilRestart} seconds ({secondsUntilRestart / 60.0:F1} minutes).");

        if (playerCount == 0)
        {
            // Server empty, restart at scheduled time but save info for late joiners
            _logger?.Log($"Server is empty - restart will occur at scheduled time");
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
                _logger?.Log($"Empty server restart timer triggered - executing restart");
                _pendingRestartTimer = null;
                _pendingRestartReason = "";
                PerformRestart(sendDiscordMessage: false, reason: description, initiator: "Scheduled Restart");
            });
        }
        else
        {
            // Players online, send warnings and schedule restart
            _logger?.Log($"{playerCount} player(s) online - sending warnings before restart");
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
        _logger?.Log($"ScheduleRestartWithWarnings: totalSeconds={totalSeconds}, reason='{reason}', initiator='{initiator}'");
        _logger?.LogDebug($"ScheduleRestartWithWarnings called: totalSeconds={totalSeconds}, reason={reason}");

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

        _logger?.LogDebug($"Warnings enabled. ShowCenterAlert={_config.WarningMessages.ShowCenterAlert}");
        _logger?.LogDebug($"Configured warning times: {string.Join(", ", _config.WarningMessages.WarningTimes)}");

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

                    _logger?.LogDebug($"Sending chat warning: {timeFormatted} left");
                    Server.PrintToChatAll(fullMessage);
                });
                _warningTimers.Add(timer);
                scheduledChatWarnings++;
            }
        }
        _logger?.LogDebug($"Scheduled {scheduledChatWarnings} chat warnings");

        // Schedule continuous center alerts for last 30 seconds
        if (_config.WarningMessages.ShowCenterAlert)
        {
            // Start the countdown when there are 30 seconds left
            if (totalSeconds >= 30)
            {
                float delayUntilCountdown = totalSeconds - 30;

                var countdownTimer = AddTimer(delayUntilCountdown, () =>
                {
                    _logger?.LogDebug("Starting 30-second center alert countdown");
                    StartContinuousCenterCountdown(30);
                });
                _warningTimers.Add(countdownTimer);
                _logger?.LogDebug($"Center countdown will start in {delayUntilCountdown} seconds");
            }
            else
            {
                // Less than 30 seconds total, start countdown immediately
                _logger?.LogDebug($"Starting immediate center alert countdown for {totalSeconds} seconds");
                StartContinuousCenterCountdown(totalSeconds);
            }
        }
        else
        {
            Console.WriteLine("[SmartRestart] Center alerts are DISABLED in config!");
        }

        // Final restart with proper reason
        AddTimer(totalSeconds, () => PerformRestart(sendDiscordMessage: true, reason: reason, initiator: initiator));
        _logger?.LogDebug($"Final restart scheduled in {totalSeconds} seconds ({totalSeconds / 60.0:F1} minutes)");
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
        _logger?.LogRestart("RESTART EXECUTING", $"Reason: {reason} | Initiator: {initiator} | Uptime: {DateTime.Now - _serverStartTime:hh\\:mm\\:ss}");
        Console.WriteLine($"[SmartRestart] ========== PERFORMING RESTART ==========");
        Console.WriteLine($"[SmartRestart] Reason: {reason}");
        Console.WriteLine($"[SmartRestart] Initiator: {initiator}");

        // Send Discord notification first (fire and forget, but give it a moment)
        if (sendDiscordMessage && _config.DiscordWebhook.Enabled && _config.DiscordWebhook.RestartEmbed.Enabled)
        {
            try
            {
                _logger?.Log("Sending Discord restart notification");
                Console.WriteLine($"[SmartRestart] Sending Discord restart notification...");
                _ = _discordWebhook?.SendRestartEmbed(reason, initiator);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Discord notification failed: {ex.Message}");
                Console.WriteLine($"[SmartRestart] Discord notification failed (non-fatal): {ex.Message}");
            }
        }

        // Send chat message to notify players
        try
        {
            var prefix = LanguageConfig.ProcessColors(_config.ChatPrefix);
            var message = LanguageConfig.ProcessColors(_lang.RestartingNow);
            Server.PrintToChatAll($" {prefix} {message}");
            _logger?.Log("Chat notification sent to all players");
            Console.WriteLine($"[SmartRestart] Chat notification sent to all players");
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"Chat notification failed: {ex.Message}");
            Console.WriteLine($"[SmartRestart] Chat notification failed (non-fatal): {ex.Message}");
        }

        // Clean up active timers and warnings to prevent interference
        try
        {
            _logger?.Log("Cleaning up active timers");
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
            _logger?.LogError($"Timer cleanup failed: {ex.Message}");
            Console.WriteLine($"[SmartRestart] Timer cleanup failed (non-fatal): {ex.Message}");
        }

        _logger?.Log($"Executing restart command: {_config.RestartCommand}");
        Console.WriteLine($"[SmartRestart] Executing restart command: {_config.RestartCommand}");

        // Execute restart command with minimal delay
        // Note: Different hosting providers require different commands:
        // - Pterodactyl/Pelican: "quit" (server stops, hosting panel restarts it)
        // - Some hosted servers: "exit" or "_restart"
        // If server doesn't restart, check your hosting provider's documentation
        AddTimer(1.0f, () =>
        {
            try
            {
                _logger?.LogRestart("RESTART COMMAND EXECUTED", $"Command: {_config.RestartCommand} | Server uptime: {DateTime.Now - _serverStartTime:hh\\:mm\\:ss}");
                Console.WriteLine($"[SmartRestart] ========== EXECUTING RESTART COMMAND ==========");
                Console.WriteLine($"[SmartRestart] Command: {_config.RestartCommand}");
                Console.WriteLine($"[SmartRestart] Server uptime: {DateTime.Now - _serverStartTime:hh\\:mm\\:ss}");
                Console.WriteLine($"[SmartRestart] Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                Server.ExecuteCommand(_config.RestartCommand);

                _logger?.Log("Restart command executed successfully - server should restart");
                Console.WriteLine($"[SmartRestart] Restart command executed successfully");
                Console.WriteLine($"[SmartRestart] If server doesn't restart, check that RestartCommand in config.json is correct for your hosting provider");
                SendCommandExecutionDiscordStatus("Restart command", _config.RestartCommand, true, null, reason, initiator);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"ERROR executing restart command: {ex.Message} | Stack: {ex.StackTrace}");
                Console.WriteLine($"[SmartRestart] ========== ERROR EXECUTING RESTART ==========");
                Console.WriteLine($"[SmartRestart] Command: {_config.RestartCommand}");
                Console.WriteLine($"[SmartRestart] Error: {ex.Message}");
                Console.WriteLine($"[SmartRestart] Stack trace: {ex.StackTrace}");
                SendCommandExecutionDiscordStatus("Restart command", _config.RestartCommand, false, ex.Message, reason, initiator);
            }
        });
    }

    private void SendCommandExecutionDiscordStatus(string action, string command, bool succeeded, string? errorMessage = null, string? reason = null, string? initiator = null)
    {
        if (!_config.DiscordWebhook.Enabled || string.IsNullOrEmpty(_config.DiscordWebhook.WebhookUrl))
            return;

        var title = succeeded
            ? $"{action} executed"
            : $"{action} failed";

        var description = new StringBuilder();
        description.AppendLine($"Command: `{command}`");
        description.AppendLine($"Status: {(succeeded ? "Success" : "Failed")}");

        if (!string.IsNullOrWhiteSpace(reason))
            description.AppendLine($"Reason: {reason}");

        if (!string.IsNullOrWhiteSpace(initiator))
            description.AppendLine($"Initiated by: {initiator}");

        if (!succeeded && !string.IsNullOrWhiteSpace(errorMessage))
            description.AppendLine($"Error: {errorMessage}");

        _ = _discordWebhook?.SendDiscordWebhook(
            title,
            description.ToString(),
            succeeded ? 3066993 : 15158332
        );
    }

    private void StartContinuousCenterCountdown(float totalSeconds)
    {
        _countdownStartTime = DateTime.Now;
        _countdownDuration = (int)Math.Ceiling(totalSeconds);

        _logger?.LogDebug($"Starting center countdown for {_countdownDuration} seconds");

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
                    _logger?.LogDebug($"Countdown: {secondsRemaining}s remaining");
                }
            }
            else
            {
                _countdownStartTime = null;
                _logger?.LogDebug("Countdown completed");
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

        var players = Utilities.GetPlayers();

        foreach (var player in players)
        {
            if (player?.IsValid == true && !player.IsBot)
            {
                // PrintToCenterAlert is designed for alerts and might be more stable
                player.PrintToCenterAlert(message);
            }
        }
    }

    private void ShowCenterAlert(string message, float durationSeconds)
    {
        var players = Utilities.GetPlayers();

        // Format message with HTML for larger red text
        var htmlMessage = $"<font class='fontSize-l' color='red'>{message}</font>";

        foreach (var player in players)
        {
            if (player?.IsValid == true && !player.IsBot && player.PawnIsAlive)
            {
                // PrintToCenterHtml duration parameter is in seconds, but as int
                // Round up to ensure at least 2 seconds visibility
                player.PrintToCenterHtml(htmlMessage, Math.Max(2, (int)Math.Ceiling(durationSeconds)));
            }
        }

        _logger?.LogDebug($"Center alert shown: {message} (duration: {durationSeconds}s)");
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

        var playerCount = CountHumanPlayers();
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
        _logger.DebugEnabled = _config.Logging.DebugEnabled;
        LoadLanguage();

        // Reset state that depends on schedule config so changes are applied immediately.
        _nextScheduledRestart = null;
        _emptyMapActionExecuted = false;

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

    [ConsoleCommand("css_cancelrestart", "Cancel the scheduled restart")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnCancelRestartCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
        {
            command.ReplyToCommand("[SmartRestart] This command can only be used by players.");
            return;
        }

        // Check if database is enabled for permission check
        if (_config.Database.Enabled && _databaseManager != null)
        {
            var steamId = player.SteamID.ToString();
            var playerName = player.PlayerName;

            _ = Task.Run(async () =>
            {
                try
                {
                    _logger?.LogDebug($"Checking permission for cancel restart: {playerName} (SteamID: {steamId})");

                    bool hasPermission = false;
                    try
                    {
                        hasPermission = await _databaseManager.CheckPlayerPermissionAsync(steamId).ConfigureAwait(false);
                    }
                    catch (Exception dbEx)
                    {
                        Console.WriteLine($"[SmartRestart] Database exception during permission check: {dbEx.Message}");
                        hasPermission = false;
                    }

                    Server.NextFrame(() =>
                    {
                        if (player == null || !player.IsValid || !player.PlayerPawn.IsValid)
                        {
                            return;
                        }

                        if (!hasPermission)
                        {
                            player.PrintToChat($" {LanguageConfig.ProcessColors(_config.ChatPrefix)} {LanguageConfig.ProcessColors("{red}You don't have permission to cancel restart!{default}")}");
                            Console.WriteLine($"[SmartRestart] {playerName} attempted to cancel restart without permission.");
                            return;
                        }

                        CancelPendingRestart(player.PlayerName);
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SmartRestart] Error in cancel restart command: {ex.Message}");
                }
            });
        }
        else
        {
            // No database, allow cancel (for debugging/testing)
            CancelPendingRestart(player.PlayerName);
        }
    }

    private void CancelPendingRestart(string playerName)
    {
        bool cancelled = false;

        // Cancel scheduled restart timer
        if (_pendingRestartTimer != null)
        {
            _pendingRestartTimer.Kill();
            _pendingRestartTimer = null;
            _pendingRestartReason = "";
            cancelled = true;
            Console.WriteLine($"[SmartRestart] Scheduled restart cancelled by {playerName}");
            _logger?.Log($"Scheduled restart cancelled by {playerName}");
        }

        // Cancel warning timers
        foreach (var timer in _warningTimers)
        {
            timer?.Kill();
        }
        _warningTimers.Clear();

        // Cancel empty server timer
        if (_emptyServerTimer != null)
        {
            _emptyServerTimer.Kill();
            _emptyServerTimer = null;
            cancelled = true;
            Console.WriteLine($"[SmartRestart] Empty server restart cancelled by {playerName}");
            _logger?.Log($"Empty server restart cancelled by {playerName}");
        }

        if (cancelled)
        {
            var prefix = LanguageConfig.ProcessColors(_config.ChatPrefix);
            Server.PrintToChatAll($" {prefix} {LanguageConfig.ProcessColors("{green}Server restart has been cancelled!{default}")}");

            if (_config.DiscordWebhook.Enabled)
            {
                _ = _discordWebhook?.SendDiscordWebhook("🛑 Server Restart Cancelled", 
                    $"Cancelled by: {playerName}", 
                    15158332); // Red/Orange color
            }
        }
        else
        {
            Server.PrintToChatAll($" {LanguageConfig.ProcessColors(_config.ChatPrefix)} {LanguageConfig.ProcessColors("{yellow}No restart is currently scheduled.{default}")}");
        }
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
                _logger?.LogDebug($"Checking permission for {playerName} (SteamID: {steamId})");

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

                _logger?.LogDebug($"Permission check result: {hasPermission}");

                // Execute on next tick to avoid threading issues
                Server.NextFrame(() =>
                {
                    // Re-validate player is still connected
                    if (player == null || !player.IsValid || !player.PlayerPawn.IsValid)
                    {
                        _logger?.LogDebug("Player disconnected before permission check completed.");
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
                    var playerCount = CountHumanPlayers();

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
