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

        private DiscordBot(string discordKey)
        {
            _client = new DiscordClient();
            Thread t = new Thread(async () =>
            {
                await _client.Connect(discordKey, TokenType.Bot);
            });

            t.Start();
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
        public static bool Initialize(string discordKey)
        {
            if(!string.IsNullOrEmpty(discordKey))
            {
                try
                {
                    // Initialize the instance
                    instance = new DiscordBot(discordKey);
                    log.Info("Discord bot has been initialized!");
                    return true;
                }
                catch(Exception e)
                {
                    return false;
                }
                
            }
            else
            {
                return false;
            }
            
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
