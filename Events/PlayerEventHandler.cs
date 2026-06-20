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

        if (player != null && player.IsValid && !player.IsBot)
        {
            _plugin.PlayerJoinTimes.Remove(player.SteamID);
        }

        if (!_config.EnableAutoRestart) return HookResult.Continue;

        // Check if server is now empty after the disconnect completes.
        _addTimer(1.0f, () =>
        {
            var playerCount = _plugin.CountHumanPlayers();

            if (playerCount == 0)
            {
                _plugin.LastPlayerLeaveTime = DateTime.Now;

                if (!_plugin.ShouldScheduleEmptyMapAction())
                {
                    return;
                }

                var delaySeconds = _config.EmptyServerBehavior.DelaySeconds;
                var delayMinutes = delaySeconds / 60.0;

                Console.WriteLine($"[SmartRestart] Server is now empty. Map refresh scheduled in {delayMinutes:F1} minutes.");

                _plugin.EmptyServerTimer?.Kill();

                _plugin.EmptyServerTimer = _addTimerWithFlags(delaySeconds, () =>
                {
                    var finalPlayerCount = _plugin.CountHumanPlayers();
                    if (finalPlayerCount == 0)
                    {
                        Console.WriteLine("[SmartRestart] Executing empty-server map refresh.");
                        _plugin.LastEmptyRestart = DateTime.Now;
                        _plugin.PeakPlayerCount = 0;
                        _plugin.PlayerJoinTimes.Clear();
                        _plugin.EmptyMapActionExecuted = true;
                        _plugin.ChangeToCurrentMap();
                    }

                    _plugin.EmptyServerTimer = null;

                    if (_config.EmptyServerBehavior.RepeatWhileStillEmpty)
                    {
                        var repeatSeconds = Math.Max(60, _config.EmptyServerBehavior.RepeatIntervalSeconds);
                        _plugin.EmptyServerTimer = _addTimerWithFlags(repeatSeconds, () =>
                        {
                            var repeatPlayerCount = _plugin.CountHumanPlayers();
                            if (repeatPlayerCount == 0)
                            {
                                if (_plugin.ShouldScheduleEmptyMapAction())
                                {
                                    Console.WriteLine("[SmartRestart] Repeat empty map refresh triggered.");
                                    _plugin.ChangeToCurrentMap();
                                }
                            }
                            else
                            {
                                _plugin.EmptyServerTimer = null;
                            }
                        }, TimerFlags.REPEAT);
                    }
                }, 0);
            }
        });

        return HookResult.Continue;
    }

    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (_plugin.EmptyServerTimer is not null)
        {
            _plugin.EmptyServerTimer.Kill();
            _plugin.EmptyServerTimer = null;
            _plugin.LastPlayerLeaveTime = null;
            Console.WriteLine("[SmartRestart] Auto-restart cancelled: Player joined server.");
        }

        _plugin.EmptyMapActionExecuted = false;

        _plugin.DebugLog($"OnPlayerConnect: _pendingRestartTimer={(_plugin.PendingRestartTimer != null ? "SET" : "NULL")}, _nextScheduledRestart={(_plugin.NextScheduledRestart != null ? _plugin.NextScheduledRestart.Value.ToString("HH:mm:ss") : "NULL")}");

        if (_plugin.PendingRestartTimer != null && _plugin.NextScheduledRestart != null)
        {
            var timeUntilRestart = (_plugin.NextScheduledRestart.Value - DateTime.Now).TotalSeconds;
            _plugin.DebugLog($"timeUntilRestart={timeUntilRestart:F0} seconds");

            if (timeUntilRestart > 0)
            {
                _plugin.DebugLog("Player joined during pending restart. Converting to warned restart.");
                _plugin.DebugLog($"Time until restart: {timeUntilRestart:F0} seconds ({timeUntilRestart / 60.0:F1} minutes)");

                _plugin.PendingRestartTimer.Kill();
                _plugin.PendingRestartTimer = null;

                var playerCount = _plugin.CountHumanPlayers();
                _plugin.DebugLog($"{playerCount} player(s) online, starting warning system.");

                if (_config.DiscordWebhook.SendOnScheduledRestart && timeUntilRestart > 30)
                {
                    var timeFormatted = LanguageConfig.FormatTime((int)timeUntilRestart);
                    _ = _plugin.DiscordWebhook?.SendDiscordWebhook(
                        $"Scheduled Restart: {_plugin.PendingRestartReason}",
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

        if (_plugin.CountdownStartTime != null && player != null && player.IsValid && !player.IsBot)
        {
            var elapsed = (DateTime.Now - _plugin.CountdownStartTime.Value).TotalSeconds;
            var secondsRemaining = _plugin.CountdownDuration - (int)elapsed;

            if (secondsRemaining > 0)
            {
                _plugin.DebugLog($"Player {player.PlayerName} joined during countdown ({secondsRemaining}s remaining)");

                var timeFormatted = LanguageConfig.FormatTime(secondsRemaining);
                var prefix = LanguageConfig.ProcessColors(_config.ChatPrefix);
                var message = _lang.RestartWarning.Replace("{time}", timeFormatted);
                var fullMessage = $" {prefix} {LanguageConfig.ProcessColors(message)}";

                _addTimer(1.0f, () =>
                {
                    if (player?.IsValid == true)
                    {
                        player.PrintToChat(fullMessage);
                        _plugin.DebugLog($"Sent restart warning to newly joined player: {secondsRemaining}s remaining");
                    }
                });
            }
        }

        if (player != null && player.IsValid && !player.IsBot)
        {
            _plugin.PlayerJoinTimes[player.SteamID] = DateTime.Now;
        }

        var currentCount = _plugin.CountHumanPlayers();
        if (currentCount > _plugin.PeakPlayerCount)
        {
            _plugin.PeakPlayerCount = currentCount;
        }

        return HookResult.Continue;
    }
}
