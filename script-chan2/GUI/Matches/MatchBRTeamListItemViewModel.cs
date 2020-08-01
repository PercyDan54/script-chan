using Caliburn.Micro;
using script_chan2.DataTypes;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class MatchBRTeamListItemViewModel : Screen
    {
        #region Constructor
        public MatchBRTeamListItemViewModel(Team team)
        {
            Team = team;
        }
        #endregion

        #region Properties
        public Team Team;

        public string TeamName
        {
            get { return Team.Name; }
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
