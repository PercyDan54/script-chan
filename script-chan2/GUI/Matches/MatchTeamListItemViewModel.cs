using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class MatchTeamListItemViewModel : Screen
    {
        public MatchTeamListItemViewModel(Team team)
        {
            Team = team;
        }

        public Team Team;

        public string Name
        {
            get { return Team.Name; }
        }
    }
}
