using Discord.Commands;
using System.Threading.Tasks;

namespace ScrimBot.Commands
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Summary("Explains each command offered by ScrimBot")]
        public async Task HelpCommand()
        {
            const string reply = "ScrimBot offers the following commands:\r\n" +
                "**add-roles <channelID> <messageID> <reactionEmote> <role>**\r\n" +
                "Finds the message with the given ID in the given channel, and assigns a role to all users who have reacted with the given emote\r\n\r\n" +
                "**remove-role <role>**\r\n" +
                "Removes the given role from all users who have been assigned it\r\n\r\n" +
                "**randomise <role> [leaderOne] [leaderTwo]**\r\n" +
                "Randomly places all members with the provided role into two teams. Optionally, two leaders may be provided\r\n\r\n" +
                "**distribute-accounts <role>**\r\n" +
                "Begins a guided process to distribute Jaeger accounts via DM to all users with the provided role\r\n\r\n" +
                "**cancel-account-distribution**\r\n" +
                "Cancels an account distribution request made by the calling user";

            await ReplyAsync(reply).ConfigureAwait(false);
        }
    }
}
