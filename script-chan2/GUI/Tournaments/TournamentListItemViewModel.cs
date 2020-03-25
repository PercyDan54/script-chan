using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using script_chan2.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class TournamentListItemViewModel : Screen
    {
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
        #endregion

        #region Actions

        public async void Edit()
        {
            Log.Information("TournamentListItemViewModel: tournament '{name}' edit dialog open", tournament.Name);
            var model = new EditTournamentDialogViewModel(tournament.Id);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view));

            if (result)
            {
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
                tournament.Save();
                NotifyOfPropertyChange(() => Name);
            }
        }

        public async void EditWebhooks()
        {
            Log.Information("TournamentListItemViewModel: tournament '{name}' edit webhooks dialog open", tournament.Name);
            var model = new TournamentWebhooksDialogViewModel(tournament);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            await DialogHost.Show(view);
        }

        public async void Delete()
        {
            Log.Information("TournamentListItemViewModel: tournament '{name}' delete dialog open", tournament.Name);
            var model = new DeleteTournamentDialogViewModel(tournament);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view));

            if (result)
            {
                tournament.Delete();
                Events.Aggregator.PublishOnUIThread("DeleteTournament");
            }
        }
        #endregion
    }
}
