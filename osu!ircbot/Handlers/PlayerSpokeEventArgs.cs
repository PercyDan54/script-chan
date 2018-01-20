using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Ircbot
{
    public class PlayerSpokeEventArgs : EventArgs
    {
        public long MatchId { get; set; }
        public string PlayerName { get; set; }
        public string Message { get; set; }
    }
}
