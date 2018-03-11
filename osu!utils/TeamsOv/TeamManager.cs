using Osu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Utils.TeamsOv
{
    public class TeamManager
    {
        private static List<TeamOv> teams;

        public TeamManager()
        {
        }

        #region Static Methods
        /// <summary>
        /// Initializes the teams
        /// </summary>
        public static void Initialize()
        {
            Cache cache = Cache.GetCache("osu!teams.db");
            teams = cache.GetArray<List<TeamOv>>("teams", new List<TeamOv>());

            // Check if I need to check players again or not? probably not useful...

        }

        /// <summary>
        /// Teams property
        /// </summary>
        public static IEnumerable<TeamOv> Teams
        {
            get
            {
                return teams;
            }
        }

        /// <summary>
        /// Returns a mappool from its id
        /// </summary>
        /// <param name="id">the mappool name</param>
        /// <returns>the mappool</returns>
        public static TeamOv Get(string name)
        {
            if (!teams.Exists(x => x.Name == name))
            {
                teams.Add(new TeamOv(name));
            }

            return teams.Find(x => x.Name == name);
        }

        /// <summary>
        /// Removes a mappools by its name
        /// </summary>
        /// <param name="name">the name of the mappool</param>
        public static void Remove(string name)
        {
            if (teams.Exists(x => x.Name == name))
            {
                teams.RemoveAll(x => x.Name == name);
            }
        }

        /// <summary>
        /// Saves the mappools list in the cache
        /// </summary>
        public static void Save()
        {
            Cache cache = Cache.GetCache("osu!teams.db");

            cache["teams"] = teams;
        }
        #endregion
    }
}
