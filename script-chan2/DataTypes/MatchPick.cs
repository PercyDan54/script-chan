using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class MatchPick
    {
        public MatchPick(Match match, MappoolMap map, Team team, Player player)
        {
            Match = match;
            Map = map;
            Team = team;
            Player = player;
        }

        public Match Match { get; set; }

        public MappoolMap Map { get; set; }

        public Team Team { get; set; }

        public Player Player { get; set; }
    }
}
