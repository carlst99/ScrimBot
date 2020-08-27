using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace ScrimBot
{
    public static class ListExtensions
    {
        private readonly static Random _rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public static class SocketGuildUserExtensions
    {
        public static string GetFriendlyName(this SocketGuildUser user)
        {
            if (!string.IsNullOrEmpty(user.Nickname))
                return user.Nickname;
            else
                return user.Username.Split("#")[0];
        }
    }
}
