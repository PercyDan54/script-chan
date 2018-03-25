using Osu.Scores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Ircbot.Handlers
{
    class AdminHandler : IrcHandler
    {

        #region Constants
        #endregion

        #region Attributes
        /// <summary>
        /// The osu! irc bot
        /// </summary>
        protected OsuIrcBot bot;

        protected List<string> channels;
        #endregion

        #region Handlers
        public void RegisterBot(OsuIrcBot bot)
        {
            this.bot = bot;
            this.channels = new List<string>();
        }

        public void OnAddRoom(Scores.Room room)
        {
            string irc_room = "#mp_" + room.Id;

            channels.Add(irc_room);
            room.IrcTargets.Add(irc_room);
            bot.JoinChannel(irc_room);

        }

        public void OnUpdateRoom(Scores.Room room)
        {
            foreach (string message in room.Ranking.GetStatus())
            {
                bot.SendMessage(room.IrcTargets, message);
            }

            Task.Factory.StartNew(() => StartCounter(room));
        }

        public async void StartCounter(Scores.Room room)
        {
            await Task.Delay(5000);
            if(room.Status != Scores.Status.RoomStatus.Finished && room.Timer > 0)
            {
                bot.SendMessage("#mp_" + room.Id, "!mp timer " + room.Timer);
            }
        }

        public void OnDeleteRoom(Scores.Room room)
        {
            string irc_room = "#mp_" + room.Id;

            channels.Remove(irc_room);
            bot.LeaveChannel(irc_room);
        }

        public void OnChangeMapRoom(Room room, long map_id, Beatmap beatmap)
        {
            List<string> target = new List<string> { "#mp_" + room.Id };
            bot.SendMessage(target, "Picked map : " + beatmap.ToString());
            bot.SendMessage(target, "!mp map " + map_id);
            bot.SendMessage(target, "!mp mods " + getPickType(beatmap.PickType));
        }

        public void onReconnectionRoom()
        {
            foreach (string channel in channels)
            {
                bot.JoinChannel(channel);
            }
        }

        public string getPickType(PickType mod)
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
