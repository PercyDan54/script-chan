using Osu.Scores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Ircbot.Handlers
{
    /// <summary>
    /// The AdminHandler class to control the irc bot
    /// </summary>
    class AdminHandler : IrcHandler
    {
        #region Attributes
        /// <summary>
        /// The osu! irc bot
        /// </summary>
        protected OsuIrcBot bot;

        /// <summary>
        /// The list of channels we are currently in
        /// </summary>
        protected List<string> channels;
        #endregion

        #region Handlers
        /// <summary>
        /// Register the irc bot class to the handler
        /// </summary>
        /// <param name="bot"></param>
        public void RegisterBot(OsuIrcBot bot)
        {
            this.bot = bot;
            this.channels = new List<string>();
        }

        /// <summary>
        /// Function used when a room has been added, we try to join it on IRC
        /// </summary>
        /// <param name="room"></param>
        public void OnAddRoom(Scores.Room room)
        {
            string irc_room = "#mp_" + room.Id;

            channels.Add(irc_room);
            room.IrcTargets.Add(irc_room);
            bot.JoinChannel(irc_room);

        }

        /// <summary>
        /// Function used when the room has finished to play a map, we print the current status of the room (score, next team to pick..)
        /// </summary>
        /// <param name="room"></param>
        public void OnUpdateRoom(Scores.Room room)
        {
            foreach (string message in room.Ranking.GetStatus())
            {
                bot.SendMessage(room.IrcTargets, message);
            }

            Task.Factory.StartNew(() => StartCounter(room));
        }

        /// <summary>
        /// Function used to start the counter in the osu! room
        /// </summary>
        /// <param name="room"></param>
        public async void StartCounter(Scores.Room room)
        {
            await Task.Delay(5000);

            // If the game has not been finished and that the timer is activated, we trigger the command
            if(room.Status != Scores.Status.RoomStatus.Finished && room.Timer > 0)
            {
                bot.SendMessage("#mp_" + room.Id, "!mp timer " + room.Timer);
            }
        }

        /// <summary>
        /// Function used when the room has been deleted on the application, we leave the IRC channel
        /// </summary>
        /// <param name="room"></param>
        public void OnDeleteRoom(Scores.Room room)
        {
            string irc_room = "#mp_" + room.Id;

            channels.Remove(irc_room);
            bot.LeaveChannel(irc_room);
        }

        /// <summary>
        /// Function called when the room has picked a new map from a mappool, we send the commands related to change the map and mods
        /// </summary>
        /// <param name="room">The room object</param>
        /// <param name="map_id">The map id we need to pick</param>
        /// <param name="beatmap">The beatmap object containing mods</param>
        public void OnChangeMapRoom(Room room, long map_id, Beatmap beatmap)
        {
            List<string> target = new List<string> { "#mp_" + room.Id };
            bot.SendMessage(target, "Picked map : " + beatmap.ToString());
            bot.SendMessage(target, "!mp map " + map_id);
            bot.SendMessage(target, "!mp mods " + GetPickType(beatmap.PickType));
        }

        /// <summary>
        /// Function called when we need to reconnect to all channels
        /// </summary>
        public void onReconnectionRoom()
        {
            foreach (string channel in channels)
            {
                bot.JoinChannel(channel);
            }
        }

        /// <summary>
        /// Retrieving the correct string for osu! mods command for the selected mod
        /// </summary>
        /// <param name="mod">The selected mod</param>
        /// <returns>The string of the mod selected</returns>
        private string GetPickType(PickType mod)
        {
            string res;
            switch (mod)
            {
                case PickType.NoMod:
                    res = "None";
                    break;
                case PickType.HardRock:
                    res = "HR";
                    break;
                case PickType.DoubleTime:
                    res = "DT";
                    break;
                case PickType.Hidden:
                    res = "HD";
                    break;
                case PickType.FreeMod:
                    res = "Freemod";
                    break;
                case PickType.TieBreaker:
                    res = "Freemod";
                    break;
                default:
                    res = "None";
                    break;
            }
            return res;
        }
        #endregion
    }
}
