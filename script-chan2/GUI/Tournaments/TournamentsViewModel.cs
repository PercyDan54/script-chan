using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Linq;

namespace script_chan2.GUI
{
    public class TournamentsViewModel : Screen, IHandle<string>
    {
        private ILogger localLog = Log.ForContext<TournamentsViewModel>();

        #region Tournaments list
        public BindableCollection<TournamentListItemViewModel> TournamentsViews
        {
            get
            {
                var list = new BindableCollection<TournamentListItemViewModel>();
                foreach (var tournament in Database.Database.Tournaments.OrderBy(x => x.Name))
                    list.Add(new TournamentListItemViewModel(tournament));
                return list;
            }
        }
        #endregion

        #region Constructor
        protected override void OnActivate()
        {
            Events.Aggregator.Subscribe(this);
        }
        #endregion Constructor

        #region Events
        public void Handle(string message)
        {
            if (message == "DeleteTournament")
                NotifyOfPropertyChange(() => TournamentsViews);
            else if (message == "AddTournament")
                NotifyOfPropertyChange(() => TournamentsViews);
        }
        #endregion

        #region Actions
        public async void OpenNewTournamentDialog()
        {
            localLog.Information("new tournament dialog open");
            var model = new EditTournamentDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("save new tournament '{tournament}'", model.Name);
                var tournament = new Tournament
                {
                    Name = model.Name,
                    GameMode = model.GameMode,
                    TeamMode = model.TeamMode,
                    WinCondition = model.WinCondition,
                    Acronym = model.Acronym,
                    TeamSize = model.TeamSize,
                    RoomSize = model.RoomSize,
                    PointsForSecondBan = model.PointsForSecondBan,
                    AllPicksFreemod = model.AllPicksFreemod,
                    AllPicksNofail = model.AllPicksNofail,
                    MpTimerCommand = model.MpTimerCommand,
                    MpTimerAfterGame = model.MpTimerAfterGame,
                    MpTimerAfterPick = model.MpTimerAfterPick,
                    WelcomeString = model.WelcomeString,
                    BRInitialLivesAmount = model.BRInitialLivesAmount
                };
                foreach (var point in model.HeadToHeadPoints)
                    tournament.HeadToHeadPoints.Add(point.Key, point.Value);
                tournament.Save();
                Settings.DefaultTournament = tournament;
                NotifyOfPropertyChange(() => TournamentsViews);
            }
        }

        public async void OpenBracketJsonImportDialog()
        {
            localLog.Information("open bracket.json import dialog");
            var model = new TournamentsBracketJsonImportDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            await DialogHost.Show(view, "MainDialogHost");
        }
        #endregion
    }
}
