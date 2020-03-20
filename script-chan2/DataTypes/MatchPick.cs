using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class MatchPick
    {
        public Match Match { get; set; }

        public MappoolMap Map { get; set; }

        public Team Team { get; set; }

        public Player Player { get; set; }

        public bool IsBan { get; set; }
    }
}
