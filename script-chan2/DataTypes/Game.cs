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
        public Game(Room room, int id, Beatmap beatmap)
        {
            Scores = new List<Score>();
            Mods = new List<GameMods>();
            Room = room;
            Id = id;
            Beatmap = beatmap;
        }

        public int Id { get; }

        public Room Room { get; }

        public Beatmap Beatmap { get; }

        public List<GameMods> Mods;

        public List<Score> Scores;
    }
}
