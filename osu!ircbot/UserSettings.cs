using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Ircbot
{
    /// <summary>
    /// The object used in FreemodViewer which contain informations from a slot with mp settings command
    /// </summary>
    public class UserSettings
    {
        /// <summary>
        /// The slot of the player
        /// </summary>
        public int Slot { get; set; }

        /// <summary>
        /// The status of the player (ready, not ready, no map)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The user id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The team color
        /// </summary>
        public string TeamColor { get; set; }

        /// <summary>
        /// Mods selected by the player
        /// </summary>
        public string ModsSelected { get; set; }
    }
}
