using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using System.Linq;

namespace script_chan2.GUI
{
    public class EditTeamDialogViewModel : Screen
    {
        #region Constructor
        public EditTeamDialogViewModel(int id = 0)
        {
            this.id = id;
            if (id > 0)
            {
                var team = Database.Database.Teams.First(x => x.Id == id);
                Name = team.Name;
                Tournament = team.Tournament;
            }
            else
            {
                Name = "";
                Tournament = Settings.DefaultTournament;
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
                foreach (var tournament in Database.Database.Tournaments.OrderBy(x => x.Name))
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
                    NotifyOfPropertyChange(() => Tournament);
                    NotifyOfPropertyChange(() => SaveEnabled);
                }
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
                if (Database.Database.Teams.Any(x => x.Name == Name && x.Tournament == Tournament && x.Id != id))
                    return false;
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
