using Discord.Commands;
using Discord.WebSocket;
using ScrimBot.Model;
using ScrimBot.Services;
using System;
using System.Threading.Tasks;

namespace ScrimBot.Commands
{
    public class AccountDistributionModule : ModuleBase<SocketCommandContext>
    {
        [Command("distribute-accounts")]
        [Summary("Begins a request to distribute accounts to members of a particular role via their DMs")]
        public async Task DistributeCommand(SocketRole role)
        {
            IDisposable typing = Context.Channel.TriggerTypingAsync();

            // Create an account distribution request and attempt to add it to the distribution service
            DistributionRequest request = new DistributionRequest(Context.User, Context.Channel, role);
            if (await AccountDistributionService.AddRequest(request).ConfigureAwait(false))
                await ReplyAsync($"Please check your DMs {Context.User.Mention}! This request will time out in five minutes.").ConfigureAwait(false);
            else
                await ReplyAsync($"{Context.User.Mention}, a distribution request has already been sent to you! Use the 'clear-account-distribution' command to cancel it.").ConfigureAwait(false);

            typing.Dispose();
        }

        [Command("clear-account-distribution")]
        [Summary("Attemps to clear an account distribution request made by the sender")]
        public async Task ClearDistributionCommand()
        {
            if (AccountDistributionService.RemoveRequest(Context.User))
                await ReplyAsync($"{Context.User.Mention}, your request was successfully cancelled.").ConfigureAwait(false);
            else
                await ReplyAsync($"I couldn't cancel your request {Context.User.Mention}. Please wait for it to time out, or make one in the first place.").ConfigureAwait(false);
        }
    }
}
