using Discord.WebSocket;

namespace ScrimBot.Model
{
    /// <summary>
    /// Contains information about a scrim account
    /// </summary>
    public class AccountInfo
    {
        /// <summary>
        /// The user that this account was distributed to
        /// </summary>
        public SocketGuildUser User { get; set; }

        /// <summary>
        /// The username of this account
        /// </summary>
        public string AccountUserName { get; set; }

        /// <summary>
        /// The password of this account
        /// </summary>
        public string AccountPassword { get; set; }

        public static AccountInfo Parse(string account)
        {
            string[] components = account.Split("    ");
            return new AccountInfo
            {
                AccountUserName = components[0],
                AccountPassword = components[1]
            };
        }

        public override string ToString()
        {
            return AccountUserName + " - " + User?.GetFriendlyName();
        }
    }
}
