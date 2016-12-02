using Caliburn.Micro;
using Osu.Scores;

namespace Osu.Mvvm.Rooms.Ranking.HeadToHead.ViewModels
{
    public class PlayerLineViewModel : PropertyChangedBase
    {
        #region Attributes
        /// <summary>
        /// The player
        /// </summary>
        private Player player;

        /// <summary>
        /// The player score
        /// </summary>
        private int score;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="player">the player</param>
        /// <param name="score">the player score</param>
        public PlayerLineViewModel(Player player, int score)
        {
            this.player = player;
            this.score = score;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Username property
        /// </summary>
        public string Username
        {
            get
            {
                return player.Username;
            }
        }

        /// <summary>
        /// Score property
        /// </summary>
        public string Score
        {
            get
            {
                return score.ToString();
            }
        }
        #endregion
    }
}
