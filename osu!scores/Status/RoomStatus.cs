using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Scores.Status
{
    /// <summary>
    /// Represents the different status of a room
    /// </summary>
    public enum RoomStatus
    {
        /// <summary>
        /// The match hasn't started yet
        /// </summary>
        NotStarted = 0,

        /// <summary>
        /// The match is in Warmup status
        /// </summary>
        Warmup = 1,

        /// <summary>
        /// The match is in Playing status
        /// </summary>
        Playing = 2,

        /// <summary>
        /// The match is in Tiebreaker status (last map played)
        /// </summary>
        Tiebreaker = 3,

        /// <summary>
        /// The match is over
        /// </summary>
        Finished = 4,
    }
}
