using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace SmartRestart;

public static class MessageHelper
{
    public static void SendChatMessage(CCSPlayerController player, string prefix, string message)
    {
        var processedPrefix = LanguageConfig.ProcessColors(prefix);
        var processedMessage = LanguageConfig.ProcessColors(message);
        var fullMessage = $" {processedPrefix} {processedMessage}";
        player.PrintToChat(fullMessage);
    }

    public static void SendChatMessageToAll(string prefix, string message)
    {
        var processedPrefix = LanguageConfig.ProcessColors(prefix);
        var processedMessage = LanguageConfig.ProcessColors(message);
        var fullMessage = $" {processedPrefix} {processedMessage}";
        Server.PrintToChatAll(fullMessage);
    }

    public static void SendCenterAlert(CCSPlayerController player, string message)
    {
        player.PrintToCenterAlert(message);
    }

    public static void SendCenterHtml(CCSPlayerController player, string message, int durationSeconds)
    {
        var htmlMessage = $"<div style='font-size: 32px; font-weight: bold; text-shadow: 2px 2px 4px rgba(0,0,0,0.8); z-index: 9999;'>{message}</div>";
        player.PrintToCenterHtml(htmlMessage, Math.Max(2, durationSeconds));
    }

    public static void SendRestartWarning(
        SmartRestartConfig config,
        LanguageConfig lang,
        int secondsRemaining)
    {
        var timeFormatted = LanguageConfig.FormatTime(secondsRemaining);
        var message = lang.RestartWarning.Replace("{time}", timeFormatted);
        SendChatMessageToAll(config.ChatPrefix, message);
    }

    public static void SendRestartWarningToPlayer(
        CCSPlayerController player,
        SmartRestartConfig config,
        LanguageConfig lang,
        int secondsRemaining)
    {
        var timeFormatted = LanguageConfig.FormatTime(secondsRemaining);
        var message = lang.RestartWarning.Replace("{time}", timeFormatted);
        SendChatMessage(player, config.ChatPrefix, message);
    }

    public static void SendRestartingNow(SmartRestartConfig config, LanguageConfig lang)
    {
        SendChatMessageToAll(config.ChatPrefix, lang.RestartingNow);
    }

    public static void SendCenterCountdown(
        CCSPlayerController player,
        LanguageConfig lang,
        int secondsRemaining)
    {
        var timeFormatted = LanguageConfig.FormatTime(secondsRemaining);
        var message = lang.RestartWarningCenter.Replace("{time}", timeFormatted);
        SendCenterAlert(player, message);
    }

    public static void SendCenterCountdownToAll(
        LanguageConfig lang,
        int secondsRemaining)
    {
        var timeFormatted = LanguageConfig.FormatTime(secondsRemaining);
        var message = lang.RestartWarningCenter.Replace("{time}", timeFormatted);

        var players = Utilities.GetPlayers()
            .Where(p => p?.IsValid == true && !p.IsBot && p.PawnIsAlive);

        foreach (var player in players)
        {
            SendCenterAlert(player, message);
        }
    }

    public static void SendPermissionDenied(CCSPlayerController player, SmartRestartConfig config)
    {
        SendChatMessage(player, config.ChatPrefix, "{red}You don't have permission to use this command.{default}");
    }

    public static void SendMinimumUptimeError(CCSPlayerController player, SmartRestartConfig config, int requiredMinutes)
    {
        SendChatMessage(player, config.ChatPrefix, $"{{red}}Cannot restart: Minimum uptime of {requiredMinutes} minutes not met.{{default}}");
    }

    public static void SendRestartScheduled(CCSPlayerController player, SmartRestartConfig config, int minutes)
    {
        SendChatMessage(player, config.ChatPrefix, $"{{green}}Server restart scheduled in {minutes} minutes!{{default}}");
    }

    public static void SendRestartCancelled(CCSPlayerController player, SmartRestartConfig config)
    {
        SendChatMessage(player, config.ChatPrefix, "{green}Pending restart has been cancelled.{default}");
    }

    public static void SendNoRestartPending(CCSPlayerController player, SmartRestartConfig config)
    {
        SendChatMessage(player, config.ChatPrefix, "{yellow}No restart is currently pending.{default}");
    }

    public static void SendNextRestart(CCSPlayerController player, SmartRestartConfig config, string time, string description)
    {
        SendChatMessage(player, config.ChatPrefix, $"{{yellow}}Next scheduled restart: {time} ({description}){{default}}");
    }

    public static void SendNoScheduledRestart(CCSPlayerController player, SmartRestartConfig config)
    {
        SendChatMessage(player, config.ChatPrefix, "{yellow}No restarts are currently scheduled.{default}");
    }

    public static void SendRestartInitiatedByPlayer(SmartRestartConfig config, string playerName)
    {
        SendChatMessageToAll(config.ChatPrefix, $"{{yellow}}Server restart initiated by {playerName}{{default}}");
    }

    public static void SendEmptyServerImmediate(CCSPlayerController player, SmartRestartConfig config)
    {
        SendChatMessage(player, config.ChatPrefix, "{green}Server is empty. Restarting immediately...{default}");
    }

    public static void SendErrorMessage(CCSPlayerController player, SmartRestartConfig config)
    {
        SendChatMessage(player, config.ChatPrefix, "{red}An error occurred. Please contact an administrator.{default}");
    }
}
