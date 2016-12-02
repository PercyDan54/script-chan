using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Osu.Api
{
    /// <summary>
    /// Represents an osu! game
    /// </summary>
    [DataContract]
    public class OsuGame
    {
        #region Game
        /// <summary>
        /// The game id
        /// </summary>
        [DataMember(Name = "game_id")]
        public readonly int GameId;

        /// <summary>
        /// The played beatmap id
        /// </summary>
        [DataMember(Name = "beatmap_id")]
        public readonly int BeatmapId;
        #endregion

        #region Time
        /// <summary>
        /// The game start time
        /// </summary>
        [DataMember(Name = "start_time")]
        public readonly string StartTime;

        /// <summary>
        /// The game end time
        /// </summary>
        [DataMember(Name = "end_time")]
        public readonly string EndTime;
        #endregion

        #region Settings
        /// <summary>
        /// The played mode
        /// </summary>
        [DataMember(Name = "play_mode")]
        public readonly OsuMode PlayMode;

        /// <summary>
        /// The scoring type
        /// </summary>
        [DataMember(Name = "scoring_type")]
        public readonly OsuScoringType ScoringType;

        /// <summary>
        /// The team type
        /// </summary>
        [DataMember(Name = "team_type")]
        public readonly OsuTeamType TeamType;

        /// <summary>
        /// The global mods
        /// </summary>
        [DataMember(Name = "mods")]
        public readonly OsuMods Mods;
        #endregion

        #region Scores
        /// <summary>
        /// The list of scores
        /// </summary>
        [DataMember(Name = "scores")]
        public readonly List<OsuScore> Scores;
        #endregion

        #region Unused
        /// <summary>
        /// The match type (???)
        /// </summary>
        [DataMember(Name = "match_type")]
        public readonly string MatchType;
        #endregion
    }
}
