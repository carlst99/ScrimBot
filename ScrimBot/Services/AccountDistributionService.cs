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
    public static class AccountDistributionService
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

        /// <summary>
        /// Handles a message from a DM channel
        /// </summary>
        /// <param name="context">The context from which the message was sent</param>
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

            // Check if this request is waiting on a confirmation
            if (request.WaitingOnConfirmation)
            {
                if (context.Message.Resolve().Equals("yes"))
                {
                    await CompleteDistribution(context, request).ConfigureAwait(false);
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

            // Start the request process
            await StartDistribution(context, request).ConfigureAwait(false);
            isTyping.Dispose();
        }

        /// <summary>
        /// Starts the distribution by parsing the accounts and reporting pairings to the user
        /// </summary>
        /// <param name="context">The context from which the message was sent</param>
        /// <param name="request">Contains information about the original request that was made</param>
        private static async Task StartDistribution(SocketCommandContext context, DistributionRequest request)
        {
            // Resolve the message and get each individual account
            // Note that spreadsheet programs copy their message in the format
            // cellA1    cellB1\r\n
            // cellA2    cellB2
            // Where the gap is four spaces
            string content = context.Message.Resolve();
            string[] accounts = content.Split('\r', '\n');

            // Check that we have enough accounts to go around
            int memberCount = request.Role.Members.Count();
            if (accounts.Length < memberCount)
            {
                await context.Channel.SendMessageAsync($"There more members with the role '{request.Role.Name}' than available accounts. Try again!").ConfigureAwait(false);
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
        }

        /// <summary>
        /// Completes distribution by sending the account to each member of the given role, and reporting pairings to the main channel
        /// </summary>
        /// <param name="context">The context from which the message was sent</param>
        /// <param name="request">Contains information about the original request that was made</param>
        private static async Task CompleteDistribution(SocketCommandContext context, DistributionRequest request)
        {
            const string message = "Below are the details for your Jaeger account. Please abide by the following rules:\r\n" +
                "- Do not share this account with anyone else\r\n" +
                "- Remove this account from the launcher when the scrim is complete\r\n" +
                "- Do not delete any characters from the account\r\n" +
                "- Do not put any characters through the ASP program\r\n\r\n";

            foreach (AccountInfo info in request.Accounts)
            {
                // Attach the account info to the message
                string individualMessage = message;
                individualMessage += $"Jaegar Account Username: {info.AccountUserName}\r\n";
                individualMessage += $"Jaegar Account Password: {info.AccountPassword}\r\n";

                // Send the account info to the user through a DM channel
                IDMChannel channel = await info.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                await channel.SendMessageAsync(individualMessage).ConfigureAwait(false);
                await channel.CloseAsync().ConfigureAwait(false);
            }

            // Build and send the confirmation message to channel in which the original request was made
            string reply = $"Accounts have been distributed to {request.Role.Mention}. Check your DMs! Pairings are:";
            foreach (AccountInfo info in request.Accounts)
                reply += $"\r\n{info}";

            await request.RequestChannel.SendMessageAsync(reply).ConfigureAwait(false);

            // Alert the requester that the distribution was complete,
            // and provide them with a list that can be easily pasted into excel
            reply = "Accounts distributed! Here's an easy-paste list:";
            foreach (AccountInfo info in request.Accounts)
                reply += $"\r\n{info.User.GetFriendlyName()}";
            await context.Channel.SendMessageAsync(reply).ConfigureAwait(false);

            // Remove the request now that it is complete
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
