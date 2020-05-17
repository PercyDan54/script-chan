using Caliburn.Micro;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
            ImportStatus = "Getting wiki page";

            var importMatches = new List<ImportMatch>();

            var webClient = new WebClient();
            webClient.Headers.Set(HttpRequestHeader.Accept, "application/json");
            dynamic response = JObject.Parse(await webClient.DownloadStringTaskAsync(WikiUrl));
            MarkdownDocument markdown = Markdown.Parse(response.markdown.Value);

            localLog.Information("search for match schedule header");
            int matchesHeadingIndex = -1;
            for (var i = 0; i < markdown.Count; i++)
            {
                var block = markdown[i];
                if (block is HeadingBlock)
                {
                    var headingBlock = (HeadingBlock)block;
                    if (headingBlock.Inline.FirstChild.ToString().StartsWith("Match schedule"))
                    {
                        localLog.Information("match schedule header found");
                        matchesHeadingIndex = i;
                        break;
                    }
                }
            }
            if (matchesHeadingIndex == -1)
                return;

            ImportMatch importMatch = null;
            for (var i = matchesHeadingIndex + 1; i < markdown.Count; i++)
            {
                var block = markdown[i];
                if (block is HeadingBlock)
                {
                    var headingBlock = (HeadingBlock)block;
                    // End of matches reached
                    if (headingBlock.Level == 2)
                        break;
                }
                if (block is ParagraphBlock)
                {
                    var paragraphBlock = (ParagraphBlock)block;
                    foreach (var subBlock in paragraphBlock.Inline)
                    {
                        if (subBlock is LinkInline)
                        {
                            if (importMatch == null)
                                importMatch = new ImportMatch() { TeamRed = ((LinkInline)subBlock).Title };
                            else
                            {
                                importMatch.TeamBlue = ((LinkInline)subBlock).Title;
                                importMatches.Add(importMatch);
                                importMatch = null;
                            }
                        }
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
                        BO = Settings.DefaultBO,
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
                        WarmupMode = true
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
        }
    }
}
