using script_chan2.Enums;
using Serilog;
using System.Collections.Generic;

namespace script_chan2.DataTypes
{
    public class MappoolMap
    {
        private ILogger localLog = Log.ForContext<MappoolMap>();

        public MappoolMap(int id = 0)
        {
            Id = id;
            Mods = new List<GameMods>();
        }

        public int Id { get; private set; }

        public Beatmap Beatmap { get; set; }

        public Mappool Mappool { get; set; }

        public string Tag { get; set; }

        public int ListIndex { get; set; }

        public List<GameMods> Mods;

        public void AddMod(GameMods mod)
        {
            localLog.Information("'{name}' add mod '{mod}'", Beatmap.Id, mod);
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
            localLog.Information("'{name}' remove mod '{mod}'", Beatmap.Id, mod);
            Mods.RemoveAll(x => x == mod);
        }

        public void MoveUp()
        {
            localLog.Information("'{name}' move up", Beatmap.Id);
            Mappool.MoveBeatmapUp(this);
        }

        public void MoveDown()
        {
            localLog.Information("'{name}' move down", Beatmap.Id);
            Mappool.MoveBeatmapDown(this);
        }

        public void Save()
        {
            localLog.Information("'{name}' save");
            if (Id == 0)
                Id = Database.Database.AddMappoolMap(this);
            else
                Database.Database.UpdateMappoolMap(this);
        }

        public void Delete()
        {
            localLog.Information("'{name}' delete", Beatmap.Id);
            Mappool.RemoveBeatmap(this);
            Database.Database.DeleteMappoolMap(this);
        }
    }
}
