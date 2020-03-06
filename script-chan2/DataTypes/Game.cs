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
            this.room = room;
            this.id = id;
            this.beatmap = beatmap;
        }

        private int id;
        public int Id
        {
            get { return id; }
        }

        private Room room;
        public Room Room
        {
            get { return room; }
        }

        private Beatmap beatmap;
        public Beatmap Beatmap
        {
            get { return beatmap; }
        }

        public List<GameMods> Mods;

        public List<Score> Scores;
    }
}
