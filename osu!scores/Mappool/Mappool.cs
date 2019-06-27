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
        #region Attributes
        /// <summary>
        /// The mappool name
        /// </summary>
        protected string name;

        /// <summary>
        /// The Dictionary of maps with the right mod for the mappool
        /// </summary>
        protected List<Beatmap> pool;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Mappool()
        {
            name = "NoName";
            pool = new List<Beatmap>();
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
        public List<Beatmap> Pool
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
            List<Beatmap> bmToRemove = new List<Beatmap>();

            // For each pair in our pool
            foreach (Beatmap beatmap in pool)
            {
                // Get the osu!beatmap from the api
                OsuBeatmap osu_beatmap = await OsuApi.GetBeatmap(beatmap.Id, false);

                // If the beatmap doesn't exist
                if (osu_beatmap == null)
                    // Remove this beatmap from the pool (Since there's some kind of error)
                    bmToRemove.Add(beatmap);
                // Else
                else
                    // Change the osu!beatmap linked in the beatmap object
                    beatmap.OsuBeatmap = osu_beatmap;
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
    }
}
