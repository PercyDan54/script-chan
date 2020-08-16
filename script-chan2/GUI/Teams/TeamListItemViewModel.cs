using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class TeamListItemViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<TeamListItemViewModel>();

        #region Constructor
        public TeamListItemViewModel(Team team)
        {
            this.team = team;
        }
        #endregion

        #region Properties
        private Team team;

        public string Name
        {
            get { return $"{team.Name} ({team.Tournament.Name})"; }
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

        public SolidColorBrush Foreground
        {
            get
            {
                if (hover)
                    return Brushes.Black;
                return Brushes.White;
            }
        }
        #endregion

        #region Actions
        public async void Edit()
        {
            localLog.Information("team '{name}' edit dialog open", team.Name);
            var model = new EditTeamDialogViewModel(team.Id);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("save team '{team}'", team.Name);
                team.Name = model.Name;
                team.Save();
                NotifyOfPropertyChange(() => Name);
            }
        }

        public async void EditPlayers()
        {
            localLog.Information("player list dialog of team '{team}' open", team.Name);
            var model = new TeamPlayersDialogViewModel(team);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            model.Activate();
            await DialogHost.Show(view, "MainDialogHost");
            model.Deactivate();
        }

        public async void Delete()
        {
            localLog.Information("team '{name}' delete dialog open", team.Name);
            var model = new DeleteTeamDialogViewModel(team);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("delete team '{team}'", team.Name);
                team.Delete();
                Events.Aggregator.PublishOnUIThread("DeleteTeam");
            }
        }

        public void MouseEnter()
        {
            hover = true;
            NotifyOfPropertyChange(() => Background);
            NotifyOfPropertyChange(() => Foreground);
        }

        public void MouseLeave()
        {
            hover = false;
            NotifyOfPropertyChange(() => Background);
            NotifyOfPropertyChange(() => Foreground);
        }
        #endregion
    }
}
