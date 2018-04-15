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
    /// <summary>
    /// Discord client which handle webhooks
    /// </summary>
    public class DiscordClient
    {
        #region Attributes
        /// <summary>
        /// The log object for discord part
        /// </summary>
        private static ILog _log = LogManager.GetLogger("osu!discord");

        /// <summary>
        /// The encoding chosen
        /// </summary>
        private readonly Encoding _encoding = new UTF8Encoding();

        /// <summary>
        /// Enabled boolean in case we have a webhook referenced
        /// </summary>
        private bool _isEnabled;

        /// <summary>
        /// The instance for the singleton pattern
        /// </summary>
        private static DiscordClient instance;
        #endregion

        #region Constructors
        public DiscordClient()
        {
            UpdateEnabled();
        }
        #endregion

        #region Properties
        /// <summary>
        /// The IsEnabled property
        /// </summary>
        public bool IsEnabled => _isEnabled;
        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize the discord client by creating the instance
        /// </summary>
        public static void Initialize()
        {
            instance = new DiscordClient();
        }

        /// <summary>
        /// Returns the unique instance of the osu!irc bot
        /// </summary>
        /// <returns>the unique instance</returns>
        public static DiscordClient GetInstance()
        {
            return instance;
        }

        /// <summary>
        /// Function called when a map has been played for a room to display the discord status
        /// </summary>
        /// <param name="room"></param>
        public void OnUpdateRoom(Room room)
        {
            // If they played at least one map which count (in case there is a mappool)
            if (room.Ranking.Played != 0)
            {
                // Log the event
                _log.Info($"OnUpdateRoom triggered for room {room.Id}!");

                // Generate the embed, if there is something create, we send it to discord on default and admins URLs
                Embed embed = room.Ranking.GetDiscordStatus();
                if (embed != null)
                {
                    DiscordHelper.SendGame(embed, new List<DiscordChannelEnum> { DiscordChannelEnum.Default, DiscordChannelEnum.Admins });
                }
            }
        }

        /// <summary>
        /// Post a message using a Payload object
        /// </summary>
        /// <param name="payload">The payload object</param>
        /// <param name="channel">The channel we are willing to send the payload</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostMessage(Payload payload, DiscordChannelEnum channel)
        {
            // If the feature is enabled
            if (IsEnabled)
            {
                // We serialize the payload
                string payloadJson = JsonConvert.SerializeObject(payload, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                // Retrieve the URL webhook
                string uri = GetLink(channel);

                // Verify if the URL entered is at least a working URI
                Uri uriResult;
                bool result = Uri.TryCreate(uri, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                // If the URI is correct
                if (!string.IsNullOrEmpty(uri) && result)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        // Add the payload in the data and post through the httpclient
                        NameValueCollection data = new NameValueCollection();
                        data["payload"] = payloadJson;
                        return await Task.Run(() => client.PostAsync(uri, new StringContent(payloadJson, Encoding.UTF8, "application/json")));
                    }
                }
            }

            return null;
        }

        public bool UpdateEnabled()
        {
            _isEnabled = Uri.TryCreate(GetLink(DiscordChannelEnum.Admins), UriKind.Absolute, out var uriResult)
                          && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return _isEnabled;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Get the webhook url for the selected channel
        /// </summary>
        /// <param name="channel">The channel we need to send the payload</param>
        /// <returns>The URL</returns>
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
        #endregion
    }
}
