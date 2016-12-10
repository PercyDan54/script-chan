using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Ircbot
{
    public class UserSettings
    {
        public int Slot { get; set; }
        public string Status { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string TeamColor { get; set; }
        public string ModsSelected { get; set; }
    }
}
