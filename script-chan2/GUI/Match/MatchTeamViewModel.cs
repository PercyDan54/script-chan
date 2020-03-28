using Caliburn.Micro;
using script_chan2.DataTypes;
using script_chan2.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class MatchTeamViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<MatchTeamViewModel>();

        #region Constructor
        public MatchTeamViewModel(Match match, TeamColors teamColor)
        {
            this.match = match;
            this.teamColor = teamColor;
        }
        #endregion

        #region Properties
        private Match match;

        private TeamColors teamColor;

        public string Name
        {
            get
            {
                if (teamColor == TeamColors.Blue)
                    return match.TeamBlue.Name;
                return match.TeamRed.Name;
            }
        }

        public int Points
        {
            get
            {
                if (teamColor == TeamColors.Blue)
                    return match.TeamBluePoints;
                return match.TeamRedPoints;
            }
        }

        public Brush FontColor
        {
            get
            {
                if (teamColor == TeamColors.Blue)
                    return Brushes.Blue;
                return Brushes.Red;
            }
        }
        #endregion

        #region Actions
        public void DecreasePoints()
        {
            localLog.Information("match '{match}' decrease points for team '{team}'", match.Name, Name);
            if (teamColor == TeamColors.Blue)
                match.TeamBluePoints--;
            else
                match.TeamRedPoints--;
            match.Save();
            NotifyOfPropertyChange(() => Points);
        }

        public void IncreasePoints()
        {
            localLog.Information("match '{match}' increase points for team '{team}'", match.Name, Name);
            if (teamColor == TeamColors.Blue)
                match.TeamBluePoints++;
            else
                match.TeamRedPoints++;
            match.Save();
            NotifyOfPropertyChange(() => Points);
        }
        #endregion
    }
}