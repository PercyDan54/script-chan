using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Osu.Ircbot
{
    /// <summary>
    /// Represents an event which is coming from a room
    /// </summary>
    public enum MultiEvent
    {
        /// <summary>
        /// Refreshing a room
        /// </summary>
        EndMap = 0,

        /// <summary>
        /// Change the map
        /// </summary>
        ChangeMap = 1,
    }
}
