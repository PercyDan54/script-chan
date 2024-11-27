using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using System.Linq;

namespace script_chan2.GUI
{
    public class EditCustomCommandDialogViewModel : Screen
    {
        #region Constructor
        public EditCustomCommandDialogViewModel(int id = 0)
        {
            this.id = id;
            if (id > 0)
            {
                var customCommand = Database.Database.CustomCommands.First(x => x.Id == id);
                Name = customCommand.Name;
                Tournament = customCommand.Tournament;
                Command = customCommand.Command;
            }
            else
            {
                Name = string.Empty;
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

        private string command;
        public string Command
        {
            get { return command; }
            set
            {
                if (value != command)
                {
                    command = value;
                    NotifyOfPropertyChange(() => Command);
                    NotifyOfPropertyChange(() => SaveEnabled);
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
                if (string.IsNullOrEmpty(Command))
                    return false;
                if (Database.Database.CustomCommands.Any(x => x.Name == Name && x.Id != id))
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
