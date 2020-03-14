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
    class MatchListItemViewModel : Screen
    {
        #region Constructor
        public MatchListItemViewModel(Match match)
        {
            this.match = match;
            Events.Aggregator.Subscribe(this);
            editMatchPlayers = new List<Player>();
        }
        #endregion

        #region Properties
        private Match match;

        public string Name
        {
            get { return match.Name; }
        }

        public string TournamentName
        {
            get
            {
                return match.Tournament.Name;
            }
        }
        #endregion

        #region Edit match dialog
        private string editMatchName;
        public string EditMatchName
        {
            get { return editMatchName; }
            set
            {
                if (value != editMatchName)
                {
                    editMatchName = value;
                    NotifyOfPropertyChange(() => EditMatchName);
                    NotifyOfPropertyChange(() => EditMatchSaveEnabled);
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

        private Tournament editMatchTournament;
        public Tournament EditMatchTournament
        {
            get { return editMatchTournament; }
            set
            {
                if (value != editMatchTournament)
                {
                    editMatchTournament = value;
                    NotifyOfPropertyChange(() => EditMatchTournament);
                    NotifyOfPropertyChange(() => Mappools);
                    NotifyOfPropertyChange(() => EditMatchSaveEnabled);
                    if (value != null)
                    {
                        EditMatchGameMode = value.GameMode;
                        EditMatchTeamMode = value.TeamMode;
                        EditMatchWinCondition = value.WinCondition;
                    }
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
                    if (mappool.Tournament != EditMatchTournament)
                        continue;
                    list.Add(mappool);
                }
                return list;
            }
        }

        private Mappool editMatchMappool;
        public Mappool EditMatchMappool
        {
            get { return editMatchMappool; }
            set
            {
                if (value != editMatchMappool)
                {
                    editMatchMappool = value;
                    NotifyOfPropertyChange(() => EditMatchMappool);
                }
            }
        }

        public List<GameModes> GameModesList
        {
            get { return Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList(); }
        }

        private GameModes editMatchGameMode;
        public GameModes EditMatchGameMode
        {
            get { return editMatchGameMode; }
            set
            {
                if (value != editMatchGameMode)
                {
                    editMatchGameMode = value;
                    NotifyOfPropertyChange(() => EditMatchGameMode);
                }
            }
        }

        public List<TeamModes> TeamModesList
        {
            get { return Enum.GetValues(typeof(TeamModes)).Cast<TeamModes>().ToList(); }
        }

        private TeamModes editMatchTeamMode;
        public TeamModes EditMatchTeamMode
        {
            get { return editMatchTeamMode; }
            set
            {
                if (value != editMatchTeamMode)
                {
                    editMatchTeamMode = value;
                    NotifyOfPropertyChange(() => EditMatchTeamMode);
                    NotifyOfPropertyChange(() => TeamsEditorIsVisible);
                    NotifyOfPropertyChange(() => PlayersEditorIsVisible);
                }
            }
        }

        public List<WinConditions> WinConditionsList
        {
            get { return Enum.GetValues(typeof(WinConditions)).Cast<WinConditions>().ToList(); }
        }

        private WinConditions editMatchWinCondition;
        public WinConditions EditMatchWinCondition
        {
            get { return editMatchWinCondition; }
            set
            {
                if (value != editMatchWinCondition)
                {
                    editMatchWinCondition = value;
                    NotifyOfPropertyChange(() => EditMatchWinCondition);
                }
            }
        }

        private int editMatchBO;
        public int EditMatchBO
        {
            get { return editMatchBO; }
            set
            {
                if (value != editMatchBO)
                {
                    editMatchBO = value;
                    NotifyOfPropertyChange(() => EditMatchBO);
                }
            }
        }

        public Visibility TeamsEditorIsVisible
        {
            get
            {
                if (EditMatchTeamMode == TeamModes.TeamVS)
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
                    if (team.Tournament != EditMatchTournament)
                        continue;
                    list.Add(team);
                }
                return list;
            }
        }

        private Team editMatchTeamBlue;
        public Team EditMatchTeamBlue
        {
            get { return editMatchTeamBlue; }
            set
            {
                if (value != editMatchTeamBlue)
                {
                    editMatchTeamBlue = value;
                    NotifyOfPropertyChange(() => EditMatchTeamBlue);
                    NotifyOfPropertyChange(() => EditMatchSaveEnabled);
                }
            }
        }

        private Team editMatchTeamRed;
        public Team EditMatchTeamRed
        {
            get { return editMatchTeamRed; }
            set
            {
                if (value != editMatchTeamRed)
                {
                    editMatchTeamRed = value;
                    NotifyOfPropertyChange(() => EditMatchTeamRed);
                    NotifyOfPropertyChange(() => EditMatchSaveEnabled);
                }
            }
        }

        private int editMatchTeamSize;
        public int EditMatchTeamSize
        {
            get { return editMatchTeamSize; }
            set
            {
                if (value != editMatchTeamSize)
                {
                    editMatchTeamSize = value;
                    NotifyOfPropertyChange(() => EditMatchTeamSize);
                }
            }
        }

        private int editMatchRoomSize;
        public int EditMatchRoomSize
        {
            get { return editMatchRoomSize; }
            set
            {
                if (value != editMatchRoomSize)
                {
                    editMatchRoomSize = value;
                    NotifyOfPropertyChange(() => EditMatchRoomSize);
                }
            }
        }

        public Visibility PlayersEditorIsVisible
        {
            get
            {
                if (EditMatchTeamMode == TeamModes.HeadToHead)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        private List<Player> editMatchPlayers;

        public BindableCollection<MatchPlayerListItemViewModel> PlayersViews
        {
            get
            {
                var list = new BindableCollection<MatchPlayerListItemViewModel>();
                foreach (var player in editMatchPlayers)
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
            if (editMatchPlayers.Contains(player))
                return;
            Log.Information("GUI edit match '{match}' add player '{player}'", match.Name, player.Name);
            editMatchPlayers.Add(player);
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
            Log.Information("GUI edit match '{match}' remove player '{player}'", match.Name, model.Player.Name);
            editMatchPlayers.Remove(model.Player);
            NotifyOfPropertyChange(() => PlayersViews);
        }

        public bool EditMatchSaveEnabled
        {
            get
            {
                if (match.Status != MatchStatus.New)
                    return false;
                if (string.IsNullOrEmpty(EditMatchName))
                    return false;
                if (EditMatchTournament == null)
                    return false;
                if (EditMatchTeamMode == TeamModes.TeamVS)
                {
                    if (EditMatchTeamBlue == null)
                        return false;
                    if (EditMatchTeamRed == null)
                        return false;
                    if (EditMatchTeamBlue == EditMatchTeamRed)
                        return false;
                }
                return true;
            }
        }

        public void Edit()
        {
            Log.Information("GUI edit match {name}", match.Name);
            EditMatchName = match.Name;
            EditMatchTournament = match.Tournament;
            EditMatchMappool = match.Mappool;
            EditMatchGameMode = match.GameMode;
            EditMatchTeamMode = match.TeamMode;
            EditMatchWinCondition = match.WinCondition;
            EditMatchBO = match.BO;
            EditMatchTeamBlue = match.TeamBlue;
            EditMatchTeamRed = match.TeamRed;
            EditMatchTeamSize = match.TeamSize;
            EditMatchRoomSize = match.RoomSize;
            editMatchPlayers = new List<Player>();
            foreach (var player in match.Players)
            {
                editMatchPlayers.Add(player.Key);
            }
            NotifyOfPropertyChange(() => PlayersViews);
        }

        public void Save()
        {
            if (EditMatchSaveEnabled)
            {
                Log.Information("GUI save match {name}", match.Name);
                match.Name = EditMatchName;
                match.Tournament = EditMatchTournament;
                match.Mappool = EditMatchMappool;
                match.GameMode = EditMatchGameMode;
                match.TeamMode = EditMatchTeamMode;
                match.WinCondition = EditMatchWinCondition;
                match.BO = EditMatchBO;
                match.TeamBlue = EditMatchTeamBlue;
                match.TeamRed = EditMatchTeamRed;
                match.TeamSize = EditMatchTeamSize;
                match.RoomSize = EditMatchRoomSize;
                match.Players.Clear();
                foreach (var player in editMatchPlayers)
                {
                    match.Players.Add(player, 0);
                }
                match.Save();
                NotifyOfPropertyChange(() => Name);
                Events.Aggregator.PublishOnUIThread("EditMatch");
            }
        }
        #endregion

        #region Actions
        public void Delete()
        {
            Log.Information("GUI delete match '{name}'", match.Name);
            match.Delete();
            Events.Aggregator.PublishOnUIThread("DeleteMatch");
        }

        public void Open()
        {
            if (!MatchList.OpenedMatches.Contains(match))
            {
                Log.Information("GUI open match '{name}'", match.Name);
                var windowManager = new WindowManager();
                windowManager.ShowWindow(new MatchViewModel(match));
                MatchList.OpenedMatches.Add(match);
            }
        }
        #endregion
    }
}
