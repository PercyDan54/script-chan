using script_chan2.Enums;
using System.Collections.Generic;

namespace script_chan2.DataTypes
{
    public class Score
    {
        public Score()
        {
            Mods = new List<GameMods>();
        }

        public Player Player { get; set; }
        public Game Game { get; set; }

        public List<GameMods> Mods { get; set; }
        public int Points { get; set; }
        public LobbyTeams Team { get; set; }
        public bool Passed { get; set; }
    }
}
