using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScrimBot.Commands
{
    public class TeamsModule : ModuleBase<SocketCommandContext>
    {
        [Command("randomise")]
        [Summary("Randomly places all users with a certain role into two teams, optionally allowing two team leaders to be chosen")]
        public async Task RandomiseCommand(SocketRole toRandomise, SocketGuildUser leaderOne = null, SocketGuildUser leaderTwo = null)
        {
            using IDisposable isTyping = Context.Channel.EnterTypingState();

            // Prevent randomising the everyone role, as this could take forever and might be a mistake
            if (toRandomise.IsEveryone)
            {
                await ReplyAsync("The 'everyone' role cannot be randomised.").ConfigureAwait(false);
                return;
            }

            // Make sure two leaders have been provided, if at least one has been provided
            if (leaderOne != null && leaderTwo == null)
            {
                await ReplyAsync("Two leaders must be specified.").ConfigureAwait(false);
                return;
            }

            // Get all the users who have been assigned the provided role
            List<SocketGuildUser> users = toRandomise.Members.Where(u => !u.IsBot).ToList();

            // Remove the leaders if they are present
            users.Remove(leaderOne);
            users.Remove(leaderTwo);

            // Create the two random teams
            Tuple<string, string> teams = CreateRandomTeams(users);

            // Build the reply
            string reply = "**Team One**";
            if (leaderOne != null)
                reply += " - Leader " + leaderOne.GetFriendlyName();
            reply += teams.Item1;
            reply += "\r\n**Team Two**";
            if (leaderTwo != null)
                reply += " - Leader " + leaderTwo.GetFriendlyName();
            reply += teams.Item2;

            await ReplyAsync(reply).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates two random teams from a list of users
        /// </summary>
        /// <param name="users">The user list from which to create the teams</param>
        /// <returns>The two teams, in a message-ready string format</returns>
        /// <remarks>The return team list will have a line break before the first team member</remarks>
        private Tuple<string, string> CreateRandomTeams(List<SocketGuildUser> users)
        {
            string teamOne = string.Empty;
            string teamTwo = string.Empty;

            users.Shuffle();
            int teamOneCount = users.Count / 2;

            for (int i = 0; i < teamOneCount; i++)
                teamOne += "\r\n" + users[i].GetFriendlyName();

            for (int i = teamOneCount; i < users.Count; i++)
                teamTwo += "\r\n" + users[i].GetFriendlyName();

            return new Tuple<string, string>(teamOne, teamTwo);
        }
    }
}
