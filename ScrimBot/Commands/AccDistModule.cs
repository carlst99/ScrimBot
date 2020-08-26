using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScrimBot.Commands
{
    public class AccDistModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Dictionary<SocketUser, SocketRole> _distributionDms = new Dictionary<SocketUser, SocketRole>();

        [Command("distribute")]
        public async Task DistributeCommand(SocketRole role)
        {
            SocketUser sender = Context.User;

            if (!_distributionDms.ContainsKey(sender))
            {
                _distributionDms.Add(sender, role);

                IDMChannel channel = await sender.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                await channel.SendMessageAsync("Copy the accounts over from your spreadsheet program, and send them in your next message. The format should be: \r\nuser1    pass1\r\nuser2    pass2\r\n...\r\nWhere there are four spaces between the username and password").ConfigureAwait(false);

                await ReplyAsync("Please check your DMs!").ConfigureAwait(false);
            } else
            {
                await ReplyAsync("A distribution request has already been sent to you! Use the 'clear-distribution' command to cancel this").ConfigureAwait(false);
            }
        }

        [Command("clear-distribution")]
        public async Task ClearDistributionCommand()
        {
            bool wasRemoved = _distributionDms.Remove(Context.User);

            await ReplyAsync("Distribution Cancellation Success: " + wasRemoved).ConfigureAwait(false);
        }
    }
}
