using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ScrimBot.Commands;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ScrimBot
{
    public class Program
    {
        /// <summary>
        /// Gets the command prefix used by ScrimBot
        /// </summary>
        public const string COMMAND_PREFIX = "sb!";

        // Permissions integer: 268504128
        // - Manage Roles
        // - Send Messages
        // - Read Message History
        // - Add Reactions
        // - View Channels
        // OAuth2 URL: https://discord.com/api/oauth2/authorize?client_id=747683069737041970&permissions=268504128&scope=bot

        public static void Main()
        {
            // Setup Serilog
#if DEBUG
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();
#else
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(GetAppdataFolder("log.log"))
                .CreateLogger();
#endif

            new Program().MainAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Starts the bot
        /// </summary>
        public async Task MainAsync()
        {
            DiscordSocketClient client = new DiscordSocketClient();
            client.Log += LogMessage;

            // Get our token and start the client
            string token = Environment.GetEnvironmentVariable("token", EnvironmentVariableTarget.Process);
            await client.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
            await client.StartAsync().ConfigureAwait(false);

            // Setup commands
            CommandHandler handler = new CommandHandler(client, new CommandService());
            await handler.InstallCommandsAsync().ConfigureAwait(false);

            await client.SetGameAsync(COMMAND_PREFIX, type: ActivityType.Listening).ConfigureAwait(false);

            // Await console input to stop the program
            Console.ReadKey();
            await client.StopAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Routes the logging functionality of Discord.Net to Serilog
        /// </summary>
        /// <param name="message">The log message</param>
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

        /// <summary>
        /// Retrieves the AppData folder for this app
        /// </summary>
        public static string GetAppdataFolder()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            path = Path.Combine(path, "ScrimBot");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }
}
