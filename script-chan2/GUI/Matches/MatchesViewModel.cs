using Caliburn.Micro;
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
        #region Match list
        public BindableCollection<MatchListItemViewModel> MatchesViews
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

        #region Constructor
        protected override void OnActivate()
        {
            newMatchPlayers = new List<Player>();
            Events.Aggregator.Subscribe(this);
        }
        #endregion

        #region Events
        public void Handle(string message)
        {
            if (message == "DeleteMatch" || message == "EditMatch")
                NotifyOfPropertyChange(() => MatchesViews);
            else if (message == "UpdateDefaultTournament")
                NotifyOfPropertyChange(() => FilterTournament);
        }
        #endregion

        #region Filters
        public Tournament FilterTournament
        {
            get { return Settings.DefaultTournament; }
            set
            {
                if (value != Settings.DefaultTournament)
                {
                    Log.Information("GUI match list set tournament filter");
                    Settings.DefaultTournament = value;
                    NotifyOfPropertyChange(() => FilterTournament);
                    NotifyOfPropertyChange(() => MatchesViews);
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
                    Log.Information("GUI match list set status filter");
                    filterStatus = value;
                    NotifyOfPropertyChange(() => FilterStatus);
                    NotifyOfPropertyChange(() => MatchesViews);
                }
            }
        }
        #endregion

        #region New match dialog
        private string newMatchName;
        public string NewMatchName
        {
            get { return newMatchName; }
            set
            {
                if (value != newMatchName)
                {
                    newMatchName = value;
                    NotifyOfPropertyChange(() => NewMatchName);
                    NotifyOfPropertyChange(() => NewMatchSaveEnabled);
                }
            }
        }

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

        private Tournament newMatchTournament;
        public Tournament NewMatchTournament
        {
            get { return newMatchTournament; }
            set
            {
                if (value != newMatchTournament)
                {
                    newMatchTournament = value;
                    NewMatchTeamRed = null;
                    NewMatchTeamBlue = null;
                    NotifyOfPropertyChange(() => NewMatchTournament);
                    NotifyOfPropertyChange(() => Mappools);
                    NotifyOfPropertyChange(() => Teams);
                    NotifyOfPropertyChange(() => NewMatchSaveEnabled);
                    if (value != null)
                    {
                        NewMatchGameMode = value.GameMode;
                        NewMatchTeamMode = value.TeamMode;
                        NewMatchWinCondition = value.WinCondition;
                        NewMatchTeamSize = value.TeamSize;
                        NewMatchRoomSize = value.RoomSize;
                    }
                    GenerateName();
                }
            }
        }

        public BindableCollection<Mappool> Mappools
        {
            get
            {
                var list = new BindableCollection<Mappool>();
                foreach (var mappool in Database.Database.Mappools.OrderBy(x => x.Name))
                {
                    if (mappool.Tournament != NewMatchTournament)
                        continue;
                    list.Add(mappool);
                }
                return list;
            }
        }

        private Mappool newMatchMappool;
        public Mappool NewMatchMappool
        {
            get { return newMatchMappool; }
            set
            {
                if (value != newMatchMappool)
                {
                    newMatchMappool = value;
                    NotifyOfPropertyChange(() => NewMatchMappool);
                }
            }
        }

        public List<GameModes> GameModesList
        {
            get { return Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList(); }
        }

        private GameModes newMatchGameMode;
        public GameModes NewMatchGameMode
        {
            get { return newMatchGameMode; }
            set
            {
                if (value != newMatchGameMode)
                {
                    newMatchGameMode = value;
                    NotifyOfPropertyChange(() => NewMatchGameMode);
                }
            }
        }

        public List<TeamModes> TeamModesList
        {
            get { return Enum.GetValues(typeof(TeamModes)).Cast<TeamModes>().ToList(); }
        }

        private TeamModes newMatchTeamMode;
        public TeamModes NewMatchTeamMode
        {
            get { return newMatchTeamMode; }
            set
            {
                if (value != newMatchTeamMode)
                {
                    newMatchTeamMode = value;
                    NotifyOfPropertyChange(() => NewMatchTeamMode);
                    NotifyOfPropertyChange(() => TeamsEditorIsVisible);
                    NotifyOfPropertyChange(() => PlayersEditorIsVisible);
                    GenerateName();
                }
            }
        }

        public List<WinConditions> WinConditionsList
        {
            get { return Enum.GetValues(typeof(WinConditions)).Cast<WinConditions>().ToList(); }
        }

        private WinConditions newMatchWinCondition;
        public WinConditions NewMatchWinCondition
        {
            get { return newMatchWinCondition; }
            set
            {
                if (value != newMatchWinCondition)
                {
                    newMatchWinCondition = value;
                    NotifyOfPropertyChange(() => NewMatchWinCondition);
                }
            }
        }

        private int newMatchBO;
        public int NewMatchBO
        {
            get { return newMatchBO; }
            set
            {
                if (value != newMatchBO)
                {
                    newMatchBO = value;
                    NotifyOfPropertyChange(() => NewMatchBO);
                }
            }
        }

        public Visibility TeamsEditorIsVisible
        {
            get
            {
                if (NewMatchTeamMode == TeamModes.TeamVS)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public BindableCollection<Team> Teams
        {
            get
            {
                var list = new BindableCollection<Team>();
                foreach (var team in Database.Database.Teams.OrderBy(x => x.Name))
                {
                    if (team.Tournament != NewMatchTournament)
                        continue;
                    list.Add(team);
                }
                return list;
            }
        }

        private Team newMatchTeamBlue;
        public Team NewMatchTeamBlue
        {
            get { return newMatchTeamBlue; }
            set
            {
                if (value != newMatchTeamBlue)
                {
                    newMatchTeamBlue = value;
                    NotifyOfPropertyChange(() => NewMatchTeamBlue);
                    NotifyOfPropertyChange(() => NewMatchSaveEnabled);
                    GenerateName();
                }
            }
        }

        private Team newMatchTeamRed;
        public Team NewMatchTeamRed
        {
            get { return newMatchTeamRed; }
            set
            {
                if (value != newMatchTeamRed)
                {
                    newMatchTeamRed = value;
                    NotifyOfPropertyChange(() => NewMatchTeamRed);
                    NotifyOfPropertyChange(() => NewMatchSaveEnabled);
                    GenerateName();
                }
            }
        }

        private int newMatchTeamSize;
        public int NewMatchTeamSize
        {
            get { return newMatchTeamSize; }
            set
            {
                if (value != newMatchTeamSize)
                {
                    newMatchTeamSize = value;
                    NotifyOfPropertyChange(() => NewMatchTeamSize);
                }
            }
        }

        private int newMatchRoomSize;
        public int NewMatchRoomSize
        {
            get { return newMatchRoomSize; }
            set
            {
                if (value != newMatchRoomSize)
                {
                    newMatchRoomSize = value;
                    NotifyOfPropertyChange(() => NewMatchRoomSize);
                }
            }
        }

        public Visibility PlayersEditorIsVisible
        {
            get
            {
                if (NewMatchTeamMode == TeamModes.HeadToHead)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        private List<Player> newMatchPlayers;

        public BindableCollection<MatchPlayerListItemViewModel> PlayersViews
        {
            get
            {
                var list = new BindableCollection<MatchPlayerListItemViewModel>();
                foreach (var player in newMatchPlayers)
                    list.Add(new MatchPlayerListItemViewModel(player));
                return list;
            }
        }

        private string addPlayerNameOrId;
        public string AddPlayerNameOrId
        {
            get { return addPlayerNameOrId; }
            set
            {
                if (value != addPlayerNameOrId)
                {
                    addPlayerNameOrId = value;
                    NotifyOfPropertyChange(() => AddPlayerNameOrId);
                }
            }
        }

        public void AddPlayer()
        {
            if (string.IsNullOrEmpty(addPlayerNameOrId))
                return;
            var player = Database.Database.GetPlayer(addPlayerNameOrId);
            if (player == null)
                return;
            if (newMatchPlayers.Contains(player))
                return;
            Log.Information("GUI new match add player {name}", player.Name);
            newMatchPlayers.Add(player);
            AddPlayerNameOrId = "";
            NotifyOfPropertyChange(() => PlayersViews);
        }

        public void AddPlayerNameKeyDown(ActionExecutionContext context)
        {
            var keyArgs = context.EventArgs as KeyEventArgs;
            if (keyArgs != null && keyArgs.Key == Key.Enter)
                AddPlayer();
        }

        public void RemovePlayer(MatchPlayerListItemViewModel model)
        {
            Log.Information("GUI new match remove player {name}", model.Player.Name);
            newMatchPlayers.Remove(model.Player);
            NotifyOfPropertyChange(() => PlayersViews);
        }

        private void GenerateName()
        {
            if (string.IsNullOrEmpty(NewMatchName) && NewMatchTeamMode == TeamModes.TeamVS && NewMatchTournament != null && NewMatchTeamBlue != null && NewMatchTeamRed != null)
            {
                NewMatchName = $"{NewMatchTournament.Acronym}: ({NewMatchTeamRed.Name}) VS ({NewMatchTeamBlue.Name})";
            }
        }

        public bool NewMatchSaveEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(NewMatchName))
                    return false;
                if (NewMatchTournament == null)
                    return false;
                if (NewMatchTeamMode == TeamModes.TeamVS)
                {
                    if (NewMatchTeamBlue == null)
                        return false;
                    if (NewMatchTeamRed == null)
                        return false;
                    if (NewMatchTeamBlue == NewMatchTeamRed)
                        return false;
                }
                return true;
            }
        }

        public void NewMatchDialogOpened()
        {
            Log.Information("GUI new match dialog open");
            NewMatchName = "";
            NewMatchTeamBlue = null;
            NewMatchTeamRed = null;
            NewMatchTeamSize = 4;
            NewMatchRoomSize = 8;
            NewMatchTournament = Settings.DefaultTournament;
            NewMatchBO = Settings.DefaultBO;
            newMatchPlayers = new List<Player>();
        }

        public void NewMatchDialogClosed()
        {
            Log.Information("GUI new match '{match}' save", NewMatchName);
            var match = new Match()
            {
                Tournament = NewMatchTournament,
                Mappool = NewMatchMappool,
                Name = NewMatchName,
                GameMode = NewMatchGameMode,
                TeamMode = NewMatchTeamMode,
                WinCondition = NewMatchWinCondition,
                TeamBlue = NewMatchTeamBlue,
                TeamRed = NewMatchTeamRed,
                TeamSize = NewMatchTeamSize,
                RoomSize = NewMatchRoomSize,
                BO = NewMatchBO,
                MpTimerCommand = NewMatchTournament.MpTimerCommand,
                MpTimerAfterGame = NewMatchTournament.MpTimerAfterGame,
                MpTimerAfterPick = NewMatchTournament.MpTimerAfterPick,
                PointsForSecondBan = NewMatchTournament.PointsForSecondBan,
                AllPicksFreemod = NewMatchTournament.AllPicksFreemod
            };
            foreach (var player in newMatchPlayers)
            {
                match.Players.Add(player, 0);
            }
            match.Save();
            Settings.DefaultTournament = NewMatchTournament;
            Settings.DefaultBO = NewMatchBO;
            NotifyOfPropertyChange(() => NewMatchTournament);
            NotifyOfPropertyChange(() => MatchesViews);
        }
        #endregion
    }
}
