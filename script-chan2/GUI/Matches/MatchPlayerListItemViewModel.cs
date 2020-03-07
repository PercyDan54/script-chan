using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class MatchPlayerListItemViewModel : Screen
    {
        #region Constructor
        public MatchPlayerListItemViewModel(Player player)
        {
            Player = player;
        }
        #endregion

        #region Properties
        public Player Player;

        public string Name
        {
            get { return $"{Player.Name} ({Player.Country})"; }
        }
        #endregion
    }
}
