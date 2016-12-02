using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Osu.Ircbot
{
    public class FreemodViewer
    {
        public List<UserSettings> Players { get { return players; } }
        public string RoomString { get; set; }
        public string MapString { get; set; }

        public List<UserSettings> players;

        public FreemodViewer()
        {
            players = new List<UserSettings>();
        }
        
        public void AddPlayer(UserSettings us)
        {
            players.Add(us);
        }
    }
}
