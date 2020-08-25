using Discord;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ScrimBot
{
    public class Program
    {
        // Permissions integer: 268504128
        // - Manage Roles
        // - Send Messages
        // - Read Message History
        // - Add Reactions
        // - View Channels
        // OAuth2 URL: https://discord.com/api/oauth2/authorize?client_id=747683069737041970&permissions=268504128&scope=bot

        public static void Main()
        {
#if DEBUG
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();
#else
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(GetAppdataFilePath("log.log"))
                .CreateLogger();
#endif

            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {

        }

        private Task LogMessage(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Debug:
                    Log.Debug(message.Exception, message.Message);
                    break;
                case LogSeverity.Verbose:
                    Log.Verbose(message.Exception, message.Message);
                    break;
                case LogSeverity.Info:
                    Log.Information(message.Exception, message.Message);
                    break;
                case LogSeverity.Warning:
                    Log.Warning(message.Exception, message.Message);
                    break;
                case LogSeverity.Error:
                    Log.Error(message.Exception, message.Message);
                    break;
                case LogSeverity.Critical:
                    Log.Fatal(message.Exception, message.Message);
                    break;
            }

            return Task.CompletedTask;
        }

        public static string GetAppdataFilePath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            path = Path.Combine(path, "ScrimBot");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }
}
