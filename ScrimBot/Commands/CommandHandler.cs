using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ScrimBot.Services;
using System.Reflection;
using System.Threading.Tasks;

namespace ScrimBot.Commands
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public CommandHandler(DiscordSocketClient client, CommandService commands)
        {
            _commands = commands;
            _client = client;
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: null).ConfigureAwait(false);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            if (!(messageParam is SocketUserMessage message))
                return;

            // Make sure we aren't triggered by bots
            if (message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Check if we have received a DM
            if (messageParam.Channel is IDMChannel)
            {
                await AccountDistributionService.HandleRequest(context).ConfigureAwait(false);
                return;
            }

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            int argPos = 0;
            if (!(message.HasStringPrefix(Program.COMMAND_PREFIX, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
            {
                return;
            }

            // Keep in mind that result does not indicate a return value
            // rather an object stating if the command executed successfully.
            IResult result = await _commands.ExecuteAsync(context, argPos, null).ConfigureAwait(false);

            // Optionally, we may inform the user if the command fails
            // to be executed; however, this may not always be desired,
            // as it may clog up the request queue should a user spam a
            // command.
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason).ConfigureAwait(false);
        }
    }
}
