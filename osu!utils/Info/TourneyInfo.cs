using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Utils
{
    [DataContract]
    public class TourneyInfo
    {
        [DataMember(Name = "acronym")]
        public string Acronym { get; set; }

        [DataMember(Name = "teammode")]
        public string TeamMode { get; set; }

        [DataMember(Name = "scoremode")]
        public string ScoreMode { get; set; }

        [DataMember(Name = "size")]
        public string RoomSize { get; set; }

        [DataMember(Name = "defaultmap")]
        public string DefaultMapId { get; set; }

        [DataMember(Name = "modetype")]
        public string ModeType { get; set; }

        [DataMember(Name = "playersperteam")]
        public int PlayersPerTeam { get; set; }

        [DataMember(Name = "matches")]
        public List<Game> Matches { get; set; }

        [DataMember(Name = "timer")]
        public int Timer { get; set; }

        [DataMember(Name = "secondbancount")]
        public int SecondBanCount { get; set; }

        [DataMember(Name = "welcomemessage")]
        public string WelcomeMessage { get; set; }

        /// <summary>
        /// Saves the matches in the cache
        /// </summary>
        public void Save()
        {
            Cache cache = Cache.GetCache("osu!matches.db");
            cache["infos"] = this;
        }

        public void CheckValue()
        {
            if (string.IsNullOrEmpty(TeamMode))
                TeamMode = "0";

            if (string.IsNullOrEmpty(ScoreMode))
                ScoreMode = "0";

            if (string.IsNullOrEmpty(RoomSize))
                RoomSize = "8";

            if (string.IsNullOrEmpty(ModeType))
                ModeType = "0";

            if (PlayersPerTeam == 0)
                PlayersPerTeam = 4;

            if (Timer == 0)
                Timer = 180;

            if (string.IsNullOrEmpty(WelcomeMessage))
                WelcomeMessage = "Please invite your teammates and sort yourself accordingly";
        }
    }
}
