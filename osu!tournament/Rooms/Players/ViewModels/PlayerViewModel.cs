using Caliburn.Micro;
using Osu.Scores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Mvvm.Rooms.Players.ViewModels
{
    public class PlayerViewModel : Screen
    {
        /// <summary>
        /// The player
        /// </summary>
        protected Player player;

        public PlayerViewModel(Player player)
        {
            this.player = player;
        }

        public bool PlayerCheckBox
        {
            get
            {
                return player.Playing;
            }
            set
            {
                if (value != player.Playing)
                {
                    player.Playing = value;
                    NotifyOfPropertyChange("PlayerCheckBox");
                }
            }
        }

        public string DisplayUserName
        {
            get
            {
                return player.OsuUser.Username;
            }
            set
            {

            }
        }
    }
}
