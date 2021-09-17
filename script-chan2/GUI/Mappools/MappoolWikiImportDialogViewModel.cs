using AngleSharp;
using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using script_chan2.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
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
            ImportStatus = Properties.Resources.MappoolWikiImportDialogViewModel_StatusGettingWikiPage;

            var importMappools = new List<ImportMappool>();

            localLog.Information("search for mappools header");
            var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            var document = await context.OpenAsync(WikiUrl);
            var currentElement = document.QuerySelectorAll("h2").First(x => x.TextContent.StartsWith("Mappools"));

            ImportMappool importMappool = null;
            string mappoolName = "";
            while (true)
            {
                currentElement = currentElement.NextElementSibling;

                // End of mappools reached
                if (currentElement.TagName == "H2")
                    break;

                // Name header
                if (currentElement.TagName == "H3")
                {
                    mappoolName = currentElement.TextContent;
                    localLog.Information("mappool header '{name}' found", mappoolName);
                    if (Settings.DefaultTournament.Mappools.Any(x => x.Name == mappoolName))
                    {
                        var j = 1;
                        while (Settings.DefaultTournament.Mappools.Any(x => x.Name == mappoolName + " (" + j + ")"))
                            j++;
                        mappoolName = mappoolName + " (" + j + ")";
                        localLog.Information("rename mappool '{oldName}' to '{newName}' because of name collision", currentElement.TextContent, mappoolName);
                    }

                    importMappool = new ImportMappool() { Name = mappoolName };
                    importMappools.Add(importMappool);
                }

                // Map list
                if (currentElement.TagName == "UL")
                {
                    // List with mod headers
                    if (currentElement.QuerySelector("ol") != null)
                    {
                        foreach (var modList in currentElement.QuerySelectorAll(":scope > li > div"))
                        {
                            var modName = modList.TextContent.Split('\n').First();
                            localLog.Information("mod header '{mod}' found", modName);

                            foreach (var map in modList.QuerySelectorAll("a"))
                            {
                                var beatmapUrl = map.Attributes["href"].Value;
                                var beatmapId = Convert.ToInt32(beatmapUrl.Split('/').Last());
                                localLog.Information("beatmap '{beatmap}' found", beatmapId);
                                importMappool.Beatmaps.Add(new ImportBeatmap { Id = beatmapId, Mod = modName });
                            }
                        }
                    }
                    // No mod headers
                    else
                    {
                        foreach (var map in currentElement.QuerySelectorAll("a"))
                        {
                            var beatmapUrl = map.Attributes["href"].Value;
                            var beatmapId = Convert.ToInt32(beatmapUrl.Split('/').Last());
                            localLog.Information("beatmap '{beatmap}' found", beatmapId);
                            importMappool.Beatmaps.Add(new ImportBeatmap { Id = beatmapId, Mod = "freemod" });
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

                var modIndexes = new Dictionary<string, int>();

                foreach (var importBeatmap in importMappool.Beatmaps)
                {
                    var beatmap = await Database.Database.GetBeatmap(importBeatmap.Id);
                    if (beatmap != null)
                    {
                        var mappoolMap = new MappoolMap()
                        {
                            Mappool = mappool,
                            Beatmap = beatmap,
                            PickCommand = true
                        };
                        var mappoolTag = importBeatmap.Mod;
                        switch (importBeatmap.Mod.ToLower())
                        {
                            case "nomod": mappoolTag = "NM"; break;
                            case "hidden": mappoolMap.AddMod(GameMods.Hidden); mappoolTag = "HD"; break;
                            case "hardrock": mappoolMap.AddMod(GameMods.HardRock); mappoolTag = "HR"; break;
                            case "doubletime": mappoolMap.AddMod(GameMods.DoubleTime); mappoolTag = "DT"; break;
                            case "freemod": mappoolMap.AddMod(GameMods.Freemod); mappoolTag = "FM"; break;
                            case "tiebreaker:":
                            case "tiebreaker": mappoolMap.AddMod(GameMods.TieBreaker); mappoolTag = "TB"; break;
                            case "rice": mappoolMap.AddMod(GameMods.Freemod); mappoolTag = "RC"; break;
                            case "hybrid": mappoolMap.AddMod(GameMods.Freemod); mappoolTag = "HB"; break;
                            default: mappoolMap.AddMod(GameMods.Freemod); break;
                        }
                        if (modIndexes.ContainsKey(mappoolTag))
                        {
                            modIndexes[mappoolTag]++;
                            mappoolMap.Tag = mappoolTag + modIndexes[mappoolTag].ToString();
                        }
                        else
                        {
                            modIndexes.Add(mappoolTag, 1);
                            mappoolMap.Tag = mappoolTag + "1";
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
