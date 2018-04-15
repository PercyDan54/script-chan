
using System;
namespace Osu.Scores
{
    /// <summary>
    /// Represents the different type of pick you have in a mappool
    /// The value is used for sorting in mappools
    /// </summary>
    public enum PickType
    {
        None = 0,
        HD = 1,
        HR = 2,
        DT = 3,
        FL = 4,
        Freemod = 5,
        TieBreaker = 6
    }
}
