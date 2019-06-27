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
        /// Returns a team from its name
        /// </summary>
        /// <param name="name">the mappool name</param>
        /// <returns>the team</returns>
        public static TeamOv Get(string name)
        {
            if (!teams.Exists(x => x.Name == name))
            {
                teams.Add(new TeamOv(name));
            }

            return teams.Find(x => x.Name == name);
        }

        /// <summary>
        /// Renames a team by its name
        /// </summary>
        /// <param name="oldName">The current name of the team</param>
        /// <param name="newName">The new name</param>
        /// <returns></returns>
        public static bool Rename(string oldName, string newName)
        {
            if (teams.Exists(x => x.Name == newName))
                return false;

            var team = teams.Find(x => x.Name == oldName);
            if (team == null)
                return false;

            team.Name = newName;
            return true;
        }

        /// <summary>
        /// Removes a team by its name
        /// </summary>
        /// <param name="name">the name of the team</param>
        public static void Remove(string name)
        {
            if (teams.Exists(x => x.Name == name))
            {
                teams.RemoveAll(x => x.Name == name);
            }
        }

        /// <summary>
        /// Saves the team list in the cache
        /// </summary>
        public static void Save()
        {
            Cache cache = Cache.GetCache("osu!teams.db");

            cache["teams"] = teams;
        }
        #endregion
    }
}
