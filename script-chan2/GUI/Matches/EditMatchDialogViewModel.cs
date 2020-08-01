using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using script_chan2.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace script_chan2.GUI
{
    public class EditMatchDialogViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<EditMatchDialogViewModel>();

        #region Constructor
        public EditMatchDialogViewModel(int id = 0)
        {
            this.id = id;
            if (id > 0)
            {
                var match = Database.Database.Matches.First(x => x.Id == id);
                Name = match.Name;
                Tournament = match.Tournament;
                Mappool = match.Mappool;
                GameMode = match.GameMode;
                TeamMode = match.TeamMode;
                WinCondition = match.WinCondition;
                BO = match.BO;
                TeamBlue = match.TeamBlue;
                TeamRed = match.TeamRed;
                TeamSize = match.TeamSize;
                RoomSize = match.RoomSize;
                BRTeams = new List<Team>();
                foreach (var team in match.TeamsBR)
                    BRTeams.Add(team.Key);
                Players = new List<Player>();
                foreach (var player in match.Players)
                    Players.Add(player.Key);
                NotifyOfPropertyChange(() => PlayerViews);
                PlayerNameOrId = "";
            }
            else
            {
                Name = "";
                TeamBlue = null;
                TeamRed = null;
                TeamSize = 4;
                RoomSize = 8;
                Tournament = Settings.DefaultTournament;
                BO = Settings.DefaultBO;
                BRTeams = new List<Team>();
                Players = new List<Player>();
            }
        }
        #endregion

        #region Properties
        private int id;

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    NotifyOfPropertyChange(() => Name);
                    NotifyOfPropertyChange(() => SaveEnabled);
                }
            }
        }

        public bool TournamentEnabled
        {
            get
            {
                return id == 0;
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

        private Tournament tournament;
        public Tournament Tournament
        {
            get { return tournament; }
            set
            {
                if (value != tournament)
                {
                    tournament = value;
                    TeamRed = null;
                    TeamBlue = null;
                    NotifyOfPropertyChange(() => Tournament);
                    NotifyOfPropertyChange(() => Mappools);
                    NotifyOfPropertyChange(() => Teams);
                    NotifyOfPropertyChange(() => SaveEnabled);
                    if (value != null)
                    {
                        GameMode = value.GameMode;
                        TeamMode = value.TeamMode;
                        WinCondition = value.WinCondition;
                        TeamSize = value.TeamSize;
                        RoomSize = value.RoomSize;
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
                    if (mappool.Tournament != Tournament)
                        continue;
                    list.Add(mappool);
                }
                return list;
            }
        }

        private Mappool mappool;
        public Mappool Mappool
        {
            get { return mappool; }
            set
            {
                if (value != mappool)
                {
                    mappool = value;
                    NotifyOfPropertyChange(() => Mappool);
                }
            }
        }

        public List<GameModes> GameModesList
        {
            get { return Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList(); }
        }

        private GameModes gameMode;
        public GameModes GameMode
        {
            get { return gameMode; }
            set
            {
                if (value != gameMode)
                {
                    gameMode = value;
                    NotifyOfPropertyChange(() => GameMode);
                }
            }
        }

        public List<TeamModes> TeamModesList
        {
            get { return Enum.GetValues(typeof(TeamModes)).Cast<TeamModes>().ToList(); }
        }

        private TeamModes teamMode;
        public TeamModes TeamMode
        {
            get { return teamMode; }
            set
            {
                if (value != teamMode)
                {
                    teamMode = value;
                    NotifyOfPropertyChange(() => TeamMode);
                    NotifyOfPropertyChange(() => TeamsEditorIsVisible);
                    NotifyOfPropertyChange(() => PlayersEditorIsVisible);
                    NotifyOfPropertyChange(() => BRTeamsEditorIsVisible);
                    GenerateName();
                }
            }
        }

        public List<WinConditions> WinConditionsList
        {
            get { return Enum.GetValues(typeof(WinConditions)).Cast<WinConditions>().ToList(); }
        }

        private WinConditions winCondition;
        public WinConditions WinCondition
        {
            get { return winCondition; }
            set
            {
                if (value != winCondition)
                {
                    winCondition = value;
                    NotifyOfPropertyChange(() => WinCondition);
                }
            }
        }

        private int bo;
        public int BO
        {
            get { return bo; }
            set
            {
                if (value != bo)
                {
                    bo = value;
                    NotifyOfPropertyChange(() => BO);
                }
            }
        }

        public Visibility TeamsEditorIsVisible
        {
            get
            {
                if (TeamMode == TeamModes.TeamVS)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility BRTeamsEditorIsVisible
        {
            get
            {
                if (TeamMode == TeamModes.BattleRoyale)
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
                    if (team.Tournament != Tournament)
                        continue;
                    list.Add(team);
                }
                return list;
            }
        }

        public List<Team> BRTeams;

        public BindableCollection<MatchBRTeamListItemViewModel> BRTeamViews
        {
            get
            {
                var list = new BindableCollection<MatchBRTeamListItemViewModel>();
                foreach (var team in BRTeams)
                    list.Add(new MatchBRTeamListItemViewModel(team));
                return list;
            }
        }

        public BindableCollection<Team> SelectableBRTeams
        {
            get
            {
                var list = new BindableCollection<Team>();
                foreach (var team in Database.Database.Teams.OrderBy(x => x.Name))
                {
                    if (team.Tournament != Tournament)
                        continue;
                    if (BRTeams.Contains(team))
                        continue;
                    list.Add(team);
                }
                return list;
            }
        }

        private Team teamBlue;
        public Team TeamBlue
        {
            get { return teamBlue; }
            set
            {
                if (value != teamBlue)
                {
                    teamBlue = value;
                    NotifyOfPropertyChange(() => TeamBlue);
                    NotifyOfPropertyChange(() => SaveEnabled);
                    GenerateName();
                }
            }
        }

        private Team teamRed;
        public Team TeamRed
        {
            get { return teamRed; }
            set
            {
                if (value != teamRed)
                {
                    teamRed = value;
                    NotifyOfPropertyChange(() => TeamRed);
                    NotifyOfPropertyChange(() => SaveEnabled);
                    GenerateName();
                }
            }
        }

        private Team selectedBRTeam;
        public Team SelectedBRTeam
        {
            get { return selectedBRTeam; }
            set
            {
                if (value != selectedBRTeam)
                {
                    selectedBRTeam = value;
                    NotifyOfPropertyChange(() => SelectedBRTeam);
                }
            }
        }

        private int teamSize;
        public int TeamSize
        {
            get { return teamSize; }
            set
            {
                if (value != teamSize)
                {
                    teamSize = value;
                    NotifyOfPropertyChange(() => TeamSize);
                }
            }
        }

        private int roomSize;
        public int RoomSize
        {
            get { return roomSize; }
            set
            {
                if (value != roomSize)
                {
                    roomSize = value;
                    NotifyOfPropertyChange(() => RoomSize);
                }
            }
        }

        public Visibility PlayersEditorIsVisible
        {
            get
            {
                if (TeamMode == TeamModes.HeadToHead)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public List<Player> Players;

        public BindableCollection<MatchPlayerListItemViewModel> PlayerViews
        {
            get
            {
                var list = new BindableCollection<MatchPlayerListItemViewModel>();
                foreach (var player in Players)
                    list.Add(new MatchPlayerListItemViewModel(player));
                return list;
            }
        }

        private string playerNameOrId;
        public string PlayerNameOrId
        {
            get { return playerNameOrId; }
            set
            {
                if (value != playerNameOrId)
                {
                    playerNameOrId = value;
                    NotifyOfPropertyChange(() => PlayerNameOrId);
                }
            }
        }

        public async Task AddPlayer()
        {
            if (string.IsNullOrEmpty(PlayerNameOrId))
                return;
            var player = await Database.Database.GetPlayer(PlayerNameOrId);
            if (player == null)
                return;
            if (Players.Contains(player))
                return;
            localLog.Information("add player {name}", player.Name);
            Players.Add(player);
            PlayerNameOrId = "";
            NotifyOfPropertyChange(() => PlayerViews);
        }

        public void RemovePlayer(MatchPlayerListItemViewModel model)
        {
            localLog.Information("remove player {name}", model.Player.Name);
            Players.Remove(model.Player);
            NotifyOfPropertyChange(() => PlayerViews);
        }

        public void RemoveBRTeam(MatchBRTeamListItemViewModel model)
        {
            localLog.Information("remove team {team}", model.Team.Name);
            BRTeams.Remove(model.Team);
            NotifyOfPropertyChange(() => BRTeams);
            NotifyOfPropertyChange(() => SelectableBRTeams);
            NotifyOfPropertyChange(() => BRTeamViews);
        }

        public void AddBRTeam()
        {
            if (SelectedBRTeam == null)
                return;
            if (BRTeams.Contains(SelectedBRTeam))
                return;
            localLog.Information("add team {name}", SelectedBRTeam.Name);
            BRTeams.Add(SelectedBRTeam);
            SelectedBRTeam = null;
            NotifyOfPropertyChange(() => SelectableBRTeams);
            NotifyOfPropertyChange(() => BRTeams);
            NotifyOfPropertyChange(() => BRTeamViews);
        }

        private void GenerateName()
        {
            if (string.IsNullOrEmpty(Name) && TeamMode == TeamModes.TeamVS && Tournament != null && TeamBlue != null && TeamRed != null)
            {
                localLog.Information("generate match name");
                Name = $"{Tournament.Acronym}: ({TeamRed.Name}) VS ({TeamBlue.Name})";
            }
        }

        public bool SaveEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return false;
                if (Tournament == null)
                    return false;
                if (TeamMode == TeamModes.TeamVS)
                {
                    if (TeamBlue == null)
                        return false;
                    if (TeamRed == null)
                        return false;
                    if (TeamBlue == TeamRed)
                        return false;
                }
                return true;
            }
        }
        #endregion

        #region Actions
        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
