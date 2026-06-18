using System.Text.Json.Serialization;

namespace SmartRestart;

public class LanguageConfig
{
    [JsonPropertyName("RestartWarning")]
    public string RestartWarning { get; set; } = "Server will restart in {yellow}{time}{default}!";

    [JsonPropertyName("RestartWarningCenter")]
    public string RestartWarningCenter { get; set; } = "⚠️ SERVER RESTART IN {time} ⚠️";

    [JsonPropertyName("RestartingNow")]
    public string RestartingNow { get; set; } = "Server is restarting now!";

    public static string ProcessColors(string message)
    {
        return message
            .Replace("{default}", "\x01")
            .Replace("{red}", "\x02")
            .Replace("{lightpurple}", "\x03")
            .Replace("{green}", "\x04")
            .Replace("{lime}", "\x05")
            .Replace("{lightgreen}", "\x06")
            .Replace("{lightred}", "\x07")
            .Replace("{gray}", "\x08")
            .Replace("{yellow}", "\x09")
            .Replace("{gold}", "\x10")      // Gold/Orange color
            .Replace("{silver}", "\x0A")
            .Replace("{blue}", "\x0B")
            .Replace("{darkblue}", "\x0C")
            .Replace("{purple}", "\x0E")
            .Replace("{lightred2}", "\x0F")
            .Replace("{orange}", "\x10");
    }

    public static string FormatTime(int seconds)
    {
        if (seconds >= 3600) // 1 hour or more
        {
            int hours = seconds / 3600;
            int remainingMinutes = (seconds % 3600) / 60;

            if (remainingMinutes > 0)
                return $"{hours} hour{(hours != 1 ? "s" : "")} {remainingMinutes} minute{(remainingMinutes != 1 ? "s" : "")}";
            return $"{hours} hour{(hours != 1 ? "s" : "")}";
        }
        else if (seconds >= 60) // 1 minute or more
        {
            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;

            if (remainingSeconds > 0)
                return $"{minutes} minute{(minutes != 1 ? "s" : "")} {remainingSeconds} second{(remainingSeconds != 1 ? "s" : "")}";
            return $"{minutes} minute{(minutes != 1 ? "s" : "")}";
        }
        else // Less than a minute
        {
            return $"{seconds} second{(seconds != 1 ? "s" : "")}";
        }
    }
}
