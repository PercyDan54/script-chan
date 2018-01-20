using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_utils.DiscordModels
{
    public class Embed
    {
        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("fields")]
        public List<Field> Fields { get; set; }

        [JsonProperty("thumbnail")]
        public Image Thumbnail { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("footer")]
        public Footer Footer { get; set; }
    }
}
