using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class Mappool
    {
        private ILogger localLog = Log.ForContext<Mappool>();

        public Mappool(int id = 0)
        {
            Beatmaps = new List<MappoolMap>();
            Id = id;
        }

        public int Id { get; private set; }

        public string Name { get; set; }

        public void Save()
        {
            localLog.Information("'{name}' save", Name);
            if (Id == 0)
                Id = Database.Database.AddMappool(this);
            else
                Database.Database.UpdateMappool(this);
        }

        public Tournament Tournament { get; set; }

        public List<MappoolMap> Beatmaps;

        private void ReindexBeatmaps()
        {
            localLog.Information("'{name}' reindex beatmaps", Name);
            for (var i = 0; i < Beatmaps.Count; i++)
            {
                Beatmaps[i].ListIndex = i + 1;
                Beatmaps[i].Save();
            }
        }

        public void AddBeatmap(MappoolMap beatmap)
        {
            localLog.Information("'{name}' add beatmap '{beatmap}'", Name, beatmap.Beatmap.Id);
            if (!Beatmaps.Contains(beatmap))
            {
                Beatmaps.Add(beatmap);
                beatmap.ListIndex = Beatmaps.Count;
            }
        }

        public void RemoveBeatmap(MappoolMap beatmap)
        {
            localLog.Information("'{name}' remove beatmap '{beatmap}'", Name, beatmap.Beatmap.Id);
            if (Beatmaps.Remove(beatmap))
            {
                ReindexBeatmaps();
            }
        }

        public void MoveBeatmapUp(MappoolMap beatmap)
        {
            localLog.Information("'{name}' move beatmap '{beatmap}' up", Name, beatmap.Beatmap.Id);
            if (!Beatmaps.Contains(beatmap))
                return;

            var index = Beatmaps.IndexOf(beatmap);
            if (index == 0)
                return;

            var tmp = Beatmaps[index - 1];
            Beatmaps[index - 1] = Beatmaps[index];
            Beatmaps[index] = tmp;

            ReindexBeatmaps();
        }

        public void MoveBeatmapDown(MappoolMap beatmap)
        {
            localLog.Information("'{name}' move beatmap '{beatmap}' down", Name, beatmap.Beatmap.Id);
            if (!Beatmaps.Contains(beatmap))
                return;

            var index = Beatmaps.IndexOf(beatmap);
            if (index >= Beatmaps.Count - 1)
                return;

            var tmp = Beatmaps[index + 1];
            Beatmaps[index + 1] = Beatmaps[index];
            Beatmaps[index] = tmp;

            ReindexBeatmaps();
        }

        public void Delete()
        {
            localLog.Information("'{name}' delete", Name);
            foreach (var match in Database.Database.Matches)
            {
                if (match.Mappool == this)
                {
                    match.Mappool = null;
                    match.Picks.Clear();
                    match.Bans.Clear();
                    match.Save();
                }
            }
            Database.Database.DeleteMappool(this);
        }
    }
}
