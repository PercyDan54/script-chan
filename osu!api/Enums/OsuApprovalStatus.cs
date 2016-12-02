
namespace Osu.Api
{
    /// <summary>
    /// Represents the osu! approval status
    /// </summary>
    public enum OsuApprovalStatus
    {
        /// <summary>
        /// Graveyard
        /// </summary>
        Graveyard = -2,

        /// <summary>
        /// Work in progress
        /// </summary>
        WIP = -1,

        /// <summary>
        /// Pending
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Ranked
        /// </summary>
        Ranked = 1,

        /// <summary>
        /// Approved
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Qualified
        /// </summary>
        Qualified = 3
    }
}
