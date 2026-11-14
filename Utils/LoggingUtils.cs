using CounterStrikeSharp.API;

using SpamBlocker.Configs;

namespace SpamBlocker.Utils
{
    public static class LoggingUtils
    {
        public static void LogDebug(string message, BaseConfig config)
        {
            if (config.EnableDebug)
            {
                Server.PrintToConsole($"[SpamBlocker] DEBUG: {message}");
            }
        }

        public static void LogInfo(string message)
        {
            Server.PrintToConsole($"[SpamBlocker] INFO: {message}");
        }

        public static void LogError(string message, Exception? exception = null)
        {
            string errorMessage = $"[SpamBlocker] ERROR: {message}";
            if (exception != null)
            {
                errorMessage += $" - {exception.Message}";
            }

            Server.PrintToConsole(errorMessage);
        }
    }
}
