using Osu.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Scores
{
    /// <summary>
    /// Object used for the room configuration in options tab of the room to apply modifications to the osu! room
    /// </summary>
    public class RoomConfiguration
    {
        public OsuTeamType TeamMode { get; set; }
        public OsuScoringType ScoreMode { get; set; }
        public string RoomSize { get; set; }
    }
}
