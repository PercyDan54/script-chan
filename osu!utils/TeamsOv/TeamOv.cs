using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Utils.TeamsOv
{
    [DataContract]
    public class TeamOv
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<PlayerOv> Players { get; set; }

        public TeamOv(string name)
        {
            this.Name = name;
            this.Players = new List<PlayerOv>();
        }
    }
}
