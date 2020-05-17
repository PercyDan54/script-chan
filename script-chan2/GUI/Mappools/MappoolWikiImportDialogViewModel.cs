using Caliburn.Micro;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using script_chan2.DataTypes;
using script_chan2.Enums;
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
    public class MappoolWikiImportDialogViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<MappoolWikiImportDialogViewModel>();

        #region Properties
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

        private int mappoolCount;
        public int MappoolCount
        {
            get { return mappoolCount; }
            set
            {
                if (value != mappoolCount)
                {
                    mappoolCount = value;
                    NotifyOfPropertyChange(() => MappoolCount);
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
        public async Task LoadWiki()
        {
            localLog.Information("start import from wiki '{url}'", WikiUrl);
            IsImporting = true;
            ImportStatus = "Getting wiki page";

            var importMappools = new List<ImportMappool>();

            var webClient = new WebClient();
            webClient.Headers.Set(HttpRequestHeader.Accept, "application/json");
            dynamic response = JObject.Parse(await webClient.DownloadStringTaskAsync(WikiUrl));
            MarkdownDocument markdown = Markdown.Parse(response.markdown.Value);

            localLog.Information("search for mappools header");
            int mappoolHeadingIndex = -1;
            for (var i = 0; i < markdown.Count; i++)
            {
                var block = markdown[i];
                if (block is HeadingBlock)
                {
                    var headingBlock = (HeadingBlock)block;
                    if (headingBlock.Inline.FirstChild.ToString() == "Mappools")
                    {
                        localLog.Information("mappools header found");
                        mappoolHeadingIndex = i;
                        break;
                    }
                }
            }
            if (mappoolHeadingIndex == -1)
                return;

            ImportMappool importMappool = null;
            for (var i = mappoolHeadingIndex + 1; i < markdown.Count; i++)
            {
                var block = markdown[i];
                if (block is HeadingBlock)
                {
                    var headingBlock = (HeadingBlock)block;
                    // End of mappools reached
                    if (headingBlock.Level == 2)
                        break;

                    // Mappool name
                    if (headingBlock.Level == 3)
                    {
                        string mappoolName = headingBlock.Inline.FirstChild.ToString();
                        localLog.Information("mappool header '{name}' found", mappoolName);
                        if (Settings.DefaultTournament.Mappools.Any(x => x.Name == mappoolName))
                        {
                            var j = 1;
                            while (Settings.DefaultTournament.Mappools.Any(x => x.Name == mappoolName + " (" + j + ")"))
                                j++;
                            mappoolName = mappoolName + " (" + j + ")";
                            localLog.Information("rename mappool '{oldName}' to '{newName}' because of name collision", headingBlock.Inline.FirstChild.ToString(), mappoolName);
                        }
                        importMappool = new ImportMappool() { Name = mappoolName };
                        importMappools.Add(importMappool);
                    }
                }
                if (block is ListBlock)
                {
                    // Mod categories
                    var listBlock = (ListBlock)block;
                    foreach (ListItemBlock modBlock in listBlock)
                    {
                        var modTitle = ((ParagraphBlock)modBlock[0]).Inline.FirstChild.ToString();
                        localLog.Information("mod header '{mod}' found", modTitle);

                        foreach (ListItemBlock beatmapBlock in (ListBlock)modBlock[1])
                        {
                            var inline = ((ParagraphBlock)beatmapBlock[0]).Inline.FirstChild;
                            var beatmapId = 0;

                            if (inline is EmphasisInline)
                            {
                                var emphasisInline = ((EmphasisInline)inline);
                                inline = emphasisInline.FirstChild;
                            }
                            if (inline is LinkInline)
                            {
                                var beatmapUrl = ((LinkInline)inline).Url;
                                beatmapId = Convert.ToInt32(beatmapUrl.Split('/').Last());
                            }
                            
                            if (beatmapId > 0)
                            {
                                localLog.Information("beatmap '{beatmap}' found", beatmapId);
                                importMappool.Beatmaps.Add(new ImportBeatmap { Id = beatmapId, Mod = modTitle });
                            }
                        }
                    }
                }
            }

            var beatmapsCount = 0;
            foreach (var mappool in importMappools)
            {
                beatmapsCount++;
                beatmapsCount += mappool.Beatmaps.Count;
            }
            MappoolCount = beatmapsCount;

            for (var i = 0; i < importMappools.Count; i++)
            {
                importMappool = importMappools[i];
                ImportStatus = $"{importMappool.Name} ({i + 1}/{importMappools.Count})";

                var mappool = new Mappool();
                mappool.Name = importMappool.Name;
                mappool.Tournament = Settings.DefaultTournament;
                mappool.Save();

                var nomodIndex = 1;
                var hiddenIndex = 1;
                var hardrockIndex = 1;
                var doubletimeIndex = 1;
                var freemodIndex = 1;

                foreach (var importBeatmap in importMappool.Beatmaps)
                {
                    var beatmap = await Database.Database.GetBeatmap(importBeatmap.Id);
                    if (beatmap != null)
                    {
                        var mappoolMap = new MappoolMap()
                        {
                            Mappool = mappool,
                            Beatmap = beatmap
                        };
                        switch (importBeatmap.Mod)
                        {
                            case "NoMod": mappoolMap.Tag = "NM" + nomodIndex; nomodIndex++; break;
                            case "Hidden": mappoolMap.AddMod(GameMods.Hidden); mappoolMap.Tag = "HD" + hiddenIndex; hiddenIndex++; break;
                            case "HardRock": mappoolMap.AddMod(GameMods.HardRock); mappoolMap.Tag = "HR" + hardrockIndex; hardrockIndex++; break;
                            case "DoubleTime": mappoolMap.AddMod(GameMods.DoubleTime); mappoolMap.Tag = "DT" + doubletimeIndex; doubletimeIndex++; break;
                            case "FreeMod": mappoolMap.AddMod(GameMods.Freemod); mappoolMap.Tag = "FM" + freemodIndex; freemodIndex++; break;
                            case "Tiebreaker": mappoolMap.AddMod(GameMods.TieBreaker); mappoolMap.Tag = "TB"; break;
                        }
                        localLog.Information("add beatmap '{beatmap}' to mappool '{mappool}'", importBeatmap.Id, mappool.Name);
                        mappool.AddBeatmap(mappoolMap);
                        mappoolMap.Save();
                    }

                    ImportProgress++;
                }

                ImportProgress++;
            }

            localLog.Information("import finished");
            Events.Aggregator.PublishOnUIThread("AddMappool");
            IsImporting = false;

            DialogHost.CloseDialogCommand.Execute(true, null);
        }

        public void DialogEscape()
        {
            if (!IsImporting)
                DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion

        private class ImportMappool
        {
            public string Name;
            public List<ImportBeatmap> Beatmaps = new List<ImportBeatmap>();
        }

        private class ImportBeatmap
        {
            public int Id;
            public string Mod;
        }
    }
}
