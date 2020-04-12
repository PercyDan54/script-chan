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
                }
            }
        }
        #endregion

        #region Actions
        public async Task LoadWiki()
        {
            localLog.Information("start import from wiki '{url}'", WikiUrl);
            var webClient = new WebClient();
            webClient.Headers.Set(HttpRequestHeader.Accept, "application/json");
            dynamic response = JObject.Parse(webClient.DownloadString(WikiUrl));
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

            Mappool mappool = null;
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
                        mappool = new Mappool();
                        mappool.Name = mappoolName;
                        mappool.Tournament = Settings.DefaultTournament;
                        mappool.Save();
                    }
                }
                if (block is ListBlock)
                {
                    // Mod categories
                    var listBlock = (ListBlock)block;

                    var nomodIndex = 1;
                    var hiddenIndex = 1;
                    var hardrockIndex = 1;
                    var doubletimeIndex = 1;
                    var freemodIndex = 1;
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
                                var beatmap = await Database.Database.GetBeatmap(beatmapId);
                                if (beatmap != null)
                                {
                                    var mappoolMap = new MappoolMap()
                                    {
                                        Mappool = mappool,
                                        Beatmap = beatmap
                                    };
                                    switch (modTitle)
                                    {
                                        case "NoMod": mappoolMap.Tag = "NM" + nomodIndex; nomodIndex++; break;
                                        case "Hidden": mappoolMap.AddMod(Enums.GameMods.Hidden); mappoolMap.Tag = "HD" + hiddenIndex; hiddenIndex++; break;
                                        case "HardRock": mappoolMap.AddMod(Enums.GameMods.HardRock); mappoolMap.Tag = "HR" + hardrockIndex; hardrockIndex++; break;
                                        case "DoubleTime": mappoolMap.AddMod(Enums.GameMods.DoubleTime); mappoolMap.Tag = "DT" + doubletimeIndex; doubletimeIndex++; break;
                                        case "FreeMod": mappoolMap.AddMod(Enums.GameMods.Freemod); mappoolMap.Tag = "FM" + freemodIndex; freemodIndex++; break;
                                        case "Tiebreaker": mappoolMap.AddMod(Enums.GameMods.TieBreaker); mappoolMap.Tag = "TB"; break;
                                    }
                                    localLog.Information("add beatmap '{beatmap}' to mappool '{mappool}'", beatmapId, mappool.Name);
                                    mappool.AddBeatmap(mappoolMap);
                                    mappoolMap.Save();
                                }
                            }
                        }
                    }
                }
            }

            localLog.Information("import finished");
            Events.Aggregator.PublishOnUIThread("AddMappool");
            DialogHost.CloseDialogCommand.Execute(true, null);
        }

        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
