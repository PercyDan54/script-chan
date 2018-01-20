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

        [DataMember(Name = "webhookdefault")]
        public string WebhookDefault { get; set; }

        [DataMember(Name = "webhookcommentators")]
        public string WebhookCommentators { get; set; }

        [DataMember(Name = "webhookreferees")]
        public string WebhookReferees { get; set; }

        [DataMember(Name = "webhookadmins")]
        public string WebhookAdmins { get; set; }

        [DataMember(Name = "commentatorsgroup")]
        public string DiscordCommentatorGroup { get; set; }
    }
}
