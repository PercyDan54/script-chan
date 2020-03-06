using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace script_chan2.GUI
{
    public class TeamListItemViewModel : Screen, IHandle<string>
    {
        public TeamListItemViewModel(Team team)
        {
            this.team = team;
            Events.Aggregator.Subscribe(this);
        }

        private Team team;

        public BindableCollection<TeamPlayerListItemViewModel> PlayersViews
        {
            get
            {
                var list = new BindableCollection<TeamPlayerListItemViewModel>();
                foreach (var player in team.Players)
                    list.Add(new TeamPlayerListItemViewModel(team, player));
                return list;
            }
        }

        public void Handle(string message)
        {
            if (message == "RemovePlayerFromTeam")
                NotifyOfPropertyChange(() => PlayersViews);
        }

        public string Name
        {
            get { return team.Name; }
        }

        public bool HasTournament
        {
            get { return team.Tournament != null; }
        }

        public string TournamentName
        {
            get
            {
                if (team.Tournament == null)
                    return "";
                return team.Tournament.Name;
            }
        }

        private string editName;
        public string EditName
        {
            get { return editName; }
            set
            {
                if (value != editName)
                {
                    editName = value;
                    NotifyOfPropertyChange(() => EditName);
                    NotifyOfPropertyChange(() => EditTeamSaveEnabled);
                }
            }
        }

        public BindableCollection<Tournament> Tournaments
        {
            get
            {
                var list = new BindableCollection<Tournament>();
                foreach (var tournament in Database.Database.Tournaments.OrderBy(x => x.Name))
                    list.Add(tournament);
                return list;
            }
        }

        private Tournament editTournament;
        public Tournament EditTournament
        {
            get { return editTournament; }
            set
            {
                if (value != editTournament)
                {
                    editTournament = value;
                    NotifyOfPropertyChange(() => EditTournament);
                    NotifyOfPropertyChange(() => EditTeamSaveEnabled);
                }
            }
        }

        public bool EditTeamSaveEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(editName))
                    return false;
                if (Database.Database.Teams.Any(x => x.Name == editName && x.Tournament == editTournament && x.Id != team.Id))
                    return false;
                return true;
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
            team.AddPlayer(player);
            AddPlayerNameOrId = "";
            NotifyOfPropertyChange(() => PlayersViews);
        }

        public void AddPlayerNameKeyDown(ActionExecutionContext context)
        {
            var keyArgs = context.EventArgs as KeyEventArgs;
            if (keyArgs != null && keyArgs.Key == Key.Enter)
                AddPlayer();
        }

        public void EditPlayers()
        {
            AddPlayerNameOrId = "";
        }

        public void Edit()
        {
            EditName = team.Name;
            EditTournament = team.Tournament;
        }

        public void Save()
        {
            if (EditTeamSaveEnabled)
            {
                team.Name = EditName;
                team.Save();
                NotifyOfPropertyChange(() => Name);
                NotifyOfPropertyChange(() => TournamentName);
            }
        }

        public void Delete()
        {
            team.Delete();
            Events.Aggregator.PublishOnUIThread("DeleteTeam");
        }
    }
}
