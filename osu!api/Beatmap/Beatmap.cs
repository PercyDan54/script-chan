using System.Runtime.Serialization;

namespace Osu.Api
{
    /// <summary>
    /// Represents an osu! beatmap
    /// </summary>
    [DataContract]
    public class OsuBeatmap
    {
        #region Metadata
        /// <summary>
        /// The beatmap id
        /// </summary>
        [DataMember(Name = "beatmap_id")]
        public readonly long BeatmapID;

        /// <summary>
        /// The beatmap set id
        /// </summary>
        [DataMember(Name = "beatmapset_id")]
        public readonly long BeatmapSetID;

        /// <summary>
        /// The artist
        /// </summary>
        [DataMember(Name = "artist")]
        public readonly string Artist;

        /// <summary>
        /// The title
        /// </summary>
        [DataMember(Name = "title")]
        public readonly string Title;

        /// <summary>
        /// The source
        /// </summary>
        [DataMember(Name = "source")]
        public readonly string Source;

        /// <summary>
        /// The creator
        /// </summary>
        [DataMember(Name = "creator")]
        public readonly string Creator;

        /// <summary>
        /// The version
        /// </summary>
        [DataMember(Name = "version")]
        public readonly string Version;

        /// <summary>
        /// The bpm
        /// </summary>
        [DataMember(Name = "bpm")]
        public readonly float BPM;
        #endregion

        #region Ranking
        /// <summary>
        /// The beatmap ranking status
        /// </summary>
        [DataMember(Name = "approved")]
        public readonly OsuApprovalStatus RankingStatus;

        /// <summary>
        /// The approval date (If any)
        /// </summary>
        [DataMember(Name = "approved_date")]
        public readonly string ApprovedDate;

        /// <summary>
        /// The last update date
        /// </summary>
        [DataMember(Name = "last_update")]
        public readonly string LastUpdate;
        #endregion

        #region Difficulty
        /// <summary>
        /// The difficulty star rating
        /// </summary>
        [DataMember(Name = "difficultyrating")]
        public readonly float DifficultyRating;

        /// <summary>
        /// The circle size
        /// </summary>
        [DataMember(Name = "diff_size")]
        public readonly float CircleSize;

        /// <summary>
        /// The overall difficulty
        /// </summary>
        [DataMember(Name = "diff_overall")]
        public readonly float OverallDifficulty;

        /// <summary>
        /// The approach rate
        /// </summary>
        [DataMember(Name = "diff_approach")]
        public readonly float ApproachRate;

        /// <summary>
        /// The hp drain
        /// </summary>
        [DataMember(Name = "diff_drain")]
        public readonly float HealthDrain;

        /// <summary>
        /// The hit length (Time between first note and last note, without breaks)
        /// </summary>
        [DataMember(Name = "hit_length")]
        public readonly int HitLength;

        /// <summary>
        /// The total length (Time between first note and last note, with breaks)
        /// </summary>
        [DataMember(Name = "total_length")]
        public readonly int TotalLength;

        /// <summary>
        /// The osu! mode
        /// </summary>
        [DataMember(Name = "mode")]
        public readonly OsuMode Mode;
        #endregion
    }
}
