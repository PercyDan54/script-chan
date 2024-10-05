using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class TournamentsBracketJsonImportDialogViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<TournamentsBracketJsonImportDialogViewModel>();

        #region Properties
        private string jsonFilePath;
        public string JsonFilePath
        {
            get { return jsonFilePath; }
            set
            {
                if (value != jsonFilePath)
                {
                    jsonFilePath = value;
                    NotifyOfPropertyChange(() => JsonFilePath);
                    NotifyOfPropertyChange(() => ImportEnabled);
                }
            }
        }

        public bool ImportEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(JsonFilePath))
                    return false;
                return true;
            }
        }
        #endregion

        #region Actions
        public void SelectFile()
        {
            localLog.Information("open json file selection");
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json files (*.json)|*.json";
            if (openFileDialog.ShowDialog() != true)
                return;

            localLog.Information("set json file");
            JsonFilePath = openFileDialog.FileName;
        }

        public async Task StartImport()
        {
            localLog.Information("start import");
            ImportRoot importObject = JsonConvert.DeserializeObject<ImportRoot>(File.ReadAllText(JsonFilePath));

            var tournament = new Tournament();
            tournament.Name = "imported tournament";
            tournament.Acronym = "imp";
            switch (importObject.Ruleset.ShortName)
            {
                case "osu": tournament.GameMode = Enums.GameModes.Standard; break;
                default: tournament.GameMode = Enums.GameModes.Standard; break;
            }
            tournament.TeamSize = importObject.PlayersPerTeam;
            tournament.RoomSize = tournament.TeamSize * 2;
            tournament.TeamMode = Enums.TeamModes.TeamVS;
            tournament.WinCondition = Enums.WinConditions.ScoreV2;
            tournament.PointsForSecondBan = 0;
            tournament.AllPicksFreemod = false;
            tournament.AllPicksNofail = false;
            tournament.MpTimerCommand = Settings.DefaultTimerCommand;
            tournament.MpTimerAfterGame = Settings.DefaultTimerAfterGame;
            tournament.MpTimerAfterPick = Settings.DefaultTimerAfterPick;
            tournament.WelcomeString = "";
            localLog.Information("saving tournament");
            tournament.Save();

            foreach (var teamItem in importObject.Teams)
            {
                var team = new Team();
                team.Tournament = tournament;
                team.Name = teamItem.FullName;
                localLog.Information("saving team '{name}'", team.Name);
                team.Save();

                foreach (var playerItem in teamItem.Players)
                {
                    if (string.IsNullOrEmpty(playerItem.username))
                        continue;

                    var player = new Player
                    {
                        Id = playerItem.id,
                        Name = playerItem.username,
                        Country = playerItem.country_code
                    };

                    localLog.Information("adding player '{player}' to team '{team}'", player.Name, team.Name);
                    Database.Database.AddPlayer(player);
                    player = await Database.Database.GetPlayer(playerItem.username);
                    if (player != null)
                    {
                        team.AddPlayer(player);
                    }
                }
            }

            foreach (var roundItem in importObject.Rounds)
            {
                var mappool = new Mappool();
                mappool.Name = roundItem.Name;
                mappool.Tournament = tournament;
                localLog.Information("saving mappool '{name}'", mappool.Name);
                mappool.Save();

                int beatmapIndex = 0;
                foreach (var beatmapItem in roundItem.Beatmaps)
                {
                    int beatmapSetId = -1;
                    string artist = "";
                    string title = "";
                    string version = "";
                    string creator = "";
                    decimal bpm = 0;
                    decimal ar = 0;
                    decimal cs = 0;

                    if (beatmapItem.BeatmapInfo.Difficulty != null)
                    {
                        artist = beatmapItem.BeatmapInfo.Metadata.Artist;
                        title = beatmapItem.BeatmapInfo.Metadata.Title;
                        version = beatmapItem.BeatmapInfo.DifficultyName;
                        creator = beatmapItem.BeatmapInfo.Metadata.Author.username;
                        bpm = Convert.ToDecimal(beatmapItem.BeatmapInfo.BPM);
                        ar = Convert.ToDecimal(beatmapItem.BeatmapInfo.Difficulty.ApproachRate);
                        cs = Convert.ToDecimal(beatmapItem.BeatmapInfo.Difficulty.CircleSize);
                    }
                    Beatmap beatmap = new Beatmap()
                    {
                        Id = beatmapItem.id,
                        SetId = beatmapSetId,
                        Artist = artist,
                        Title = title,
                        Version = version,
                        Creator = creator,
                        BPM = bpm,
                        AR = ar,
                        CS = cs,
                    };
                    Database.Database.AddBeatmap(beatmap);

                    beatmap = await Database.Database.GetBeatmap(Convert.ToInt32(beatmapItem.id));

                    MappoolMap mappoolMap = new MappoolMap()
                    {
                        Mappool = mappool,
                        Beatmap = beatmap,
                        Tag = "",
                        ListIndex = Convert.ToInt32(beatmapIndex),
                        PickCommand = true,
                        Mods = Utils.ConvertStringtoGameMods(beatmapItem.Mods)
                    };

                    localLog.Information("adding beatmap '{beatmap}' to mappool '{mappool}'", beatmap.Title, mappool.Name);
                    mappool.Beatmaps.Add(mappoolMap);
                    mappoolMap.Save();

                    beatmapIndex++;
                }
            }

            foreach (var matchItem in importObject.Matches)
            {
                if (!string.IsNullOrEmpty(matchItem.Team1Acronym) && !string.IsNullOrEmpty(matchItem.Team2Acronym))
                {
                    var match = new Match();
                    match.Tournament = tournament;
                    match.TeamRed = tournament.Teams.First(x => x.Name == importObject.Teams.First(y => y.Acronym == matchItem.Team1Acronym).FullName);
                    match.TeamBlue = tournament.Teams.First(x => x.Name == importObject.Teams.First(y => y.Acronym == matchItem.Team2Acronym).FullName);
                    match.Name = match.TeamRed.Name + " vs " + match.TeamBlue.Name;
                    match.MatchTime = matchItem.Date;
                    match.GameMode = tournament.GameMode;
                    match.TeamMode = tournament.TeamMode;
                    match.WinCondition = tournament.WinCondition;
                    match.TeamSize = tournament.TeamSize;
                    match.RoomSize = tournament.RoomSize;
                    match.BO = matchItem.PointsToWin * 2 - 1;
                    match.ViewerMode = false;
                    match.MpTimerCommand = tournament.MpTimerCommand;
                    match.MpTimerAfterGame = tournament.MpTimerAfterGame;
                    match.MpTimerAfterPick = tournament.MpTimerAfterPick;
                    match.PointsForSecondBan = tournament.PointsForSecondBan;
                    match.AllPicksFreemod = tournament.AllPicksFreemod;
                    match.AllPicksNofail = tournament.AllPicksNofail;
                    match.WarmupMode = true;

                    foreach (var roundItem in importObject.Rounds)
                    {
                        if (roundItem.Matches.Contains(matchItem.ID))
                        {
                            match.Mappool = tournament.Mappools.First(x => x.Name == roundItem.Name);
                            break;
                        }
                    }

                    localLog.Information("saving match '{name}'", match.Name);
                    match.Save();
                }
            }

            localLog.Information("import finished");
            Events.Aggregator.PublishOnUIThread("AddTournament");
            DialogHost.CloseDialogCommand.Execute(false, null);
        }

        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion

        private class ImportRoot
        {
            public ImportRuleset Ruleset { get; set; }
            public List<ImportRound> Rounds { get; set; }
            public List<ImportTeam> Teams { get; set; }
            public List<ImportMatch> Matches { get; set; }
            public int PlayersPerTeam { get; set; }
        }

        private class ImportRuleset
        {
            public string ShortName { get; set; }
        }

        private class ImportRound
        {
            public string Name { get; set; }
            public List<ImportBeatmap> Beatmaps { get; set; }
            public List<int> Matches { get; set; }
        }

        private class ImportBeatmap
        {
            public int id { get; set; }
            public string Mods { get; set; }
            public ImportBeatmapInfo BeatmapInfo { get; set; }
        }

        private class ImportBeatmapInfo
        {
            public decimal? BPM { get; set; }
            public int? OnlineID { get; set; }
            public int? beatmapset_id { get; set; }
            public ImportMetadata Metadata { get; set; }
            public ImportBaseDifficulty Difficulty { get; set; }
            public string DifficultyName { get; set; }
        }

        private class ImportBeatmapset
        {
            public string Title { get; set; }
            public string Artist { get; set; }
            public string creator { get; set; }
            public int? OnlineBeatmapSetId { get; set; }
            public ImportMetadata Metadata { get; set; }
            public ImportOnlineInfo OnlineInfo { get; set; }
        }

        private class ImportMetadata
        {
            public string Title { get; set; }
            public string Artist { get; set; }
            public string creator { get; set; }
            public ImportPlayer Author { get; set; }
        }

        private class ImportOnlineInfo
        {
            public decimal? BPM { get; set; }
        }

        private class ImportBaseDifficulty
        {
            public decimal? CircleSize { get; set; }
            public decimal? ApproachRate { get; set; }
            public decimal? DrainRate { get; set; }
            public decimal? OverallDifficulty { get; set; }
        }

        private class ImportTeam
        {
            public string FullName { get; set; }
            public string Acronym { get; set; }
            public List<ImportPlayer> Players { get; set; }
        }

        private class ImportPlayer
        {
            public int id { get; set; }
            public string username { get; set; }
            public string country_code { get; set; }
        }

        private class ImportMatch
        {
            public int ID { get; set; }
            public string Team1Acronym { get; set; }
            public string Team2Acronym { get; set; }
            public DateTime Date { get; set; }
            public int PointsToWin { get; set; }
        }
    }
}
