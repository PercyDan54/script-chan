using Caliburn.Micro;
using Osu.Scores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Mvvm.Rooms.Irc.ViewModels
{
    public class IrcPlayerViewModel : Screen
    {
        #region Attributes
        /// <summary>
        /// The name of the user
        /// </summary>
        private Player player;

        /// <summary>
        /// The boolean for the checkbox
        /// </summary>
        private bool shouldSend;

        /// <summary>
        /// The room
        /// </summary>
        private Room room;
        #endregion

        #region Constructor
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="player">the player</param>
        /// <param name="room">the room</param>
        public IrcPlayerViewModel(Player player, Room room)
        {
            this.player = player;
            this.room = room;
            this.shouldSend = room.IrcTargets.Contains(player.OsuUser.Username);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Player property
        /// </summary>
        public bool Player
        {
            get
            {
                return shouldSend;
            }
            set
            {
                if (value != shouldSend)
                {
                    shouldSend = value;

                    if (shouldSend)
                        room.IrcTargets.Add(player.OsuUser.Username);
                    else
                        room.IrcTargets.Remove(player.OsuUser.Username);

                    NotifyOfPropertyChange(() => Player);
                }
            }
        }

        /// <summary>
        /// Username property
        /// </summary>
        public string Userame
        {
            get
            {
                return player.OsuUser.Username;
            }
        }
        #endregion
    }
}
