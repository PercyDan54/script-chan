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
            this.player = player;
            this.game = game;
            this.points = points;
            this.team = team;
            this.passed = passed;
            Mods = new List<GameMods>();
        }

        private Player player;
        public Player Player
        {
            get { return player; }
        }

        private Game game;
        public Game Game
        {
            get { return game; }
        }

        public List<GameMods> Mods { get; set; }

        private int points;
        public int Points
        {
            get { return points; }
        }

        private LobbyTeams team;
        public LobbyTeams Team
        {
            get { return team; }
        }

        private bool passed;
        public bool Passed
        {
            get { return passed; }
        }
    }
}
