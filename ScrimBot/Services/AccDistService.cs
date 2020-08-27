using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ScrimBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScrimBot.Services
{
    public static class AccDistService
    {
        private static readonly List<DistributionRequest> _distributionRequests = new List<DistributionRequest>();

        /// <summary>
        /// Adds an account distribution request to be processed
        /// </summary>
        /// <param name="request"></param>
        public static async Task<bool> AddRequest(DistributionRequest request)
        {
            if (CheckContainsRequest(request.User))
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

        /// <summary>
        /// Attempts to remove a request made by the provided user
        /// </summary>
        /// <param name="requestMaker">The user who made the request</param>
        public static bool RemoveRequest(SocketUser requestMaker)
            => _distributionRequests.Remove(GetRequestByUser(requestMaker));

        public static async Task HandleRequest(SocketCommandContext context)
        {
            // Check that we have a request from this user before continuing
            if (!CheckContainsRequest(context.User))
            {
                await context.Channel.SendMessageAsync("I wasn't expecting a message from you! Has your request timed out?").ConfigureAwait(false);
                return;
            }

            // Get the request and show our typing status
            DistributionRequest request = GetRequestByUser(context.User);
            IDisposable isTyping = context.Channel.EnterTypingState();

            // Check if this request has been
            if (request.WaitingOnConfirmation)
            {
                if (context.Message.Resolve().Equals("yes"))
                {
                    await CompleteRequest(context, request).ConfigureAwait(false);
                    isTyping.Dispose();
                }
                else
                {
                    // Reset and await a new batch of accounts
                    request.WaitingOnConfirmation = false;
                    await context.Channel.SendMessageAsync("Copy that. Send your accounts again.").ConfigureAwait(false);
                }
                return;
            }
            // Indicate that the next message we receive should be a confirmation message
            request.WaitingOnConfirmation = true;

            string content = context.Message.Resolve();
            string[] accounts = content.Split('\r', '\n');

            // Check that we have enough accounts to go around
            int memberCount = request.Role.Members.Count();
            if (accounts.Length < memberCount)
            {
                await context.Channel.SendMessageAsync($"There more members with the role '{request.Role.Name}' than accounts. Try again!").ConfigureAwait(false);
                return;
            }

            // Parse each account into an info object
            List<AccountInfo> accountInfos = new List<AccountInfo>();
            foreach (string account in accounts)
                accountInfos.Add(AccountInfo.Parse(account));

            // Assign a member to each account
            for (int i = 0; i < memberCount; i++)
                accountInfos[i].User = request.Role.Members.ElementAt(i);

            // Remove unused infos and attach the list to the request
            accountInfos.RemoveRange(memberCount, accountInfos.Count - memberCount);
            request.Accounts = accountInfos;

            // Build and send the confirmation message
            string reply = "Here's the accounts I will distribute. Reply with 'yes' to confirm:";
            foreach (AccountInfo info in accountInfos)
                reply += $"\r\n{info}";

            await context.Channel.SendMessageAsync(reply).ConfigureAwait(false);
            isTyping.Dispose();
        }

        private static async Task CompleteRequest(SocketCommandContext context, DistributionRequest request)
        {
            
            RemoveRequest(context.User);
        }

        /// <summary>
        /// Checks to see if the request store contains a request made by the provided user
        /// </summary>
        /// <param name="user">The user who has made a request</param>
        private static bool CheckContainsRequest(SocketUser user)
            => _distributionRequests.Any(u => u.User.Id.Equals(user.Id));

        /// <summary>
        /// Gets a request made by the provided user
        /// </summary>
        /// <param name="user">The user who made the request</param>
        private static DistributionRequest GetRequestByUser(SocketUser user)
            => _distributionRequests.First(u => u.User.Id.Equals(user.Id));
    }
}
