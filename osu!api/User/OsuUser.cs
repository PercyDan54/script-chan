using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Osu.Api
{
    /// <summary>
    /// Represents an osu! user
    /// </summary>
    [DataContract]
    public class OsuUser
    {
        #region User
        /// <summary>
        /// The user id
        /// </summary>
        [DataMember(Name = "user_id")]
        public readonly long Id;

        /// <summary>
        /// The user name
        /// </summary>
        [DataMember(Name = "username")]
        public readonly string Username;

        /// <summary>
        /// The country (ISO-3166-1 alpha-2)
        /// </summary>
        [DataMember(Name = "country")]
        public readonly string Country;
        #endregion

        #region Scores
        /// <summary>
        /// The count of 300 made by this user
        /// </summary>
        [DataMember(Name = "count300")]
        public readonly long Count300;

        /// <summary>
        /// The count of 100 made by this user
        /// </summary>
        [DataMember(Name = "count100")]
        public readonly long Count100;

        /// <summary>
        /// The count of 50 made by this user
        /// </summary>
        [DataMember(Name = "count50")]
        public readonly long Count50;

        /// <summary>
        /// The playcount
        /// </summary>
        [DataMember(Name = "playcount")]
        public readonly long Playcount;

        /// <summary>
        /// The ranked score
        /// </summary>
        [DataMember(Name = "ranked_score")]
        public readonly long RankedScore;

        /// <summary>
        /// The total score
        /// </summary>
        [DataMember(Name = "total_score")]
        public readonly long TotalScore;

        /// <summary>
        /// The count of SS
        /// </summary>
        [DataMember(Name = "count_rank_ss")]
        public readonly long CountRankSS;

        /// <summary>
        /// The count of S
        /// </summary>
        [DataMember(Name = "count_rank_s")]
        public readonly long CountRankS;

        /// <summary>
        /// The count of A
        /// </summary>
        [DataMember(Name = "count_rank_a")]
        public readonly long CountRankA;
        #endregion

        #region Rank
        /// <summary>
        /// The performance rank
        /// </summary>
        [DataMember(Name = "pp_rank")]
        public readonly double Rank;

        /// <summary>
        /// The level
        /// </summary>
        [DataMember(Name = "level")]
        public readonly double Level;

        /// <summary>
        /// The raw pp value
        /// </summary>
        [DataMember(Name = "pp_raw")]
        public readonly double PpRaw;

        /// <summary>
        /// The accuracy
        /// </summary>
        [DataMember(Name = "accuracy")]
        public readonly double Accuracy;
        #endregion

        #region Events
        /// <summary>
        /// The user events
        /// </summary>
        [DataMember(Name = "events")]
        public readonly List<OsuEvent> Events;
        #endregion
    }
}
