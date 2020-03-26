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
using System.Windows;
using System.Windows.Input;

namespace script_chan2.GUI
{
    class MatchListItemViewModel : Screen
    {
        #region Constructor
        public MatchListItemViewModel(Match match)
        {
            this.match = match;
        }
        #endregion

        #region Properties
        private Match match;

        public string Name
        {
            get { return match.Name; }
        }
        #endregion

        #region Actions
        public async void Edit()
        {
            Log.Information("MatchListItemViewModel: match '{name}' edit dialog open", match.Name);
            var model = new EditMatchDialogViewModel(match.Id);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view));

            if (result)
            {
                match.Name = model.Name;
                match.Mappool = model.Mappool;
                match.GameMode = model.GameMode;
                match.TeamMode = model.TeamMode;
                match.WinCondition = model.WinCondition;
                match.BO = model.BO;
                match.TeamBlue = model.TeamBlue;
                match.TeamRed = model.TeamRed;
                match.TeamSize = model.TeamSize;
                match.RoomSize = model.RoomSize;
                match.Players.Clear();
                foreach (var player in model.Players)
                {
                    match.Players.Add(player, 0);
                }
                match.Save();
                NotifyOfPropertyChange(() => Name);
                Events.Aggregator.PublishOnUIThread("EditMatch");
            }
        }

        public async void Delete()
        {
            Log.Information("MatchListItemViewModel: match '{name}' delete dialog open", match.Name);
            var model = new DeleteMatchDialogViewModel(match);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view));

            if (result)
            {
                match.Delete();
                Events.Aggregator.PublishOnUIThread("DeleteMatch");
            }
        }

        public void Open()
        {
            if (!MatchList.OpenedMatches.Contains(match))
            {
                Log.Information("MatchListItemViewModel: open match '{name}'", match.Name);
                var windowManager = new WindowManager();
                windowManager.ShowWindow(new MatchViewModel(match));
                MatchList.OpenedMatches.Add(match);
            }
        }
        #endregion
    }
}
