using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Ircbot
{
    /// <summary>
    /// Event object when an event of the room has been catched
    /// </summary>
    public class MultiplayerEventArgs : EventArgs
    {
        /// <summary>
        /// The room/match id
        /// </summary>
        public long MatchId { get; set; }

        /// <summary>
        /// The event received
        /// </summary>
        public MultiEvent Event_Type { get; set; }

        /// <summary>
        /// The map id for a change map event
        /// </summary>
        public long Map_Id { get; set; }
    }
}
