using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class TournamentListItemViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<TournamentListItemViewModel>();

        #region Constructor
        public TournamentListItemViewModel(Tournament tournament)
        {
            this.tournament = tournament;
        }
        #endregion

        #region Properties
        private Tournament tournament;

        public string Name
        {
            get { return tournament.Name; }
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
            localLog.Information("tournament '{name}' edit dialog open", tournament.Name);
            var model = new EditTournamentDialogViewModel(tournament.Id);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("save tournament '{name}'", tournament.Name);
                tournament.Name = model.Name;
                tournament.GameMode = model.GameMode;
                tournament.TeamMode = model.TeamMode;
                tournament.WinCondition = model.WinCondition;
                tournament.Acronym = model.Acronym;
                tournament.TeamSize = model.TeamSize;
                tournament.RoomSize = model.RoomSize;
                tournament.PointsForSecondBan = model.PointsForSecondBan;
                tournament.AllPicksFreemod = model.AllPicksFreemod;
                tournament.MpTimerCommand = model.MpTimerCommand;
                tournament.MpTimerAfterGame = model.MpTimerAfterGame;
                tournament.MpTimerAfterPick = model.MpTimerAfterPick;
                tournament.WelcomeString = model.WelcomeString;
                tournament.BRInitialLivesAmount = model.BRInitialLivesAmount;
                tournament.HeadToHeadPoints.Clear();
                foreach (var point in model.HeadToHeadPoints)
                    tournament.HeadToHeadPoints.Add(point.Key, point.Value);
                tournament.Save();
                NotifyOfPropertyChange(() => Name);
            }
        }

        public async void EditWebhooks()
        {
            localLog.Information("tournament '{name}' edit webhooks dialog open", tournament.Name);
            var model = new TournamentWebhooksDialogViewModel(tournament);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            await DialogHost.Show(view, "MainDialogHost");
        }

        public async void Delete()
        {
            localLog.Information("tournament '{name}' delete dialog open", tournament.Name);
            var model = new DeleteTournamentDialogViewModel(tournament);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("delete tournament '{tournament}'", tournament.Name);
                tournament.Delete();
                Events.Aggregator.PublishOnUIThread("DeleteTournament");
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
