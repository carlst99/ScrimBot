using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Nito.AsyncEx;
using ScrimBot.Commands;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ScrimBot
{
    public static class Program
    {
        // Permissions integer: 268504128
        // - Manage Roles
        // - Send Messages
        // - Read Message History
        // - Add Reactions
        // - View Channels
        // OAuth2 URL: https://discord.com/api/oauth2/authorize?client_id=<YOUR_CLIENT_ID>&permissions=268504128&scope=bot

        /// <summary>
        /// The name of the environment variable storing our bot token
        /// </summary>
        private const string TOKEN_ENV_NAME = "SCRIMBOT_TOKEN";

        /// <summary>
        /// Gets the command prefix used by ScrimBot
        /// </summary>
        public const string COMMAND_PREFIX = "sb!";

        public const string NAME = "ScrimBot";

        private static DiscordSocketClient _client;

        public static int Main()
        {
            // Setup Serilog
#if DEBUG
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();
#else
            string logFile = Path.Combine(GetAppdataDirectory(), "NAME", "log.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(logFile)
                .CreateLogger();
            Log.Information("Logfile store at {logFile}", logFile);
#endif

            try
            {
                Log.Information("Starting");
                AsyncContext.Run(MainAsync);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to start");
                return 1;
            }

            Log.Information("Stopped");
            return 0;
        }

        public static async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.Log += LogMessage;

            // Get our token and start the client
            string token = Environment.GetEnvironmentVariable(TOKEN_ENV_NAME, EnvironmentVariableTarget.Process);
            await _client.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);

            // Setup commands
            CommandHandler handler = new CommandHandler(_client, new CommandService());
            await handler.InstallCommandsAsync().ConfigureAwait(false);

            // Show our prefix in the status
            await _client.SetGameAsync(COMMAND_PREFIX + "help", type: ActivityType.Listening).ConfigureAwait(false);
            await Task.Delay(-1).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves the AppData folder for this app
        /// </summary>
        public static string GetAppdataDirectory()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            path = Path.Combine(path, "ScrimBot");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// Routes the logging functionality of Discord.Net to Serilog
        /// </summary>
        /// <param name="message">The log message</param>
        private static Task LogMessage(LogMessage message)
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
    }
}
