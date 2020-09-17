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
        public async Task AddRolesCommand(ulong channelID, ulong messageID, string confirmationEmote, SocketRole role)
        {
            IDisposable typingState = Context.Channel.EnterTypingState();
            SocketTextChannel messageChannel = (SocketTextChannel)Context.Guild.GetChannel(channelID);
            IMessage message = await messageChannel.GetMessageAsync(messageID).ConfigureAwait(false);

            // Attempt to find the right reaction on the message
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

            // Add the role to each user who added a reaction
            string users = string.Empty;
            foreach (IUser user in usersWhoReacted)
            {
                SocketGuildUser guildUser = Context.Guild.GetUser(user.Id);
                await guildUser.AddRoleAsync(role).ConfigureAwait(false);
                users += guildUser.GetFriendlyName() + ", ";
            }

            typingState.Dispose();
            await ReplyAsync($"The role {role.Mention} was added to the following members: {users.TrimEnd(',', ' ')}").ConfigureAwait(false);
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
                users += user.GetFriendlyName() + ", ";
            }

            typingState.Dispose();
            await ReplyAsync($"The role {role.Mention} was removed from the following users: {users.TrimEnd(',', ' ')}").ConfigureAwait(false);
        }
    }
}
