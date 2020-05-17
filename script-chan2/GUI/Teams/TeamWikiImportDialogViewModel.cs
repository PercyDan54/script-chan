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
            ImportStatus = "Getting wiki page";

            var importTeams = new List<ImportTeam>();

            var webClient = new WebClient();
            webClient.Headers.Set(HttpRequestHeader.Accept, "application/json");
            dynamic response = JObject.Parse(await webClient.DownloadStringTaskAsync(WikiUrl));
            MarkdownDocument markdown = Markdown.Parse(response.markdown.Value);

            localLog.Information("search for participants header");
            int participantsHeadingIndex = -1;
            for (var i = 0; i < markdown.Count; i++)
            {
                var block = markdown[i];
                if (block is HeadingBlock)
                {
                    var headingBlock = (HeadingBlock)block;
                    if (headingBlock.Inline.FirstChild.ToString() == "Participants")
                    {
                        localLog.Information("participants header found");
                        participantsHeadingIndex = i;
                        break;
                    }
                }
            }
            if (participantsHeadingIndex == -1)
                return;

            ImportTeam importTeam = null;
            var wrapperBlock = (ParagraphBlock)markdown[participantsHeadingIndex + 1];
            var wrapperInline = wrapperBlock.Inline;
            foreach (var block in wrapperBlock.Inline)
            {
                if (block is LinkInline)
                {
                    var linkBlock = (LinkInline)block;
                    if (linkBlock.Label != null && linkBlock.Label.StartsWith("flag_"))
                    {
                        importTeam = new ImportTeam { Name = linkBlock.Title, Country = linkBlock.Label.Replace("flag_", "") };
                        importTeams.Add(importTeam);
                    }
                    else
                    {
                        var profileUrl = linkBlock.Url;
                        var userId = Convert.ToInt32(profileUrl.Split('/').Last());
                        var username = "";
                        foreach (LiteralInline usernamePart in linkBlock)
                        {
                            username += usernamePart.ToString();
                        }
                        if (importTeam != null)
                        {
                            var importPlayer = new ImportPlayer()
                            {
                                Id = userId,
                                Name = username,
                                Country = importTeam.Country
                            };
                            importTeam.Players.Add(importPlayer);
                        }
                    }
                }
                if (block is EmphasisInline)
                {
                    var emphasisInline = (EmphasisInline)block;
                    if (emphasisInline.FirstChild is LinkInline)
                    {
                        var linkBlock = (LinkInline)emphasisInline.FirstChild;
                        var profileUrl = linkBlock.Url;
                        var userId = Convert.ToInt32(profileUrl.Split('/').Last());
                        var username = "";
                        foreach (LiteralInline usernamePart in linkBlock)
                        {
                            username += usernamePart.ToString();
                        }
                        if (importTeam != null)
                        {
                            var importPlayer = new ImportPlayer()
                            {
                                Id = userId,
                                Name = username,
                                Country = importTeam.Country
                            };
                            importTeam.Players.Add(importPlayer);
                        }
                    }
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
