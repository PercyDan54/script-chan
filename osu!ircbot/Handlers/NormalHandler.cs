using Osu.Scores;
using System;

namespace Osu.Ircbot
{
    /// <summary>
    /// We are not using this handler anymore, keeping it in case we need to separate admin handler again
    /// </summary>
    class NormalHandler : IrcHandler
    {
        #region Attributes
        /// <summary>
        /// The osu! irc bot
        /// </summary>
        protected OsuIrcBot bot;

        /// <summary>
        /// Registers the bot
        /// </summary>
        /// <param name="bot">the bot</param>
        #endregion

        #region Handlers
        public void RegisterBot(OsuIrcBot bot)
        {
            this.bot = bot;
        }

        public void OnAddRoom(Scores.Room room)
        {
            // Do nothing
        }

        public void OnUpdateRoom(Scores.Room room)
        {
            bot.SendMessage(room.IrcTargets, room.Ranking.ToString());
        }

        public void OnDeleteRoom(Scores.Room room)
        {
            // Do nothing
        }

        public void OnChangeMapRoom(Room room, long map_id, Beatmap beatmap)
        {
            // Do nothing
        }

        public void onReconnectionRoom()
        {
            // Do nothing
        }
        #endregion
    }
}
