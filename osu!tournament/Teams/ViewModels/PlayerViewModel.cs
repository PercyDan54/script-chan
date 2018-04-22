using Caliburn.Micro;
using Osu.Utils.TeamsOv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Mvvm.Teams.ViewModels
{
    /// <summary>
    /// The player view model
    /// </summary>
    public class PlayerViewModel : Screen
    {
        #region Attributes
        /// <summary>
        /// The player overview
        /// </summary>
        public PlayerOv Player { get; set; }

        /// <summary>
        /// The parent of the playerviewmodel
        /// </summary>
        protected TeamViewModel parent;
        #endregion

        #region Constructors
        public PlayerViewModel(TeamViewModel parent, PlayerOv player)
        {
            this.parent = parent;
            this.Player = player;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Name displayed on the UI
        /// </summary>
        public override string DisplayName
        {
            get
            {
                // Double underscore to escape the character
                return Player.Name.Replace("_", "__") + " [" + Player.Country + "]";
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Delete the player from the team
        /// </summary>
        public void Delete()
        {
            parent.Delete(this);
        }
        #endregion
    }
}
