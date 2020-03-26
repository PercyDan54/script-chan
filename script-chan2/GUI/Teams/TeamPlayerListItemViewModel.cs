using Caliburn.Micro;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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

        private bool hover = false;
        public SolidColorBrush Background
        {
            get
            {
                if (hover)
                    return Brushes.LightGray;
                return Brushes.Transparent;
            }
        }
        #endregion

        #region Actions
        public void Delete()
        {
            Log.Information("TeamPlayerListItemViewModel: edit team '{team}' remove player '{player}'", team.Name, player.Name);
            team.RemovePlayer(player);
            Events.Aggregator.PublishOnUIThread("RemovePlayerFromTeam");
        }

        public void MouseEnter()
        {
            hover = true;
            NotifyOfPropertyChange(() => Background);
        }

        public void MouseLeave()
        {
            hover = false;
            NotifyOfPropertyChange(() => Background);
        }
        #endregion
    }
}
