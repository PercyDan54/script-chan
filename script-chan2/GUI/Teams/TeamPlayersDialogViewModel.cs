using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace script_chan2.GUI
{
    public class TeamPlayersDialogViewModel : Screen, IHandle<string>
    {
        private ILogger localLog = Log.ForContext<TeamPlayersDialogViewModel>();

        #region Constructor
        public TeamPlayersDialogViewModel(Team team)
        {
            this.team = team;
            Events.Aggregator.Subscribe(this);
        }
        #endregion

        #region Events
        public void Handle(string message)
        {
            if (message == "RemovePlayerFromTeam")
                NotifyOfPropertyChange(() => PlayerViews);
        }
        #endregion

        #region Properties
        private Team team;

        public BindableCollection<TeamPlayerListItemViewModel> PlayerViews
        {
            get
            {
                var list = new BindableCollection<TeamPlayerListItemViewModel>();
                foreach (var player in team.Players)
                    list.Add(new TeamPlayerListItemViewModel(team, player));
                return list;
            }
        }

        public void Activate()
        {
            localLog.Information("player list dialog of team '{team}' open", team.Name);
            AddPlayerNameOrId = "";
        }

        public void Deactivate()
        {
            localLog.Information("player list dialog of team '{team}' close", team.Name);
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

        private bool isAddingPlayer;
        public bool IsAddingPlayer
        {
            get { return isAddingPlayer; }
            set
            {
                if (value != isAddingPlayer)
                {
                    isAddingPlayer = value;
                    NotifyOfPropertyChange(() => AddButtonVisible);
                    NotifyOfPropertyChange(() => AddProgressVisible);
                    NotifyOfPropertyChange(() => CloseButtonEnabled);
                }
            }
        }

        public Visibility AddButtonVisible
        {
            get
            {
                if (IsAddingPlayer)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility AddProgressVisible
        {
            get
            {
                if (IsAddingPlayer)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public bool CloseButtonEnabled
        {
            get { return !IsAddingPlayer; }
        }
        #endregion

        #region Actions
        public async Task AddPlayer()
        {
            if (string.IsNullOrEmpty(AddPlayerNameOrId))
                return;

            localLog.Information("add players '{players}'", AddPlayerNameOrId);
            IsAddingPlayer = true;
            var playerList = AddPlayerNameOrId.Split(';');
            foreach (var playerId in playerList)
            {
                await AddPlayerInternal(playerId);
            }
            AddPlayerNameOrId = "";
            IsAddingPlayer = false;
        }

        private async Task AddPlayerInternal(string idOrName)
        {
            var player = await Database.Database.GetPlayer(idOrName);
            if (player != null)
            {
                team.AddPlayer(player);
                NotifyOfPropertyChange(() => PlayerViews);
            }
        }

        public void DialogEscape()
        {
            if (!IsAddingPlayer)
                DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
