using Osu.Api;

namespace Osu.Scores
{
    /// <summary>
    /// The player
    /// </summary>
    public class Player
    {
        #region Attributes
        /// <summary>
        /// The underlying osu player
        /// </summary>
        protected OsuUser osu_user;

        /// <summary>
        /// If the player is playing
        /// </summary>
        protected bool playing;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Player()
        {
            osu_user = null;
            playing = true;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Id property
        /// </summary>
        public long Id
        {
            get
            {
                return osu_user.Id;
            }
        }

        /// <summary>
        /// Username property
        /// </summary>
        public string Username
        {
            get
            {
                return osu_user.Username;
            }
        }

        /// <summary>
        /// User property
        /// </summary>
        public OsuUser OsuUser
        {
            get
            {
                return osu_user;
            }

            set
            {
                osu_user = value;
            }
        }

        /// <summary>
        /// Playing property
        /// </summary>
        public bool Playing
        {
            get
            {
                return playing;
            }
            set
            {
                playing = value;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the player as a string
        /// </summary>
        /// <returns>a string</returns>
        public override string ToString()
        {
            return osu_user.Username;
        }
        #endregion
    }
}
