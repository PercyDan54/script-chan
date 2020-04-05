using script_chan2.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class Game
    {
        public Game()
        {
            Scores = new List<Score>();
            Mods = new List<GameMods>();
        }

        public int Id { get; set; }

        public Match Match { get; set; }

        public Beatmap Beatmap { get; set; }

        public List<GameMods> Mods;

        public List<Score> Scores;

        public bool Counted { get; set; }

        public bool Warmup { get; set; }
    }
}
