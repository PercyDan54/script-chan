using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Utils.Bans
{
    public class ObsBanHelper
    {
        /// <summary>
        /// The cache filename
        /// </summary>
        private const string CacheFilename = "osu!obsban.db";
        private const string CountriesFolder = "countries";
        private const string MapsFolder = "maps";

        /// <summary>
        /// The cache
        /// </summary>
        protected static Cache cache;

        protected static string path;
        protected static bool valid;

        protected static ObsBanHelper instance;

        protected string bluename;
        protected string redname;
        protected int numberOfBans;

        public ObsBanHelper(string blueteam, string redteam)
        {
            bluename = blueteam;
            redname = redteam;
        }

        public static string Path
        {
            get
            {
                return path;
            }
            set
            {
                if(value != path)
                {
                    path = value;
                    valid = false;
                    cache["path"] = path;
                }
            }
        }

        public static bool IsValid
        {
            get
            {
                return valid;
            }
        }

        /// <summary>
        /// Initializes the osu!obsban
        /// </summary>
        public static void Initialize()
        {
            // Create cache
            cache = Cache.GetCache(CacheFilename);

            // Get key
            path = cache.Get("path", "");

            // Set valid to false
            valid = false;

            // Set initialized to true
            //initialized = true;
        }

        public static void CheckPath()
        {
            if (string.IsNullOrEmpty(Path))
            {
                valid = false;
            }
            else
            {
                try
                {
                    IEnumerable<string> files = Directory.EnumerateFiles(Path);
                    IEnumerable<string> directories = Directory.EnumerateDirectories(Path);

                    var exist = files.FirstOrDefault(x => x.Contains("empty")) != null
                        && directories.FirstOrDefault(x => x.Contains(CountriesFolder)) != null
                        && directories.FirstOrDefault(x => x.Contains(MapsFolder)) != null;

                    if (!exist)
                    {
                        valid = false;
                    }
                    else
                    {
                        valid = true;
                    }
                }
                catch(Exception e)
                {
                    valid = false;
                }
            }
        }

        public void SetTeamsOnObs()
        {
            var countries = Directory.GetFiles(Path + "\\" + CountriesFolder);
            var blueteam = countries.FirstOrDefault(x => x.Contains(bluename));
            var redteam = countries.FirstOrDefault(x => x.Contains(redname));

            if(blueteam != null)
            {
                File.Copy(blueteam, Path + "\\FA.png", true);
            }

            if(redteam != null)
            {
                File.Copy(redteam, Path + "\\FB.png", true);
            }
        }

        public bool SetBannedMap(string teamname, string mapsetid, int count)
        {
            var mapsfolder = Directory.GetFiles(Path + "\\" + MapsFolder);
            var beatmap = mapsfolder.FirstOrDefault(x => x.Contains(mapsetid));

            if(beatmap != null)
            {
                string resultname;
                if(teamname == bluename)
                {
                    resultname = "A" + count + ".png";
                }
                else if(teamname == redname)
                {
                    resultname = "B" + count + ".png";
                }
                else
                {
                    return false;
                }

                File.Copy(beatmap, Path + "\\" + resultname, true);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static ObsBanHelper GetInstance()
        {
            return instance;
        }

        public static void SetInstance(string b, string r)
        {
            instance = new ObsBanHelper(b, r);
        }
    }
}
