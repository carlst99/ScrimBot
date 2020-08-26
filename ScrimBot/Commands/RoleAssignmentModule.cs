using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScrimBot.Commands
{
    public class RoleAssignmentModule : ModuleBase<SocketCommandContext>
    {
        [Command("add-roles")]
        [Summary("Adds a role to all users who reacted to a message")]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task AddRolesCommand(ulong messageID, string confirmationEmote, SocketRole role)
        {
            IDisposable typingState = Context.Channel.EnterTypingState();
            IMessage message = await Context.Channel.GetMessageAsync(messageID).ConfigureAwait(false);

            IEmote emote = null;
            try
            {
                emote = message.Reactions.Keys.First(e => e.Name.Equals(confirmationEmote));
            } catch
            {
                await ReplyAsync("The message did not contain a reaction that matched the provided one").ConfigureAwait(false);
                return;
            }

            IEnumerable<IUser> usersWhoReacted = await message.GetReactionUsersAsync(emote, int.MaxValue).FlattenAsync().ConfigureAwait(false);

            string users = string.Empty;
            foreach (IUser user in usersWhoReacted)
            {
                SocketGuildUser guildUser = Context.Guild.GetUser(user.Id);
                await guildUser.AddRoleAsync(role).ConfigureAwait(false);
                users += guildUser.GetEasyName() + ", ";
            }

            typingState.Dispose();
            await ReplyAsync("Role added to the following members: " + users.TrimEnd(',', ' ')).ConfigureAwait(false);

            // TODO list people to which the roles were added
        }

        [Command("remove-role")]
        [Summary("Removes a role from all users who have it")]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task RemoveRoleCommand(SocketRole role)
        {
            IDisposable typingState = Context.Channel.EnterTypingState();

            string users = string.Empty;
            foreach (SocketGuildUser user in role.Members)
            {
                await user.RemoveRoleAsync(role).ConfigureAwait(false);
                users += user.GetEasyName() + ", ";
            }

            typingState.Dispose();
            await ReplyAsync("Role removed from the following users: " + users.TrimEnd(',', ' ')).ConfigureAwait(false);
        }
    }
}
