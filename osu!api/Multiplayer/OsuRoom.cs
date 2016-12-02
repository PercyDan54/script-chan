using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Osu.Api
{
    /// <summary>
    /// Represents an osu! multiplayer room
    /// </summary>
    [DataContract]
    public class OsuRoom
    {
        #region Match
        /// <summary>
        /// The match informations
        /// </summary>
        [DataMember(Name = "match")]
        public readonly OsuMatch Match;
        #endregion

        #region Games
        /// <summary>
        /// The list of games played
        /// </summary>
        [DataMember(Name = "games")]
        public readonly List<OsuGame> Games;
        #endregion
    }
}
