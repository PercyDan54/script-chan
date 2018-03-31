using Discord;
using Discord.WebSocket;
using log4net;
using Osu.Scores;
using Osu.Utils.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace osu_discord
{
    /// <summary>
    /// Discord bot has been dropped for now, we are using Webhooks.
    /// </summary>
    public class DiscordBot
    {
        private DiscordSocketClient _client;

        private static DiscordBot instance;

        private static ILog log = LogManager.GetLogger("osu!discord");

        private string serverName;
        private string channelName;

        private DiscordBot(string discordKey, string sName, string cName)
        {
            serverName = sName;
            channelName = cName;

            var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
            _client = new DiscordSocketClient(_config);
            Thread t = new Thread(async () =>
            {
                await _client.LoginAsync(TokenType.Bot, discordKey);
                await _client.StartAsync();
            });

            t.Start();
        }

        public async Task OnUpdateRoom(Room room)
        {
            /*
            if (room.Ranking.Played != 0)
            {
                log.Info(string.Format("OnUpdateRoom triggered for room {0}!", room.Id));
                //List<string> sentences = room.Ranking.GetDiscordStatus();
                if(sentences != null)
                {
                    string test = String.Join(Environment.NewLine, sentences);
                    await SendMessage(test);
                }
            }
            */
        }

        public async Task SendMessage(string message)
        {
            if(!string.IsNullOrEmpty(message))
            {
                var server = _client.Guilds.FirstOrDefault(x => x.Name == serverName);
                var channel = server?.TextChannels.FirstOrDefault(x => x.Name == channelName);
                await channel?.SendMessageAsync(message);
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
            /*
            if(!string.IsNullOrEmpty(InfosHelper.UserDataInfos.DiscordKey))
            {
                try
                {
                    // Initialize the instance
                    instance = new DiscordBot(InfosHelper.UserDataInfos.DiscordKey, InfosHelper.UserDataInfos.DiscordServerName, InfosHelper.UserDataInfos.DiscordChannelName);
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
            */
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
