using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class TeamsViewModel : Screen, IHandle<string>
    {
        public BindableCollection<Team> Teams { get; set; }

        public BindableCollection<TeamListItemViewModel> TeamsViews
        {
            get
            {
                var list = new BindableCollection<TeamListItemViewModel>();
                foreach (var team in Teams)
                    list.Add(new TeamListItemViewModel(team));
                return list;
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

        protected override void OnActivate()
        {
            Reload();
            Events.Aggregator.Subscribe(this);
        }

        public void Reload()
        {
            Teams = new BindableCollection<Team>();
            foreach (var team in Database.Database.Teams.OrderBy(x => x.Name))
            {
                if (Settings.DefaultTournament != null && team.Tournament != Settings.DefaultTournament)
                    continue;
                Teams.Add(team);
            }
            NotifyOfPropertyChange(() => Teams);
            NotifyOfPropertyChange(() => TeamsViews);
        }

        public void Handle(string message)
        {
            if (message.ToString() == "DeleteTeam")
                Reload();
        }

        public Tournament FilterTournament
        {
            get { return Settings.DefaultTournament; }
            set
            {
                if (value != Settings.DefaultTournament)
                {
                    Settings.DefaultTournament = value;
                    NotifyOfPropertyChange(() => FilterTournament);
                    Reload();
                }
            }
        }

        private string newTeamName;
        public string NewTeamName
        {
            get { return newTeamName; }
            set
            {
                if (value != newTeamName)
                {
                    newTeamName = value;
                    NotifyOfPropertyChange(() => NewTeamName);
                    NotifyOfPropertyChange(() => NewTeamSaveEnabled);
                }
            }
        }

        private Tournament newTeamTournament;
        public Tournament NewTeamTournament
        {
            get { return newTeamTournament; }
            set
            {
                if (value != newTeamTournament)
                {
                    newTeamTournament = value;
                    NotifyOfPropertyChange(() => NewTeamTournament);
                    NotifyOfPropertyChange(() => NewTeamSaveEnabled);
                }
            }
        }

        public bool NewTeamSaveEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(newTeamName))
                    return false;
                if (Teams.Any(x => x.Name == newTeamName && x.Tournament == newTeamTournament))
                    return false;
                return true;
            }
        }

        public void NewTeamDialogOpened()
        {
            NewTeamName = "";
            NewTeamTournament = Settings.DefaultTournament;
        }

        public void NewTeamDialogClosed()
        {
            var team = new Team(NewTeamTournament, NewTeamName);
            team.Save();
            Settings.DefaultTournament = NewTeamTournament;
            NotifyOfPropertyChange(() => NewTeamTournament);
            Reload();
        }
    }
}
