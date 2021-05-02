using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;

namespace script_chan2.GUI
{
    public class MatchTeamDialogViewModel : Screen
    {
        private Match match;
        private Team team;

        public MatchTeamDialogViewModel(Match match, Team team)
        {
            this.match = match;
            this.team = team;
        }

        public string TeamName
        {
            get { return team.Name; }
        }

        public BindableCollection<MatchTeamDialogPlayerViewModel> PlayersViews
        {
            get
            {
                var list = new BindableCollection<MatchTeamDialogPlayerViewModel>();
                foreach (var player in team.Players)
                {
                    list.Add(new MatchTeamDialogPlayerViewModel(match, player));
                }
                return list;
            }
        }

        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(true, null);
        }
    }
}
