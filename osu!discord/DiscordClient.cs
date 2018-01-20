using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Osu.Utils.Info;
using System.Net.Http;
using osu_utils.DiscordModels;
using Osu.Scores;
using log4net;

namespace osu_discord
{
    public class DiscordClient
    {
        private static ILog log = LogManager.GetLogger("osu!discord");
        private Dictionary<DiscordChannelEnum, string> _uriDic;
        private readonly Encoding _encoding = new UTF8Encoding();
        private bool _isEnabled;
        private static DiscordClient instance;

        public bool IsEnabled { get { return _isEnabled; } }

        public DiscordClient()
        {
            _uriDic = new Dictionary<DiscordChannelEnum, string>();
            if (!string.IsNullOrEmpty(InfosHelper.UserDataInfos.WebhookDefault))
            {
                _uriDic.Add(DiscordChannelEnum.Default, InfosHelper.UserDataInfos.WebhookDefault);
                _isEnabled = true;

                if (!string.IsNullOrEmpty(InfosHelper.UserDataInfos.WebhookReferees))
                {
                    _uriDic.Add(DiscordChannelEnum.Referees, InfosHelper.UserDataInfos.WebhookReferees);
                }
                if (!string.IsNullOrEmpty(InfosHelper.UserDataInfos.WebhookCommentators))
                {
                    _uriDic.Add(DiscordChannelEnum.Commentators, InfosHelper.UserDataInfos.WebhookCommentators);
                }
                if (!string.IsNullOrEmpty(InfosHelper.UserDataInfos.WebhookAdmins))
                {
                    _uriDic.Add(DiscordChannelEnum.Admins, InfosHelper.UserDataInfos.WebhookAdmins);
                }
            }
            else
            {
                _isEnabled = true;
            }
        }

        public static void Initialize()
        {
            instance = new DiscordClient();
        }

        public void OnUpdateRoom(Room room)
        {
            if (room.Ranking.Played != 0)
            {
                log.Info(string.Format("OnUpdateRoom triggered for room {0}!", room.Id));
                Embed embed = room.Ranking.GetDiscordStatus();
                if(embed != null)
                { 
                    DiscordHelper.SendGame(embed, DiscordChannelEnum.Default);
                }
            }
        }

        //Post a message using a Payload object
        public void PostMessage(Payload payload, DiscordChannelEnum channel)
        {
            if(IsEnabled)
            {
                string payloadJson = JsonConvert.SerializeObject(payload, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                string uri = _uriDic[channel] != null ? _uriDic[channel] : _uriDic[DiscordChannelEnum.Default];

                using (HttpClient client = new HttpClient())
                {
                    NameValueCollection data = new NameValueCollection();
                    data["payload"] = payloadJson;

                    var response = client.PostAsync(uri, new StringContent(payloadJson, Encoding.UTF8, "application/json")).Result;
                }
            }
        }

        /// <summary>
        /// Returns the unique instance of the osu!irc bot
        /// </summary>
        /// <returns>the unique instance</returns>
        public static DiscordClient GetInstance()
        {
            return instance;
        }
    }
}
