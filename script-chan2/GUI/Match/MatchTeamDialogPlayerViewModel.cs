using Caliburn.Micro;
using script_chan2.DataTypes;
using Serilog;
using System.Windows;

namespace script_chan2.GUI
{
    public class MatchTeamDialogPlayerViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<MatchPickOverviewDialogViewModel>();

        private Match match;
        private Player player;

        public MatchTeamDialogPlayerViewModel(Match match, Player player)
        {
            this.match = match;
            this.player = player;
        }

        public string PlayerName
        {
            get { return player.Name; }
        }

        public Visibility RoomOpenVisible
        {
            get
            {
                if (match.RoomId > 0)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public void SwitchPlayer()
        {
            localLog.Information("Team dialog switch player {player}", player.Name);
            OsuIrc.OsuIrc.SendMessage("BanchoBot", "!mp switch #" + player.Id);
        }
    }
}
