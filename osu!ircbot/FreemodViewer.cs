using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Osu.Ircbot
{
    /// <summary>
    /// The class used for the results of banchobot prints for mp setting
    /// </summary>
    public class FreemodViewer
    {
        #region Properties
        /// <summary>
        /// The list containing user details in the room (slot, color, mods..)
        /// </summary>
        public List<UserSettings> Players => players;
        /// <summary>
        /// The room string
        /// </summary>
        public string RoomString { get; set; }
        /// <summary>
        /// The map string
        /// </summary>
        public string MapString { get; set; }
        #endregion

        #region Attributes
        /// <summary>
        /// The list containing user details in the room (slot, color, mods..)
        /// </summary>
        public List<UserSettings> players;
        #endregion

        #region Constructors
        public FreemodViewer()
        {
            players = new List<UserSettings>();
        }
        #endregion

        #region Public Methods
        public void AddPlayer(UserSettings us)
        {
            players.Add(us);
        }
        #endregion
    }
}
