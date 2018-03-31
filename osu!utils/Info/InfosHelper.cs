using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Utils.Info
{
    /// <summary>
    /// Class containing informations from the cache about the tournament and user data
    /// </summary>
    public static class InfosHelper
    {
        public static TourneyInfo TourneyInfos { get; set; }
        public static UserDataInfo UserDataInfos { get; set; }
    }
}
