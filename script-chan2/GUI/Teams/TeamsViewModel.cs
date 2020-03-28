using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class TeamsViewModel : Screen, IHandle<string>
    {
        private ILogger localLog = Log.ForContext<TeamsViewModel>();

        #region Teams list
        public BindableCollection<TeamListItemViewModel> TeamViews
        {
            get
            {
                var list = new BindableCollection<TeamListItemViewModel>();
                foreach (var team in Database.Database.Teams.OrderBy(x => x.Name).OrderBy(y => y.Tournament.Name))
                {
                    if (Settings.DefaultTournament != null && team.Tournament != Settings.DefaultTournament)
                        continue;
                    list.Add(new TeamListItemViewModel(team));
                }
                return list;
            }
        }
        #endregion

        #region Constructor
        protected override void OnActivate()
        {
            Events.Aggregator.Subscribe(this);
        }
        #endregion

        #region Events
        public void Handle(string message)
        {
            if (message == "DeleteTeam")
                NotifyOfPropertyChange(() => TeamViews);
            else if (message == "UpdateDefaultTournament")
                NotifyOfPropertyChange(() => FilterTournament);
        }
        #endregion

        #region Filter
        public BindableCollection<Tournament> Tournaments
        {
            get
            {
                var list = new BindableCollection<Tournament>();
                foreach (var tournament in Database.Database.Tournaments.OrderBy(x => x.Name))
                    list.Add(tournament);
                return list;
            }
        }

        public Tournament FilterTournament
        {
            get { return Settings.DefaultTournament; }
            set
            {
                if (value != Settings.DefaultTournament)
                {
                    localLog.Information("set tournament filter");
                    Settings.DefaultTournament = value;
                    NotifyOfPropertyChange(() => FilterTournament);
                    NotifyOfPropertyChange(() => TeamViews);
                }
            }
        }
        #endregion

        #region Actions
        public async void OpenNewTeamDialog()
        {
            localLog.Information("open new team dialog");
            var model = new EditTeamDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view));

            if (result)
            {
                localLog.Information("save new team '{team}'", model.Name);
                var team = new Team()
                {
                    Name = model.Name,
                    Tournament = model.Tournament
                };
                team.Save();
                Settings.DefaultTournament = model.Tournament;
                NotifyOfPropertyChange(() => TeamViews);
            }
        }
        #endregion
    }
}
