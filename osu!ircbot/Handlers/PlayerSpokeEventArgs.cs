using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Ircbot
{
    /// <summary>
    /// Event object when a player spoke in the channel
    /// </summary>
    public class PlayerSpokeEventArgs : EventArgs
    {
        /// <summary>
        /// The match id
        /// </summary>
        public long MatchId { get; set; }

        /// <summary>
        /// The player username
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// The message sent by the player
        /// </summary>
        public string Message { get; set; }
    }
}
