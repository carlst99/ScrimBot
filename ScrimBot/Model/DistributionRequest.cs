using Discord.WebSocket;

namespace ScrimBot.Model
{
    public class DistributionRequest
    {
        public SocketUser User { get; set; }
        public ISocketMessageChannel RequestChannel { get; set; }
        public SocketRole Role { get; set; }

        public DistributionRequest(SocketUser user, ISocketMessageChannel requestChannel, SocketRole role)
        {
            User = user;
            RequestChannel = requestChannel;
            Role = role;
        }
    }
}
