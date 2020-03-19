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
        public Mappool(int id = 0)
        {
            Beatmaps = new List<MappoolMap>();
            Id = id;
        }

        public int Id { get; private set; }

        public string Name { get; set; }

        public void Save()
        {
            if (Id == 0)
                Id = Database.Database.AddMappool(this);
            else
                Database.Database.UpdateMappool(this);
        }

        public Tournament Tournament { get; set; }

        public List<MappoolMap> Beatmaps;

        private void ReindexBeatmaps()
        {
            for (var i = 0; i < Beatmaps.Count; i++)
            {
                Beatmaps[i].ListIndex = i + 1;
                Beatmaps[i].Save();
            }
        }

        public void AddBeatmap(MappoolMap beatmap)
        {
            if (!Beatmaps.Contains(beatmap))
            {
                Beatmaps.Add(beatmap);
                beatmap.ListIndex = Beatmaps.Count;
            }
        }

        public void RemoveBeatmap(MappoolMap beatmap)
        {
            if (Beatmaps.Remove(beatmap))
            {
                ReindexBeatmaps();
            }
        }

        public void MoveBeatmapUp(MappoolMap beatmap)
        {
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
            Database.Database.DeleteMappool(this);
        }
    }
}
