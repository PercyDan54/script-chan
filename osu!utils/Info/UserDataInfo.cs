using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Utils.Info
{
    [DataContract]
    public class UserDataInfo
    {
        [DataMember(Name = "ircpublic")]
        public string IPPublicBancho { get; set; }

        [DataMember(Name = "ircprivate")]
        public string IPPrivateBancho { get; set; }

        [DataMember(Name = "admins")]
        public string Admins { get; set; }

        [DataMember(Name = "discordkey")]
        public string DiscordKey { get; set; }

        [DataMember(Name = "discordguildname")]
        public string DiscordServerName { get; set; }

        [DataMember(Name = "discordchannelname")]
        public string DiscordChannelName { get; set; }

        [DataMember(Name = "commentatorsgroup")]
        public string DiscordCommentatorGroup { get; set; }
    }
}
