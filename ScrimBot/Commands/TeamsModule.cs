using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScrimBot.Commands
{
    public class TeamsModule : ModuleBase<SocketCommandContext>
    {
        [Command("randomise")]
        [Summary("Randomly places all users with a certain role into two teams")]
        public async Task RandomiseCommand(SocketRole toRandomise)
        {
            if (toRandomise.IsEveryone)
                await ReplyAsync("That could take a while! Use the 'randomise-everyone' command to randomise that role").ConfigureAwait(false);
            else
                await RandomiseEveryoneCommand(toRandomise).ConfigureAwait(false);
        }

        [Command("randomise-everyone")]
        [Summary("Randomly places all users with a certain role into two teams. This variant adds support for the 'everyone' role")]
        [RequireUserPermission(ChannelPermission.MentionEveryone)]
        public async Task RandomiseEveryoneCommand(SocketRole toRandomise)
        {
            await Context.Channel.TriggerTypingAsync().ConfigureAwait(false);

            string reply = "**Team One**";
            List<SocketGuildUser> users = toRandomise.Members.Where(u => !u.IsBot).ToList();
            users.Shuffle();

            int teamOneCount = users.Count / 2;
            for (int i = 0; i < teamOneCount; i++)
                reply += "\r\n" + users[i].GetFriendlyName();

            reply += "\r\n**Team Two**";
            for (int i = teamOneCount; i < users.Count; i++)
                reply += "\r\n" + users[i].GetFriendlyName();

            await ReplyAsync(reply).ConfigureAwait(false);
        }
    }
}
