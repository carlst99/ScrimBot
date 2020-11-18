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
        public async Task AddRolesCommand(SocketTextChannel channel, ulong messageID, string confirmationEmote, SocketRole role)
        {
            using IDisposable typingState = Context.Channel.EnterTypingState();

            IMessage message = await channel.GetMessageAsync(messageID).ConfigureAwait(false);
            if (message is null)
            {
                await ReplyAsync("ScrimBot could not get the provided message. Please ensure you copied the right ID, and that ScrimBot has permissions to view the channel that the message was posted in").ConfigureAwait(false);
                return;
            }

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

            // Get all the users who reacted to the message
            IEnumerable<IUser> usersWhoReacted = await message.GetReactionUsersAsync(emote, int.MaxValue).FlattenAsync().ConfigureAwait(false);

            // Add the role to each user who added a reaction
            string users = string.Empty;
            foreach (IUser user in usersWhoReacted)
            {
                SocketGuildUser guildUser = Context.Guild.GetUser(user.Id);
                if (guildUser is null)
                {
                    await ReplyAsync("Could not add the role to " + user.Username).ConfigureAwait(false);
                    continue;
                }

                await guildUser.AddRoleAsync(role).ConfigureAwait(false);
                users += guildUser.GetFriendlyName() + ", ";
            }

            await ReplyAsync($"The role '{role}' was added to the following members: {users.TrimEnd(',', ' ')}").ConfigureAwait(false);
        }

        [Command("remove-role")]
        [Summary("Removes a role from all users who have it")]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task RemoveRoleCommand(SocketRole role)
        {
            using IDisposable typingState = Context.Channel.EnterTypingState();

            string users = string.Empty;
            foreach (SocketGuildUser user in role.Members)
            {
                await user.RemoveRoleAsync(role).ConfigureAwait(false);
                users += user.GetFriendlyName() + ", ";
            }

            await ReplyAsync($"The role {role} was removed from the following users: {users.TrimEnd(',', ' ')}").ConfigureAwait(false);
        }
    }
}
