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
        public MappoolMap(int id = 0)
        {
            Id = id;
            Mods = new List<GameMods>();
        }

        public int Id { get; private set; }

        public Beatmap Beatmap { get; set; }
        public Mappool Mappool { get; set; }

        public int ListIndex { get; set; }

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
            Mappool.MoveBeatmapUp(this);
        }

        public void MoveDown()
        {
            Mappool.MoveBeatmapDown(this);
        }

        public void Save()
        {
            if (Id == 0)
                Id = Database.Database.AddMappoolMap(this);
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
