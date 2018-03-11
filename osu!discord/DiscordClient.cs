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
        private readonly Encoding _encoding = new UTF8Encoding();
        private bool _isEnabled;
        private static DiscordClient instance;

        public bool IsEnabled { get { return _isEnabled; } }

        public DiscordClient()
        {
            //We don't really care anymore
             _isEnabled = true;
        }

        private string GetLink(DiscordChannelEnum channel)
        {
            switch(channel)
            {
                case DiscordChannelEnum.Default:
                    return InfosHelper.UserDataInfos.WebhookDefault;
                case DiscordChannelEnum.Commentators:
                    return InfosHelper.UserDataInfos.WebhookCommentators;
                case DiscordChannelEnum.Referees:
                    return InfosHelper.UserDataInfos.WebhookReferees;
                case DiscordChannelEnum.Admins:
                    return InfosHelper.UserDataInfos.WebhookAdmins;
                default:
                    return "";
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
                    DiscordHelper.SendGame(embed, new List<DiscordChannelEnum> { DiscordChannelEnum.Default, DiscordChannelEnum.Admins });
                }
            }
        }

        //Post a message using a Payload object
        public async Task<HttpResponseMessage> PostMessage(Payload payload, DiscordChannelEnum channel)
        {
            if(IsEnabled)
            {
                string payloadJson = JsonConvert.SerializeObject(payload, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                string uri = GetLink(channel);

                Uri uriResult;
                bool result = Uri.TryCreate(uri, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                if (!string.IsNullOrEmpty(uri) && result)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        NameValueCollection data = new NameValueCollection();
                        data["payload"] = payloadJson;
                        return await Task.Run(() => client.PostAsync(uri, new StringContent(payloadJson, Encoding.UTF8, "application/json")));
                    }
                }
            }

            return null;
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
