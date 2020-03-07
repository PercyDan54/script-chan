using Caliburn.Micro;
using script_chan2.DataTypes;
using script_chan2.Enums;
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
        }
        #endregion

        #region Properties
        private Match match;

        public string Name
        {
            get { return match.Name; }
        }

        public bool HasTournament
        {
            get { return match.Tournament != null; }
        }

        public string TournamentName
        {
            get
            {
                if (match.Tournament == null)
                    return "";
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
                return Visibility.Hidden;
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
                    if (editMatchTeams.Contains(team))
                        continue;
                    list.Add(team);
                }
                return list;
            }
        }

        private Team selectedTeam;
        public Team SelectedTeam
        {
            get { return selectedTeam; }
            set
            {
                if (value != selectedTeam)
                {
                    selectedTeam = value;
                    NotifyOfPropertyChange(() => SelectedTeam);
                }
            }
        }

        private List<Team> editMatchTeams;

        public BindableCollection<MatchTeamListItemViewModel> TeamsViews
        {
            get
            {
                var list = new BindableCollection<MatchTeamListItemViewModel>();
                foreach (var team in editMatchTeams)
                    list.Add(new MatchTeamListItemViewModel(team));
                return list;
            }
        }

        public void AddTeam()
        {
            if (SelectedTeam == null)
                return;
            editMatchTeams.Add(SelectedTeam);
            SelectedTeam = null;
            NotifyOfPropertyChange(() => TeamsViews);
            NotifyOfPropertyChange(() => Teams);
        }

        public void RemoveTeam(MatchTeamListItemViewModel model)
        {
            editMatchTeams.Remove(model.Team);
            NotifyOfPropertyChange(() => TeamsViews);
            NotifyOfPropertyChange(() => Teams);
        }

        public Visibility PlayersEditorIsVisible
        {
            get
            {
                if (EditMatchTeamMode == TeamModes.HeadToHead)
                    return Visibility.Visible;
                return Visibility.Hidden;
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
            editMatchPlayers.Remove(model.Player);
            NotifyOfPropertyChange(() => PlayersViews);
        }

        public bool EditMatchSaveEnabled
        {
            get
            {
                if (match.Status != MatchStatus.New)
                    return false;
                if (string.IsNullOrEmpty(editMatchName))
                    return false;
                return true;
            }
        }

        public void Edit()
        {
            EditMatchName = match.Name;
            EditMatchTournament = match.Tournament;
            EditMatchMappool = match.Mappool;
            EditMatchGameMode = match.GameMode;
            EditMatchTeamMode = match.TeamMode;
            EditMatchWinCondition = match.WinCondition;
            EditMatchBO = match.BO;
            editMatchTeams = match.Teams.ToList();
            editMatchPlayers = match.Players.ToList();
            NotifyOfPropertyChange(() => TeamsViews);
            NotifyOfPropertyChange(() => Teams);
            NotifyOfPropertyChange(() => PlayersViews);
        }

        public void Save()
        {
            if (EditMatchSaveEnabled)
            {
                match.Name = EditMatchName;
                match.Tournament = EditMatchTournament;
                match.Mappool = EditMatchMappool;
                match.GameMode = EditMatchGameMode;
                match.TeamMode = EditMatchTeamMode;
                match.WinCondition = EditMatchWinCondition;
                match.BO = EditMatchBO;
                match.Teams = editMatchTeams;
                match.Players = editMatchPlayers;
                match.Save();
                NotifyOfPropertyChange(() => Name);
                Events.Aggregator.PublishOnUIThread("EditMatch");
            }
        }
        #endregion

        public void Delete()
        {
            match.Delete();
            Events.Aggregator.PublishOnUIThread("DeleteMatch");
        }
    }
}
