using Caliburn.Micro;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class TeamPlayerListItemViewModel : Screen
    {
        #region Constructor
        public TeamPlayerListItemViewModel(Team team, Player player)
        {
            this.team = team;
            this.player = player;
        }
        #endregion

        #region Properties
        private Team team;

        private Player player;

        public string Name
        {
            get { return player.Name; }
        }
        #endregion

        #region Actions
        public void Delete()
        {
            Log.Information("GUI edit team '{team}' remove player '{player}'", team.Name, player.Name);
            team.RemovePlayer(player);
            Events.Aggregator.PublishOnUIThread("RemovePlayerFromTeam");
        }
        #endregion
    }
}
