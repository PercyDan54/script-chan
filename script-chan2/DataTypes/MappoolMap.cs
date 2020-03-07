using script_chan2.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class MappoolMap
    {
        public MappoolMap(Mappool mappool, Beatmap beatmap, string mods, int id = 0)
        {
            Mods = new List<GameMods>();
            foreach (string mod in mods.Split(','))
            {
                if (Enum.TryParse(mod, out GameMods gameMod))
                {
                    Mods.Add(gameMod);
                }
            }
            this.mappool = mappool;
            this.beatmap = beatmap;
            this.id = id;
        }

        private int id;
        public int Id
        {
            get { return id; }
        }

        private Beatmap beatmap;
        public Beatmap Beatmap
        {
            get { return beatmap; }
            set
            {
                if (value != beatmap)
                {
                    beatmap = value;
                }
            }
        }

        private Mappool mappool;
        public Mappool Mappool
        {
            get { return mappool; }
        }

        private int listIndex;
        public int ListIndex
        {
            get { return listIndex; }
            set
            {
                if (value != listIndex)
                {
                    listIndex = value;
                }
            }
        }

        public List<GameMods> Mods;

        public void AddMod(GameMods mod)
        {
            if (Mods.Contains(mod))
                return;

            if (mod == GameMods.TieBreaker)
                Mods.Clear();
            else if (mod == GameMods.DoubleTime)
                Mods.RemoveAll(x => x == GameMods.TieBreaker);
            else
                Mods.RemoveAll(x => x == GameMods.DoubleTime || x == GameMods.TieBreaker);

            Mods.Add(mod);
        }

        public void RemoveMod(GameMods mod)
        {
            Mods.RemoveAll(x => x == mod);
        }

        public void MoveUp()
        {
            mappool.MoveBeatmapUp(this);
        }

        public void MoveDown()
        {
            mappool.MoveBeatmapDown(this);
        }

        public void Save()
        {
            if (id == 0)
                id = Database.Database.AddMappoolMap(this);
            else
                Database.Database.UpdateMappoolMap(this);
        }

        public void Delete()
        {
            Mappool.RemoveBeatmap(this);
            Database.Database.DeleteMappoolMap(this);
        }
    }
}
