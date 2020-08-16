using Caliburn.Micro;
using script_chan2.DataTypes;
using Serilog;

namespace script_chan2.GUI
{
    public class MatchPlayerViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<MatchPlayerViewModel>();

        #region Constructor
        public MatchPlayerViewModel(Match match, Player player)
        {
            this.match = match;
            this.player = player;
        }
        #endregion

        #region Properties
        private Match match;

        private Player player;

        public string Username
        {
            get { return player.Name; }
        }

        public int Points
        {
            get { return match.Players[player]; }
        }
        #endregion

        #region Actions
        public void DecreasePoints()
        {
            localLog.Information("match '{match}' decrease points for player '{player}'", match.Name, Username);
            match.Players[player]--;
            match.Save();
            NotifyOfPropertyChange(() => Points);
        }

        public void IncreasePoints()
        {
            localLog.Information("match '{match}' increase points for player '{player}'", match.Name, Username);
            match.Players[player]++;
            match.Save();
            NotifyOfPropertyChange(() => Points);
        }
        #endregion
    }
}
