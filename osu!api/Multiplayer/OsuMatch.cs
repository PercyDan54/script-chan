using System.Runtime.Serialization;

namespace Osu.Api
{
    /// <summary>
    /// Represents an osu! match informations
    /// </summary>
    [DataContract]
    public class OsuMatch
    {
        #region Match
        /// <summary>
        /// The match id
        /// </summary>
        [DataMember(Name = "match_id")]
        public readonly int MatchId;

        /// <summary>
        /// The match name
        /// </summary>
        [DataMember(Name = "name")]
        public readonly string Name;
        #endregion

        #region Time
        /// <summary>
        /// The start time
        /// </summary>
        [DataMember(Name = "start_time")]
        public readonly string StartTime;

        /// <summary>
        /// The end time (Always null)
        /// </summary>
        [DataMember(Name = "end_time")]
        public readonly string EndTime;
        #endregion
    }
}
