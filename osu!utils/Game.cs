using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Utils
{
    [DataContract]
    public class Game
    {
        [DataMember(Name = "batch")]
        public string Batch { get; set; }

        [DataMember(Name = "teamblue")]
        public string TeamBlueName { get; set; }

        [DataMember(Name = "teamred")]
        public string TeamRedName { get; set; }
    }
}
