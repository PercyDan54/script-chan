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
using System.Windows.Media;

namespace script_chan2.GUI
{
    class MatchListItemViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<MatchListItemViewModel>();

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
        #endregion

        #region Actions
        public async void Edit()
        {
            localLog.Information("match '{name}' edit dialog open", match.Name);
            var model = new EditMatchDialogViewModel(match.Id);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("save match '{match}'", match.Name);
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
            localLog.Information("match '{name}' delete dialog open", match.Name);
            var model = new DeleteMatchDialogViewModel(match);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("delete match '{match}'", match.Name);
                match.Delete();
                Events.Aggregator.PublishOnUIThread("DeleteMatch");
            }
        }

        public void Open()
        {
            if (!MatchList.OpenedMatches.Contains(match))
            {
                localLog.Information("open match '{name}'", match.Name);
                var windowManager = new WindowManager();
                windowManager.ShowWindow(new MatchViewModel(match));
                MatchList.OpenedMatches.Add(match);
            }
        }

        public void MouseEnter()
        {
            hover = true;
            NotifyOfPropertyChange(() => Background);
        }

        public void MouseLeave()
        {
            hover = false;
            NotifyOfPropertyChange(() => Background);
        }
        #endregion
    }
}
