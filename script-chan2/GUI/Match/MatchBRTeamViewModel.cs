using Caliburn.Micro;
using script_chan2.DataTypes;
using Serilog;
using System.Windows;

namespace script_chan2.GUI
{
    public class MatchBRTeamViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<MatchTeamViewModel>();

        #region Constructor
        public MatchBRTeamViewModel(Match match, Team team)
        {
            this.match = match;
            Team = team;
        }
        #endregion

        #region Properties
        private Match match;

        public Team Team;

        public string Teamname
        {
            get
            {
                return Team.Name;
            }
        }

        public int Lives
        {
            get
            {
                return match.TeamsBR[Team];
            }
        }

        public Visibility KickVisible
        {
            get
            {
                if (Lives <= 0)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }
        #endregion

        #region Actions
        public void DecreaseLives()
        {
            localLog.Information("match '{match}' decrease lives for team '{team}'", match.Name, Teamname);
            match.TeamsBR[Team]--;
            match.Save();
            NotifyOfPropertyChange(() => Lives);
            NotifyOfPropertyChange(() => KickVisible);
        }

        public void IncreaseLives()
        {
            localLog.Information("match '{match}' increase lives for team '{team}'", match.Name, Teamname);
            match.TeamsBR[Team]++;
            match.Save();
            NotifyOfPropertyChange(() => Lives);
            NotifyOfPropertyChange(() => KickVisible);
        }
        #endregion
    }
}
