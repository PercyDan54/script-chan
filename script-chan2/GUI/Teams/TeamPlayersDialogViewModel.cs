using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using WK.Libraries.SharpClipboardNS;

namespace script_chan2.GUI
{
    public class TeamPlayersDialogViewModel : Screen, IHandle<string>
    {
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

        private SharpClipboard clipboard;

        public void Activate()
        {
            Log.Information("TeamListItemViewModel: player list dialog of team '{team}' open", team.Name);
            AddPlayerNameOrId = "";
            Clipboard.SetText("");
            clipboard = new SharpClipboard();
            clipboard.ClipboardChanged += Clipboard_ClipboardChanged;
        }

        public void Deactivate()
        {
            Log.Information("TeamListItemViewModel: player list dialog of team '{team}' close", team.Name);
            clipboard.ClipboardChanged -= Clipboard_ClipboardChanged;
        }

        private void Clipboard_ClipboardChanged(object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            if (e.ContentType != SharpClipboard.ContentTypes.Text)
                return;

            var text = clipboard.ClipboardText;

            if (Regex.IsMatch(text, @"https://osu.ppy.sh/users/\d*"))
                text = text.Split('/').Last();

            if (int.TryParse(text, out int id))
            {
                Log.Information("TeamListItemViewModel: team player list dialog clipboard event, found id {id}", id);
                if (!string.IsNullOrEmpty(AddPlayerNameOrId))
                    AddPlayerNameOrId += ";";
                AddPlayerNameOrId += text;
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
        #endregion

        #region Actions
        public void AddPlayer()
        {
            if (string.IsNullOrEmpty(AddPlayerNameOrId))
                return;
            var playerList = AddPlayerNameOrId.Split(';');
            foreach (var playerId in playerList)
            {
                var player = Database.Database.GetPlayer(playerId);
                if (player == null)
                    continue;
                Log.Information("TeamListItemViewModel: edit team '{team}' add player '{player}'", team.Name, player.Name);
                team.AddPlayer(player);
            }
            AddPlayerNameOrId = "";
            NotifyOfPropertyChange(() => PlayerViews);
        }

        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
