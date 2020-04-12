using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace script_chan2.GUI
{
    public class TeamWikiImportDialogViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<TeamsViewModel>();

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
                if (IsImporting)
                    return false;
                return true;
            }
        }

        private bool isImporting;
        public bool IsImporting
        {
            get { return isImporting; }
            set
            {
                if (value != isImporting)
                {
                    isImporting = value;
                    NotifyOfPropertyChange(() => ImportEnabled);
                    NotifyOfPropertyChange(() => CloseEnabled);
                    NotifyOfPropertyChange(() => ProgressVisible);
                    NotifyOfPropertyChange(() => ImportTextVisible);
                }
            }
        }

        public bool CloseEnabled
        {
            get { return !IsImporting; }
        }

        private int teamCount;
        public int TeamCount
        {
            get { return teamCount; }
            set
            {
                if (value != teamCount)
                {
                    teamCount = value;
                    NotifyOfPropertyChange(() => TeamCount);
                }
            }
        }

        private int importProgress;
        public int ImportProgress
        {
            get { return importProgress; }
            set
            {
                if (value != importProgress)
                {
                    importProgress = value;
                    NotifyOfPropertyChange(() => ImportProgress);
                }
            }
        }

        public Visibility ProgressVisible
        {
            get
            {
                if (IsImporting)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility ImportTextVisible
        {
            get
            {
                if (!IsImporting)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        private string importStatus;
        public string ImportStatus
        {
            get { return importStatus; }
            set
            {
                if (value != importStatus)
                {
                    importStatus = value;
                    NotifyOfPropertyChange(() => ImportStatus);
                }
            }
        }
        #endregion

        #region Actions
        public async Task StartImport()
        {
            localLog.Information("start import");
            IsImporting = true;
            ImportStatus = "Parsing text";

            var importTeams = new List<ImportTeam>();

            var rootSplit = ImportText.Split('\n');
            foreach (var line in rootSplit)
            {
                var trimmedText = line.Trim();

                var team = new ImportTeam { Name = trimmedText.Split('\t')[0].Trim() };

                foreach (var player in trimmedText.Split('\t')[1].Split(','))
                {
                    team.Players.Add(player.Trim());
                }

                importTeams.Add(team);
            }

            TeamCount = importTeams.Count;

            for (var i = 0; i < importTeams.Count; i++)
            {
                ImportProgress = i;
                var importTeam = importTeams[i];
                ImportStatus = $"{importTeam.Name} ({i + 1}/{TeamCount})";

                if (!Tournament.Teams.Any(x => x.Name == importTeam.Name))
                {
                    var team = new Team()
                    {
                        Name = importTeam.Name,
                        Tournament = Tournament
                    };
                    team.Save();

                    foreach (var playerName in importTeam.Players)
                    {
                        var player = await Database.Database.GetPlayer(playerName);
                        if (player != null)
                        {
                            team.AddPlayer(player);
                        }
                    }
                }
            }

            Events.Aggregator.PublishOnUIThread("AddTeam");
            IsImporting = false;

            DialogHost.CloseDialogCommand.Execute(true, null);
        }

        public void DialogEscape()
        {
            if (!IsImporting)
                DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion

        private class ImportTeam
        {
            public string Name;
            public List<string> Players = new List<string>();
        }
    }
}
