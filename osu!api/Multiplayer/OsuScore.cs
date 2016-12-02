using System;
using System.Runtime.Serialization;

namespace Osu.Api
{
    /// <summary>
    /// Represents an osu! score
    /// </summary>
    [DataContract]
    public class OsuScore
    {
        #region User
        /// <summary>
        /// The slot number
        /// </summary>
        [DataMember(Name = "slot")]
        public readonly int Slot;

        /// <summary>
        /// The user id
        /// </summary>
        [DataMember(Name = "user_id")]
        public readonly int UserId;

        /// <summary>
        /// The team number (0 = no team, 1 = blue, 2 = red)
        /// </summary>
        [DataMember(Name = "team")]
        public readonly OsuTeam Team;
        #endregion

        #region Score
        /// <summary>
        /// The score
        /// </summary>
        [DataMember(Name = "score")]
        public readonly long Score;

        /// <summary>
        /// The max combo
        /// </summary>
        [DataMember(Name = "maxcombo")]
        public readonly int MaxCombo;

        /// <summary>
        /// The count of 50
        /// </summary>
        [DataMember(Name = "count50")]
        public readonly int Count50;

        /// <summary>
        /// The count of 100
        /// </summary>
        [DataMember(Name = "count100")]
        public readonly int Count100;

        /// <summary>
        /// The count of 300
        /// </summary>
        [DataMember(Name = "count300")]
        public readonly int Count300;

        /// <summary>
        /// The count of miss
        /// </summary>
        [DataMember(Name = "countmiss")]
        public readonly int CountMiss;

        /// <summary>
        /// The count of geki
        /// </summary>
        [DataMember(Name = "countgeki")]
        public readonly int CountGeki;

        /// <summary>
        /// The count of katu
        /// </summary>
        [DataMember(Name = "countkatu")]
        public readonly int CountKatu;

        /// <summary>
        /// If the score is a full combo
        /// </summary>
        [DataMember(Name = "perfect")]
        public readonly int Perfect;

        /// <summary>
        /// If the player failed once or not
        /// </summary>
        [DataMember(Name = "pass")]
        public readonly int Pass;
        #endregion

        #region Unused
        /// <summary>
        /// The rank (Not supported yet)
        /// </summary>
        [DataMember(Name = "rank")]
        public readonly string Rank;
        #endregion
    }
}
