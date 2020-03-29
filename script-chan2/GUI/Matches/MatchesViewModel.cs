﻿using Caliburn.Micro;
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
    class MatchesViewModel : Screen, IHandle<string>
    {
        private ILogger localLog = Log.ForContext<MatchesViewModel>();

        #region Constructor
        protected override void OnActivate()
        {
            Events.Aggregator.Subscribe(this);
        }
        #endregion

        #region Events
        public void Handle(string message)
        {
            if (message == "DeleteMatch" || message == "EditMatch")
                NotifyOfPropertyChange(() => MatchViews);
            else if (message == "UpdateDefaultTournament")
                NotifyOfPropertyChange(() => FilterTournament);
        }
        #endregion

        #region Properties
        public BindableCollection<MatchListItemViewModel> MatchViews
        {
            get
            {
                var list = new BindableCollection<MatchListItemViewModel>();
                foreach (var match in Database.Database.Matches.OrderBy(x => x.Name))
                {
                    if (Settings.DefaultTournament != null && match.Tournament != Settings.DefaultTournament)
                        continue;
                    if (FilterStatus != null && match.Status != FilterStatus)
                        continue;
                    list.Add(new MatchListItemViewModel(match));
                }
                return list;
            }
        }
        #endregion

        #region Filters
        public BindableCollection<Tournament> Tournaments
        {
            get
            {
                var list = new BindableCollection<Tournament>();
                foreach (var tournament in Database.Database.Tournaments.OrderBy(x => x.Id))
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
                    NotifyOfPropertyChange(() => MatchViews);
                }
            }
        }

        public List<MatchStatus> Statuses
        {
            get { return Enum.GetValues(typeof(MatchStatus)).Cast<MatchStatus>().ToList(); }
        }

        private MatchStatus? filterStatus;
        public MatchStatus? FilterStatus
        {
            get { return filterStatus; }
            set
            {
                if (value != filterStatus)
                {
                    localLog.Information("set status filter");
                    filterStatus = value;
                    NotifyOfPropertyChange(() => FilterStatus);
                    NotifyOfPropertyChange(() => MatchViews);
                }
            }
        }
        #endregion

        #region Actions
        public async void OpenNewMatchDialog()
        {
            localLog.Information("new match dialog open");
            var model = new EditMatchDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("save new match '{match}'", model.Name);
                var match = new Match()
                {
                    Name = model.Name,
                    Tournament = model.Tournament,
                    Mappool = model.Mappool,
                    GameMode = model.GameMode,
                    TeamMode = model.TeamMode,
                    WinCondition = model.WinCondition,
                    TeamBlue = model.TeamBlue,
                    TeamRed = model.TeamRed,
                    TeamSize = model.TeamSize,
                    RoomSize = model.RoomSize,
                    BO = model.BO,
                    MpTimerCommand = model.Tournament.MpTimerCommand,
                    MpTimerAfterGame = model.Tournament.MpTimerAfterGame,
                    MpTimerAfterPick = model.Tournament.MpTimerAfterPick,
                    PointsForSecondBan = model.Tournament.PointsForSecondBan,
                    AllPicksFreemod = model.Tournament.AllPicksFreemod,
                    WarmupMode = true
                };
                foreach (var player in model.Players)
                    match.Players.Add(player, 0);
                match.Save();
                Settings.DefaultBO = model.BO;
                Settings.DefaultTournament = model.Tournament;
                NotifyOfPropertyChange(() => MatchViews);
            }
        }
        #endregion
    }
}
