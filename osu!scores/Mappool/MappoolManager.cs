using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Osu.Utils;

namespace Osu.Scores
{
    public static class MappoolManager
    {
        #region Attributes
        private static List<Mappool> mappools;
        #endregion

        #region Properties
        public static IEnumerable<Mappool> Mappools
        {
            get
            {
                return mappools;
            }
        }
        #endregion

        #region Constructor
        static MappoolManager()
        {

        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize the mappool list from file
        /// </summary>
        /// <returns></returns>
        public static async Task Initialize()
        {
            Cache cache = Cache.GetCache("osu!mappools.db");

            mappools = cache.GetArray("mappools", new List<Mappool>());

            foreach (Mappool mappool in Mappools)
                await mappool.Update();
        }

        /// <summary>
        /// Returns a mappool from its name
        /// </summary>
        /// <param name="name">the mappool name</param>
        /// <returns>the mappool</returns>
        public static Mappool Get(string name)
        {
            if (!mappools.Exists(x => x.Name == name))
            {
                mappools.Add(new Mappool() { Name = name });
                FireChangeEvent();
            }

            return mappools.Find(x => x.Name == name);
        }

        /// <summary>
        /// Removes a mappool by its name
        /// </summary>
        /// <param name="name">the name of the mappool</param>
        public static void Remove(string name)
        {
            mappools.RemoveAll(x => x.Name == name);
            FireChangeEvent();
        }

        /// <summary>
        /// Saves the mappools list in the cache
        /// </summary>
        public static void Save()
        {
            Cache cache = Cache.GetCache("osu!mappools.db");

            cache["mappools"] = mappools;
        }
        #endregion

        #region Events
        public static event EventHandler<EventArgs> ChangeEvent;

        private static void FireChangeEvent()
        {
            ChangeEvent?.Invoke(typeof(Mappool), new EventArgs());
        }
        #endregion
    }
}
