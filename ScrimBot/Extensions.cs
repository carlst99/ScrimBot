using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace ScrimBot
{
    public static class ListExtensions
    {
        private readonly static Random _rng = new Random();

        /// <summary>
        /// Shuffles a list using the Fisher-Yates shuffle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
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
        /// <summary>
        /// Returns either a user's nickname, or their username stripped of the unique tag
        /// </summary>
        /// <param name="user"></param>
        public static string GetFriendlyName(this SocketGuildUser user)
        {
            if (!string.IsNullOrEmpty(user.Nickname))
                return user.Nickname;
            else
                return user.Username.Split("#")[0];
        }
    }
}
