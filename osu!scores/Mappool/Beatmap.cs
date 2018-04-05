using Osu.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Osu.Scores
{
    /// <summary>
    /// Represents a mappool beatmap
    /// </summary>
    [DataContract]
    public class Beatmap : IComparable<Beatmap>
    {
        #region Attributes
        /// <summary>
        /// The beatmap id
        /// </summary>
        [DataMember]
        protected long id;

        /// <summary>
        /// The osu! beatmap
        /// </summary>
        [DataMember]
        protected OsuBeatmap osu_beatmap;

        /// <summary>
        /// The pick type
        /// </summary>
        [DataMember]
        protected List<PickType> pick_type;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Beatmap()
        {
            osu_beatmap = null;
            pick_type = new List<PickType>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// OsuBeatmap property
        /// </summary>
        public OsuBeatmap OsuBeatmap
        {
            get
            {
                return osu_beatmap;
            }
            set
            {
                osu_beatmap = value;
            }
        }

        /// <summary>
        /// PickType property
        /// </summary>
        public List<PickType> PickType
        {
            get
            {
                return pick_type;
            }
            set
            {
                pick_type = value;
            }
        }

        /// <summary>
        /// PickType property
        /// </summary>
        public long Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a mod to the beatmap
        /// </summary>
        /// <param name="mod">The mod</param>
        public void AddMod(PickType mod)
        {
            if (pick_type.Contains(mod)) return;
            if (mod == Scores.PickType.None || mod == Scores.PickType.Freemod || mod == Scores.PickType.TieBreaker)
                pick_type.Clear();
            else
                pick_type.RemoveAll(x => x == Scores.PickType.None || x == Scores.PickType.Freemod || x == Scores.PickType.TieBreaker);

            pick_type.Add(mod);

            pick_type = pick_type.OrderByDescending(x => (int) x).ToList();

            ModsChanged?.Invoke();
        }

        /// <summary>
        /// Removes a mod from the beatmap
        /// </summary>
        /// <param name="mod">The mod</param>
        public void RemoveMod(PickType mod)
        {
            pick_type.RemoveAll(x => x == mod);
            if (pick_type.Count == 0)
                AddMod(Scores.PickType.None);

            pick_type = pick_type.OrderByDescending(x => (int)x).ToList();

            ModsChanged?.Invoke();
        }

        /// <summary>
        /// Returns the beatmap as a string
        /// </summary>
        /// <returns>the beatmap as a string</returns>
        public override string ToString()
        {
            return OsuBeatmap.Artist + " - " + OsuBeatmap.Title + " [" + OsuBeatmap.Version + "] (by " + OsuBeatmap.Creator + ")";
        }

        // TODO!!!
        /// <summary>
        /// Compares the beatmap to another beatmap
        /// </summary>
        /// <param name="other">the other beatmap</param>
        /// <returns>an integer</returns>
        public int CompareTo(Beatmap other)
        {
            if (pick_type.Contains(Scores.PickType.None))
            {
                if (other.pick_type.Contains(Scores.PickType.None))
                    return 0;
                return -1;
            }

            if (other.pick_type.Contains(Scores.PickType.None))
                return 1;

            if (pick_type.Contains(Scores.PickType.TieBreaker))
            {
                if (other.pick_type.Contains(Scores.PickType.TieBreaker))
                    return 0;
                return 1;
            }

            if (other.pick_type.Contains(Scores.PickType.TieBreaker))
                return -1;

            if (pick_type.Contains(Scores.PickType.Freemod))
            {
                if (other.pick_type.Contains(Scores.PickType.Freemod))
                    return 0;
                return 1;
            }

            if (other.pick_type.Contains(Scores.PickType.Freemod))
                return -1;

            if (pick_type.Count == other.pick_type.Count)
                return pick_type.Max(x => (int) x) - other.pick_type.Max(x => (int) x);

            return pick_type.Count - other.pick_type.Count;
        }
        #endregion

        #region Handlers

        public delegate void ModsChangedHandler();

        public event ModsChangedHandler ModsChanged;

        #endregion
    }
}
