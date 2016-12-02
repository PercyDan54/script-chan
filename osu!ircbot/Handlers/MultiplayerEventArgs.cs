using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Ircbot
{
    public class MultiplayerEventArgs : EventArgs
    {
        public long MatchId { get; set; }
        public MultiEvent Event_Type { get; set; }
        public long Map_Id { get; set; }
    }
}
