using Osu.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Scores
{
    public class RoomConfiguration
    {
        public OsuTeamType TeamMode { get; set; }
        public OsuScoringType ScoreMode { get; set; }
        public string RoomSize { get; set; }
    }
}
