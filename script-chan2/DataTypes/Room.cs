using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class Room
    {
        public Room(int id)
        {
            Games = new List<Game>();
            Id = id;
            LastEventId = 0;
        }

        public int Id { get; }

        public int LastEventId { get; set; }

        public string Name { get; set; }

        public List<Game> Games;

        public void Refresh()
        {
            OsuApi.OsuApi.UpdateRoom(this);
        }
    }
}
