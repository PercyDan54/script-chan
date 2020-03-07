using script_chan2.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class Score
    {
        public Score(Game game, Player player, int points, LobbyTeams team, bool passed)
        {
            Player = player;
            Game = game;
            Points = points;
            Team = team;
            Passed = passed;
            Mods = new List<GameMods>();
        }

        public Player Player { get; }
        public Game Game { get; }

        public List<GameMods> Mods { get; set; }
        public int Points { get; }
        public LobbyTeams Team { get; }
        public bool Passed { get; }
    }
}
