using Osu.Api;
using Osu.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Osu.Scores
{
    /// <summary>
    /// The mappool
    /// </summary>
    public class Mappool
    {
        #region Constants
        /// <summary>
        /// The dictionary of mappools
        /// </summary>
        private static Dictionary<string, Mappool> mappools;
        #endregion

        #region Attributes
        /// <summary>
        /// The mappool name
        /// </summary>
        protected string name;

        /// <summary>
        /// The Dictionary of maps with the right mod for the mappool
        /// </summary>
        protected Dictionary<long, Beatmap> pool;
        #endregion

        #region Events
        public static event EventHandler<EventArgs> ChangeEvent;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Mappool()
        {
            name = "NoName";
            pool = new Dictionary<long, Beatmap>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Name property
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Pool property
        /// </summary>
        public Dictionary<long, Beatmap> Pool
        {
            get
            {
                return pool;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the mappool using the set api
        /// </summary>
        /// <returns>a task</returns>
        public async Task Update()
        {
            List<long> bmToRemove = new List<long>();

            // For each pair in our pool
            foreach (KeyValuePair<long, Beatmap> pair in pool)
            {
                // Get the osu!beatmap from the api
                OsuBeatmap osu_beatmap = await OsuApi.GetBeatmap(pair.Key, false);

                // If the beatmap doesn't exist
                if (osu_beatmap == null)
                    // Remove this beatmap from the pool (Since there's some kind of error)
                    bmToRemove.Add(pair.Key);
                // Else
                else
                    // Change the osu!beatmap linked in the beatmap object
                    pair.Value.OsuBeatmap = osu_beatmap;
            }

            foreach(var bm in bmToRemove)
            {
                pool.Remove(bm);
            }
        }

        /// <summary>
        /// Converts the mappool as a string
        /// </summary>
        /// <returns>a string</returns>
        public override string ToString()
        {
            return name;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Initializes the mappools
        /// </summary>
        public static async Task Initialize()
        {
            Cache cache = Cache.GetCache("osu!cache.db");

            mappools = cache.GetObject<Dictionary<string, Mappool>>("mappools", new Dictionary<string, Mappool>());

            foreach (Mappool mappool in Mappools)
                await mappool.Update();
        }

        /// <summary>
        /// Mappools property
        /// </summary>
        public static IEnumerable<Mappool> Mappools
        {
            get
            {
                return mappools.Values.ToList();
            }
        }

        /// <summary>
        /// Returns a mappool from its id
        /// </summary>
        /// <param name="id">the mappool name</param>
        /// <returns>the mappool</returns>
        public static Mappool Get(string name)
        {
            if (!mappools.ContainsKey(name))
            {
                mappools[name] = new Mappool();
                FireChangeEvent();
            }

            return mappools[name];
        }

        /// <summary>
        /// Removes a mappools by its name
        /// </summary>
        /// <param name="name">the name of the mappool</param>
        public static void Remove(string name)
        {
            if (mappools.ContainsKey(name))
            {
                mappools.Remove(name);
                FireChangeEvent();
            }


        }

        private static void FireChangeEvent()
        {
            if (ChangeEvent != null)
            {
                ChangeEvent(typeof(Mappool), new EventArgs());
            }
        }

        /// <summary>
        /// Saves the mappools list in the cache
        /// </summary>
        public static void Save()
        {
            Cache cache = Cache.GetCache("osu!cache.db");

            cache["mappools"] = mappools;
        }
        #endregion
    }
}
