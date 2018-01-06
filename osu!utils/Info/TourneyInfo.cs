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
        public Game[] Matches { get; set; }
    }
}
