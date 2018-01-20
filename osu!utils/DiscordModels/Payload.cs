using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_utils.DiscordModels
{
    /// <summary>
    /// This class serialize into required JSON for discord
    /// </summary>
    public class Payload
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("embeds")]
        public List<Embed> Embeds { get; set; }
    }
}
