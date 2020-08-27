using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ScrimBot.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScrimBot.Services
{
    public static class AccDistService
    {
        private static readonly List<DistributionRequest> _distributionRequests = new List<DistributionRequest>();

        public static async Task<bool> AddRequest(DistributionRequest request)
        {
            if (CheckContains(request.User))
            {
                return false;
            } else
            {
                _distributionRequests.Add(request);

                IDMChannel channel = await request.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                await channel.SendMessageAsync("Copy the accounts over from your spreadsheet program, " +
                    "and send them in your next message. The format should be: " +
                    "\r\nuser1    pass1\r\nuser2    pass2\r\n...\r\nWhere there are four spaces between the username and password").ConfigureAwait(false);

                await channel.CloseAsync().ConfigureAwait(false);

                return true;
            }
        }

        public static bool RemoveRequest(SocketUser requestMaker)
        {
            return _distributionRequests.Remove(GetByUser(requestMaker));
        }

        public static async Task HandleRequest(SocketCommandContext context)
        {
            if (!CheckContains(context.User))
            {
                await context.Channel.SendMessageAsync("I wasn't expecting a message from you! Has your request timed out?").ConfigureAwait(false);
                return;
            }

            string content = context.Message.Resolve();
            string[] accountPairs = content.Split('\r', '\n');

            await context.Channel.SendMessageAsync("Confirmed!").ConfigureAwait(false);
        }

        private static bool CheckContains(SocketUser user)
            => _distributionRequests.Any(u => u.User.Id.Equals(user.Id));

        private static DistributionRequest GetByUser(SocketUser user)
            => _distributionRequests.First(u => u.User.Id.Equals(user.Id));
    }
}
