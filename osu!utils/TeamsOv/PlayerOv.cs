using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Utils.TeamsOv
{
    [DataContract]
    public class PlayerOv
    {
        [DataMember]
        public long Id;
        [DataMember]
        public string Name;
        [DataMember]
        public string Country;
    }
}
