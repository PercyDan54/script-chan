using Osu.Api;
using System;
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
        protected PickType pick_type;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Beatmap()
        {
            osu_beatmap = null;
            pick_type = PickType.NoMod;
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
        public PickType PickType
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
        /// Returns the beatmap as a string
        /// </summary>
        /// <returns>the beatmap as a string</returns>
        public override string ToString()
        {
            return OsuBeatmap.Artist + " - " + OsuBeatmap.Title + " [" + OsuBeatmap.Version + "] (by " + OsuBeatmap.Creator + ")";
        }

        /// <summary>
        /// Compares the beatmap to another beatmap
        /// </summary>
        /// <param name="other">the other beatmap</param>
        /// <returns>an integer</returns>
        public int CompareTo(Beatmap other)
        {
            return pick_type.CompareTo(other.PickType);
        }
        #endregion
    }
}
