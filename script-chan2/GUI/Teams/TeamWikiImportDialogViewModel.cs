using AngleSharp;
using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace script_chan2.GUI
{
    public class TeamWikiImportDialogViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<TeamWikiImportDialogViewModel>();

        #region Constructor
        public TeamWikiImportDialogViewModel()
        {
            Tournament = Settings.DefaultTournament;
            WikiUrl = "";
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

        private string wikiUrl;
        public string WikiUrl
        {
            get { return wikiUrl; }
            set
            {
                if (value != wikiUrl)
                {
                    wikiUrl = value;
                    NotifyOfPropertyChange(() => WikiUrl);
                    NotifyOfPropertyChange(() => ImportEnabled);
                }
            }
        }

        public bool ImportEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(WikiUrl))
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
                    NotifyOfPropertyChange(() => WikiUrlEnabled);
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

        public bool WikiUrlEnabled
        {
            get { return !IsImporting; }
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
            ImportStatus = Properties.Resources.TeamWikiImportDialogViewModel_StatusGettingWikiPage;

            var importTeams = new List<ImportTeam>();

            var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            var document = await context.OpenAsync(WikiUrl);
            var participantsHeader = document.QuerySelectorAll("h2").First(x => x.TextContent.StartsWith("Participants"));
            var teamsTable = participantsHeader.NextElementSibling;
            var teams = teamsTable.QuerySelectorAll("tbody tr");
            ImportTeam importTeam = null;
            foreach (var team in teams)
            {
                var isoName = team.Children[1].TextContent;
                switch (isoName)
                {
                    case "Czech Republic": isoName = "Czechia"; break;
                    case "Macau": isoName = "Macao"; break;
                    case "South Korea": isoName = "Korea, Republic of"; break;
                    case "Taiwan": isoName = "Taiwan, Province of China"; break;
                    case "United Kingdom": isoName = "United Kingdom of Great Britain and Northern Ireland"; break;
                    case "United States": isoName = "United States of America"; break;
                    case "Venezuela": isoName = "Venezuela, Bolivarian Republic of"; break;
                    case "Vietnam": isoName = "Viet Nam"; break;
                }
                importTeam = new ImportTeam
                {
                    Name = team.Children[1].TextContent,
                    Country = ISO3166.Country.List.First(x => x.Name == isoName).TwoLetterCode
                };
                importTeams.Add(importTeam);

                var players = team.Children[2].QuerySelectorAll("a");
                foreach (var player in players)
                {
                    var importPlayer = new ImportPlayer()
                    {
                        Id = Convert.ToInt32(player.Attributes["href"].Value.Split('/').Last()),
                        Name = player.TextContent,
                        Country = importTeam.Country
                    };
                    importTeam.Players.Add(importPlayer);
                }
            }

            TeamCount = importTeams.Count;

            for (var i = 0; i < importTeams.Count; i++)
            {
                ImportProgress = i;
                importTeam = importTeams[i];
                ImportStatus = $"{importTeam.Name} ({i + 1}/{TeamCount})";

                if (!Tournament.Teams.Any(x => x.Name == importTeam.Name))
                {
                    var team = new Team()
                    {
                        Name = importTeam.Name,
                        Tournament = Tournament
                    };
                    team.Save();

                    foreach (var playerObject in importTeam.Players)
                    {
                        Player player = new Player()
                        {
                            Id = playerObject.Id,
                            Name = playerObject.Name,
                            Country = playerObject.Country
                        };

                        Database.Database.AddPlayer(player);
                        player = await Database.Database.GetPlayer(playerObject.Name);
                        if (player != null)
                        {
                            team.AddPlayer(player);
                        }
                    }
                }
            }

            localLog.Information("import finished");
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
            public string Country;
            public List<ImportPlayer> Players = new List<ImportPlayer>();
        }

        private class ImportPlayer
        {
            public int Id;
            public string Name;
            public string Country;
        }
    }
}
