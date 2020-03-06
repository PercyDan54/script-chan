using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class Beatmap
    {
        public Beatmap(int id, int setId, string artist, string title, string version, string creator)
        {
            Id = id;
            SetId = setId;
            Artist = artist;
            Title = title;
            Version = version;
            Creator = creator;
        }

        public int Id { get; }

        public int SetId { get; }

        public string Artist { get; }

        public string Title { get; }

        public string Version { get; }

        public string Creator { get; }
    }
}
