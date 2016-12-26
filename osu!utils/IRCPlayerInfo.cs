using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Utils
{
    public class IRCPlayerInfo
    {
        public string _team;
        public string _username;

        public IRCPlayerInfo(string team, string username)
        {
            _team = team;
            _username = username;
        }

        public string Team { get { return _team; } }
        public string Username { get { return _username; } }
        public bool IsSwitched { get; set; }
    }
}
