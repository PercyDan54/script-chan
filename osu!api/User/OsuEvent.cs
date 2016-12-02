using System.Runtime.Serialization;

namespace Osu.Api
{
    /// <summary>
    /// Represents an osu! player event
    /// </summary>
    [DataContract]
    public class OsuEvent
    {
        /// <summary>
        /// The display html
        /// </summary>
        [DataMember(Name = "display_html")]
        public readonly string DisplayHtml;

        /// <summary>
        /// The beatmap id
        /// </summary>
        [DataMember(Name = "beatmap_id")]
        public readonly long? BeatmapId;

        /// <summary>
        /// The beatmap set id
        /// </summary>
        [DataMember(Name = "beatmapset_id")]
        public readonly long? BeatmapSetId;

        /// <summary>
        /// The date
        /// </summary>
        [DataMember(Name = "date")]
        public readonly string Date;

        /// <summary>
        /// The epic factor (Between 1 and 32)
        /// </summary>
        [DataMember(Name = "epicfactor")]
        public readonly int EpicFactor;
    }
}
