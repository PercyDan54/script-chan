
using System;
namespace Osu.Scores
{
    /// <summary>
    /// Represents the different type of pick you have in a mappool
    /// </summary>
    public enum PickType
    {
        /// <summary>
        /// The No Mod Pick
        /// </summary>
        NoMod = 0,
        
        /// <summary>
        /// The Hidden Mod Pick
        /// </summary>
        Hidden = 1,

        /// <summary>
        /// The HardRock Mod Pick
        /// </summary>
        HardRock = 2,

        /// <summary>
        /// The DoubleTime Mod Pick
        /// </summary>
        DoubleTime = 3,

        /// <summary>
        /// The Free Mod Pick
        /// </summary>
        FreeMod = 4,

        /// <summary>
        /// The TieBreaker Pick
        /// </summary>
        TieBreaker = 5
    }
}
