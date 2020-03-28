using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class TeamWikiImportDialogViewModel : Screen
    {
        #region Constructor
        public TeamWikiImportDialogViewModel()
        {
            Tournament = Settings.DefaultTournament;
            ImportText = "";
        }
        #endregion

        #region Properties
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
                    NotifyOfPropertyChange(() => ImportEnabled);
                }
            }
        }

        private string importText;
        public string ImportText
        {
            get { return importText; }
            set
            {
                if (value != importText)
                {
                    importText = value;
                    NotifyOfPropertyChange(() => ImportText);
                    NotifyOfPropertyChange(() => ImportEnabled);
                }
            }
        }

        public bool ImportEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(ImportText))
                    return false;
                if (Tournament == null)
                    return false;
                return true;
            }
        }
        #endregion

        #region Actions
        public void StartImport()
        {
            var rootSplit = ImportText.Split('\n');
            foreach (var line in rootSplit)
            {
                var trimmedText = line.Trim();
                var country = trimmedText.Split('\t')[0].Trim();
                var players = trimmedText.Split('\t')[1].Split(',');
                for (var i = 0; i < players.Length; i++)
                {
                    players[i] = players[i].Trim();
                }

                if (!Tournament.Teams.Any(x => x.Name == country))
                {
                    var team = new Team()
                    {
                        Name = country,
                        Tournament = Tournament
                    };
                    team.Save();

                    foreach (var playerName in players)
                    {
                        var player = Database.Database.GetPlayer(playerName);
                        if (player != null)
                        {
                            team.AddPlayer(player);
                        }
                    }
                }
            }
            Events.Aggregator.PublishOnUIThread("AddTeam");
            DialogHost.CloseDialogCommand.Execute(true, null);
        }

        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
