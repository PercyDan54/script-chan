using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class TeamPlayerListItemViewModel : Screen
    {
        public TeamPlayerListItemViewModel(Team team, Player player)
        {
            this.team = team;
            this.player = player;
        }

        private Team team;

        private Player player;

        public string Name
        {
            get { return player.Name; }
        }

        public void Delete()
        {
            team.RemovePlayer(player);
            Events.Aggregator.PublishOnUIThread("RemovePlayerFromTeam");
        }
    }
}
