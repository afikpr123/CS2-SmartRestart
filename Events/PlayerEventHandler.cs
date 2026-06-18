using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace SmartRestart.Events;

public class PlayerEventHandler
{
    private readonly SmartRestartPlugin _plugin;
    private readonly SmartRestartConfig _config;
    private readonly LanguageConfig _lang;
    private readonly Action<float, Action> _addTimer;
    private readonly Func<float, Action, TimerFlags, Timer> _addTimerWithFlags;

    public PlayerEventHandler(
        SmartRestartPlugin plugin,
        SmartRestartConfig config,
        LanguageConfig lang,
        Action<float, Action> addTimer,
        Func<float, Action, TimerFlags, Timer> addTimerWithFlags)
    {
        _plugin = plugin;
        _config = config;
        _lang = lang;
        _addTimer = addTimer;
        _addTimerWithFlags = addTimerWithFlags;
    }

    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;

        // Track session completion
        if (player != null && player.IsValid && !player.IsBot && _plugin.PlayerJoinTimes.ContainsKey(player.SteamID))
        {
            var sessionLength = DateTime.Now - _plugin.PlayerJoinTimes[player.SteamID];
            if (sessionLength.TotalMinutes >= _config.SmartEmptyRestart.SessionBased.MinimumSessionLengthMinutes)
            {
                _plugin.CompletedSessions.Add(sessionLength);
                Console.WriteLine($"[SmartRestart] Session tracked: {sessionLength.TotalMinutes:F1} minutes (Total sessions: {_plugin.CompletedSessions.Count})");
            }
            _plugin.PlayerJoinTimes.Remove(player.SteamID);
        }

        if (!_config.EnableAutoRestart) return HookResult.Continue;

        // Check if server is now empty
        _addTimer(1.0f, () =>
        {
            var playerCount = Utilities.GetPlayers().Count(p => p?.IsValid == true && !p.IsBot);

            if (playerCount == 0)
            {
                _plugin.LastPlayerLeaveTime = DateTime.Now;

                // Smart checks before scheduling restart
                if (!_plugin.ShouldScheduleEmptyRestart())
                {
                    return;
                }

                // Calculate smart delay based on time-of-day and configuration
                var delaySeconds = _plugin.CalculateSmartDelay();
                var delayMinutes = delaySeconds / 60.0;

                Console.WriteLine($"[SmartRestart] Server is now empty. Restart scheduled in {delayMinutes:F1} minutes.");

                // Cancel any existing timer
                _plugin.EmptyServerTimer?.Kill();

                // Schedule restart after smart delay
                _plugin.EmptyServerTimer = _addTimerWithFlags(delaySeconds, () =>
                {
                    // Final checks before restart
                    var uptime = DateTime.Now - _plugin.ServerStartTime;
                    if (uptime.TotalHours < _config.MinimumUptimeForEmptyRestartHours)
                    {
                        var requiredHours = _config.MinimumUptimeForEmptyRestartHours;
                        Console.WriteLine($"[SmartRestart] Auto-restart cancelled: Server uptime is only {uptime.TotalHours:F1} hours (minimum {requiredHours} hours required for empty-server restart).");
                        _plugin.EmptyServerTimer = null;
                        return;
                    }

                    if (_plugin.CanRestart())
                    {
                        var totalPlaytime = _plugin.CompletedSessions.Sum(s => s.TotalMinutes);
                        Console.WriteLine($"[SmartRestart] Executing automatic restart (empty, uptime: {_plugin.FormatUptime(uptime)}, peak: {_plugin.PeakPlayerCount}, sessions: {_plugin.CompletedSessions.Count}, playtime: {totalPlaytime:F1}m).");

                        if (_config.DiscordWebhook.SendOnEmptyServerRestart)
                        {
                            _ = _plugin.DiscordWebhook?.SendRestartEmbed("Server is empty", "Auto-Restart");
                        }

                        _plugin.LastEmptyRestart = DateTime.Now;
                        _plugin.PeakPlayerCount = 0;
                        _plugin.CompletedSessions.Clear();
                        _plugin.PlayerJoinTimes.Clear();
                        _plugin.LastSignificantActivity = null;
                        _plugin.PerformRestart(sendDiscordMessage: false);
                    }
                }, 0);
            }
        });

        return HookResult.Continue;
    }

    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;

        // Cancel empty server restart if a player joins
        if (_plugin.EmptyServerTimer is not null)
        {
            _plugin.EmptyServerTimer.Kill();
            _plugin.EmptyServerTimer = null;
            _plugin.LastPlayerLeaveTime = null;
            Console.WriteLine("[SmartRestart] Auto-restart cancelled: Player joined server.");
        }

        // Check if there's a pending scheduled restart
        Console.WriteLine($"[SmartRestart] OnPlayerConnect DEBUG: _pendingRestartTimer={(_plugin.PendingRestartTimer != null ? "SET" : "NULL")}, _nextScheduledRestart={(_plugin.NextScheduledRestart != null ? _plugin.NextScheduledRestart.Value.ToString("HH:mm:ss") : "NULL")}");

        if (_plugin.PendingRestartTimer != null && _plugin.NextScheduledRestart != null)
        {
            var timeUntilRestart = (_plugin.NextScheduledRestart.Value - DateTime.Now).TotalSeconds;
            Console.WriteLine($"[SmartRestart] DEBUG: timeUntilRestart={timeUntilRestart:F0} seconds");

            if (timeUntilRestart > 0)
            {
                Console.WriteLine($"[SmartRestart] ⚠️ Player joined during pending restart! Converting to warned restart.");
                Console.WriteLine($"[SmartRestart] Time until restart: {timeUntilRestart:F0} seconds ({timeUntilRestart / 60.0:F1} minutes)");

                // Cancel the silent restart timer
                _plugin.PendingRestartTimer.Kill();
                _plugin.PendingRestartTimer = null;

                // Start warnings and countdown for players
                var playerCount = Utilities.GetPlayers().Count(p => p?.IsValid == true && !p.IsBot);
                Console.WriteLine($"[SmartRestart] {playerCount} player(s) online, starting warning system.");

                if (_config.DiscordWebhook.SendOnScheduledRestart && timeUntilRestart > 30)
                {
                    var timeFormatted = LanguageConfig.FormatTime((int)timeUntilRestart);
                    _ = _plugin.DiscordWebhook?.SendDiscordWebhook($"⏰ Scheduled Restart: {_plugin.PendingRestartReason}", 
                        $"Restart in: {timeFormatted}\nPlayers online: {playerCount}", 
                        1838576);
                }

                _plugin.ScheduleRestartWithWarnings(totalSeconds: (int)timeUntilRestart, reason: _plugin.PendingRestartReason, initiator: "Scheduled Restart");
                _plugin.PendingRestartReason = "";
            }
            else
            {
                Console.WriteLine($"[SmartRestart] WARNING: timeUntilRestart is {timeUntilRestart:F0}, not starting warnings");
            }
        }

        // If countdown is already active, notify the joining player
        if (_plugin.CountdownStartTime != null && player != null && player.IsValid && !player.IsBot)
        {
            var elapsed = (DateTime.Now - _plugin.CountdownStartTime.Value).TotalSeconds;
            var secondsRemaining = _plugin.CountdownDuration - (int)elapsed;

            if (secondsRemaining > 0)
            {
                Console.WriteLine($"[SmartRestart] Player {player.PlayerName} joined during countdown ({secondsRemaining}s remaining)");

                var timeFormatted = LanguageConfig.FormatTime(secondsRemaining);
                var prefix = LanguageConfig.ProcessColors(_config.ChatPrefix);
                var message = _lang.RestartWarning.Replace("{time}", timeFormatted);
                var fullMessage = $" {prefix} {LanguageConfig.ProcessColors(message)}";

                _addTimer(1.0f, () => 
                {
                    if (player?.IsValid == true)
                    {
                        player.PrintToChat(fullMessage);
                        Console.WriteLine($"[SmartRestart] Sent restart warning to newly joined player: {secondsRemaining}s remaining");
                    }
                });
            }
        }

        // Track player join time for session tracking
        if (player != null && player.IsValid && !player.IsBot)
        {
            _plugin.PlayerJoinTimes[player.SteamID] = DateTime.Now;
            _plugin.LastSignificantActivity = DateTime.Now;
        }

        // Track peak player count
        var currentCount = Utilities.GetPlayers().Count(p => p?.IsValid == true && !p.IsBot);
        if (currentCount > _plugin.PeakPlayerCount)
        {
            _plugin.PeakPlayerCount = currentCount;
        }

        return HookResult.Continue;
    }
}
