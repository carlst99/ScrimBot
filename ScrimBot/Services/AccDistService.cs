using Discord.WebSocket;
using System.Collections.Generic;

namespace ScrimBot.Services
{
    public static class AccDistService
    {
        private static readonly Dictionary<SocketUser, DistributionRequest> _distributionRequests = new Dictionary<SocketUser, DistributionRequest>();

        public static bool AddRequest(SocketUser requestMaker, DistributionRequest request)
        {
            if (_distributionRequests.ContainsKey(requestMaker))
            {
                return false;
            } else
            {
                _distributionRequests.Add(requestMaker, request);
                return true;
            }
        }

        public static bool RemoveRequest(SocketUser requestMaker)
        {
            return _distributionRequests.Remove(requestMaker);
        }
    }

    public class DistributionRequest
    {
        public ISocketMessageChannel RequestChannel { get; set; }
        public SocketRole Role { get; set; }

        public DistributionRequest(ISocketMessageChannel requestChannel, SocketRole role)
        {
            RequestChannel = requestChannel;
            Role = role;
        }
    }
}
