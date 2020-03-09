using Caliburn.Micro;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace script_chan2.GUI
{
    public class MatchViewModel : Screen
    {
        #region Lists
        public BindableCollection<MatchTeamViewModel> TeamsViews
        {
            get
            {
                var list = new BindableCollection<MatchTeamViewModel>();
                if (match.TeamMode == Enums.TeamModes.TeamVS)
                {
                    list.Add(new MatchTeamViewModel(match, Enums.TeamColors.Blue));
                    list.Add(new MatchTeamViewModel(match, Enums.TeamColors.Red));
                }
                return list;
            }
        }
        #endregion

        #region Constructor
        public MatchViewModel(Match match)
        {
            this.match = match;
        }
        #endregion

        #region Events
        protected override void OnDeactivate(bool close)
        {
            Log.Information("GUI close match '{name}'", match.Name);
            MatchList.OpenedMatches.Remove(match);
        }
        #endregion

        #region Properties
        private Match match;

        public string WindowTitle
        {
            get { return match.Name; }
        }

        public Visibility TeamsListIsVisible
        {
            get
            {
                if (match.TeamMode == Enums.TeamModes.TeamVS)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility PlayersListIsVisible
        {
            get
            {
                if (match.TeamMode == Enums.TeamModes.HeadToHead)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }
        #endregion
    }
}
