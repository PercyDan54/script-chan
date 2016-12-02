using Discord;
using log4net;
using Osu.Scores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace osu_discord
{
    public class DiscordBot
    {
        private DiscordClient _client;

        private static DiscordBot instance;

        private static ILog log = LogManager.GetLogger("osu!discord");

        private DiscordBot()
        {
            _client = new DiscordClient();
            Thread t = new Thread(async () =>
            {
                await _client.Connect("MjQ3MDE1Mjk2MzgzMDU3OTIz.CwjEVw.BesdwOA3pKPeM2rBDTS9ErDBYnM", TokenType.Bot);
            });

            t.Start();
        }

        private void Start()
        {
            _client.ExecuteAndWait(async () => {
                await _client.Connect("MjQ3MDE1Mjk2MzgzMDU3OTIz.CwjEVw.BesdwOA3pKPeM2rBDTS9ErDBYnM", TokenType.Bot);
            });
        }

        public void OnUpdateRoom(Room room)
        {
            if (room.Ranking.Played != 0)
            {
                log.Info(string.Format("OnUpdateRoom triggered for room {0}!", room.Id));
                List<string> sentences = room.Ranking.GetDiscordStatus();
                if(sentences != null)
                {
                    string test = String.Join(Environment.NewLine, sentences);
                    SendMessage(test);
                }
            }
        }

        public void SendMessage(string message)
        {
            if(!string.IsNullOrEmpty(message))
            {
                var server = _client.Servers.FirstOrDefault(x => x.Name == "osu! staff");
                var channel = server?.TextChannels.FirstOrDefault(x => x.Name == "tourney");
                channel?.SendMessage(message);
            }
            else
            {
                log.Warn("SendMessage null or empty!");
            }
        }

        #region Static Methods
        /// <summary>
        /// Initialize the osu!ircbot
        /// </summary>
        public static void Initialize()
        {
            // Initialize the instance
            instance = new DiscordBot();
            log.Info("Discord bot has been initialized!");
        }

        /// <summary>
        /// Returns the unique instance of the osu!irc bot
        /// </summary>
        /// <returns>the unique instance</returns>
        public static DiscordBot GetInstance()
        {
            return instance;
        }
        #endregion
    }
}
