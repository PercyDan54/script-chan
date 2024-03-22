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
    public class MatchesWikiImportDialogViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<MatchesWikiImportDialogViewModel>();

        #region Constructor
        public MatchesWikiImportDialogViewModel()
        {
            Tournament = Settings.DefaultTournament;
            WikiUrl = "";
            BO = Settings.DefaultBO;
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
                    NotifyOfPropertyChange(() => NoTeamsWarningVisible);
                }
            }
        }

        public BindableCollection<Mappool> Mappools
        {
            get
            {
                var list = new BindableCollection<Mappool>();
                foreach (var mappool in Database.Database.Mappools.Where(x => x.Tournament == Tournament).OrderBy(x => x.Name))
                    list.Add(mappool);
                return list;
            }
        }

        private Mappool mappool;
        public Mappool Mappool
        {
            get { return mappool; }
            set
            {
                if (value != mappool)
                {
                    mappool = value;
                    NotifyOfPropertyChange(() => Mappool);
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

        private int bo;
        public int BO
        {
            get { return bo; }
            set
            {
                if (value != bo)
                {
                    bo = value;
                    NotifyOfPropertyChange(() => BO);
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
                if (BO <= 0)
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

        private int matchCount;
        public int MatchCount
        {
            get { return matchCount; }
            set
            {
                if (value != matchCount)
                {
                    matchCount = value;
                    NotifyOfPropertyChange(() => MatchCount);
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

        public Visibility NoTeamsWarningVisible
        {
            get
            {
                if (Tournament.Teams.Count > 0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
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
            ImportStatus = Properties.Resources.MatchesWikiImportDialogViewModel_StatusGettingWikiPage;

            var importMatches = new List<ImportMatch>();

            var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            var document = await context.OpenAsync(WikiUrl);
            var currentElement = document.QuerySelectorAll("h2").First(x => x.TextContent.StartsWith("Match schedule", StringComparison.CurrentCultureIgnoreCase));

            ImportMatch importMatch = null;
            string date = "";
            while (true)
            {
                currentElement = currentElement.NextElementSibling;

                // End of matches reached
                if (currentElement.TagName != "H3" && currentElement.TagName != "DIV")
                    break;

                // Date header
                if (currentElement.TagName == "H3")
                {
                    date = currentElement.TextContent; 
                }

                // Match table
                if (currentElement.TagName == "DIV")
                {
                    var matches = currentElement.QuerySelectorAll("tbody tr");
                    foreach (var match in matches)
                    {
                        importMatch = new ImportMatch
                        {
                            TeamRed = match.Children[0].TextContent.Trim(' '),
                            TeamBlue = match.Children[1].TextContent.Trim(' '),
                            MatchTime = DateTime.Parse(date + " " + match.Children[2].TextContent.Split(' ')[3])
                        };
                        importMatches.Add(importMatch);
                    }
                }
            }

            MatchCount = importMatches.Count;

            for (var i = 0; i < importMatches.Count; i++)
            {
                ImportProgress = i;
                importMatch = importMatches[i];
                ImportStatus = $"{importMatch.TeamRed} vs {importMatch.TeamBlue} ({i + 1}/{MatchCount})";

                if (Tournament.Teams.Any(x => x.Name == importMatch.TeamRed) && Tournament.Teams.Any(x => x.Name == importMatch.TeamBlue))
                {
                    var match = new Match()
                    {
                        Name = $"{Tournament.Acronym}: ({importMatch.TeamRed}) VS ({importMatch.TeamBlue})",
                        Tournament = Tournament,
                        Mappool = Mappool,
                        TeamRed = Tournament.Teams.First(x => x.Name == importMatch.TeamRed),
                        TeamBlue = Tournament.Teams.First(x => x.Name == importMatch.TeamBlue),
                        BO = BO,
                        GameMode = Tournament.GameMode,
                        TeamMode = Tournament.TeamMode,
                        WinCondition = Tournament.WinCondition,
                        RoomSize = Tournament.RoomSize,
                        TeamSize = Tournament.TeamSize,
                        MpTimerCommand = Tournament.MpTimerCommand,
                        MpTimerAfterGame = Tournament.MpTimerAfterGame,
                        MpTimerAfterPick = Tournament.MpTimerAfterPick,
                        PointsForSecondBan = Tournament.PointsForSecondBan,
                        AllPicksFreemod = Tournament.AllPicksFreemod,
                        AllPicksNofail = Tournament.AllPicksNofail,
                        WarmupMode = true,
                        MatchTime = importMatch.MatchTime
                    };
                    match.Save();
                }
            }

            Events.Aggregator.PublishOnUIThread("AddMatch");
            IsImporting = false;

            DialogHost.CloseDialogCommand.Execute(true, null);
        }

        public void DialogEscape()
        {
            if (!IsImporting)
                DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion

        private class ImportMatch
        {
            public string TeamBlue;
            public string TeamRed;
            public DateTime MatchTime;
        }
    }
}
