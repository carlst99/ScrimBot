using Discord.WebSocket;
using System.Collections.Generic;

namespace ScrimBot.Model
{
    /// <summary>
    /// Contains information about an account distribution request
    /// </summary>
    public class DistributionRequest
    {
        /// <summary>
        /// The user that requested the distribution
        /// </summary>
        public SocketUser User { get; set; }

        /// <summary>
        /// The channel in which the request was made
        /// </summary>
        public ISocketMessageChannel RequestChannel { get; set; }

        /// <summary>
        /// The role to which the accounts should be distributed
        /// </summary>
        public SocketRole Role { get; set; }

        /// <summary>
        /// The account information to be used when completing a request
        /// </summary>
        public List<AccountInfo> Accounts { get; set; }

        /// <summary>
        /// This value indicates whether or not the request is waiting on user confirmation to complete
        /// </summary>
        public bool WaitingOnConfirmation { get; set; }

        public DistributionRequest(SocketUser user, ISocketMessageChannel requestChannel, SocketRole role)
        {
            User = user;
            RequestChannel = requestChannel;
            Role = role;
        }
    }
}
