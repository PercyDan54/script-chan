using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
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
            dynamic importObject = JObject.Parse(File.ReadAllText(JsonFilePath));

            var tournament = new Tournament();
            tournament.Name = "imported tournament";
            tournament.Acronym = "imp";
            switch (importObject.Ruleset.ShortName.Value)
            {
                case "osu": tournament.GameMode = Enums.GameModes.Standard; break;
                default: tournament.GameMode = Enums.GameModes.Standard; break;
            }
            tournament.TeamSize = Convert.ToInt32(importObject.PlayersPerTeam.Value);
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
            tournament.Save();

            foreach (var teamItem in importObject.Teams)
            {
                var team = new Team();
                team.Tournament = tournament;
                team.Name = teamItem.Acronym.Value;
                team.Save();
            }

            foreach (var roundItem in importObject.Rounds)
            {
                var mappool = new Mappool();
                mappool.Name = roundItem.Name.Value;
                mappool.Tournament = tournament;
                mappool.Save();

                int beatmapIndex = 0;
                foreach (var beatmapItem in roundItem.Beatmaps)
                {
                    Beatmap beatmap = new Beatmap()
                    {
                        Id = Convert.ToInt32(beatmapItem.ID.Value),
                        SetId = Convert.ToInt32(beatmapItem.BeatmapInfo.beatmapset_id.Value),
                        Artist = beatmapItem.BeatmapInfo.beatmapset.Artist.Value,
                        Title = beatmapItem.BeatmapInfo.beatmapset.Title.Value,
                        Version = beatmapItem.BeatmapInfo.version.Value,
                        Creator = beatmapItem.BeatmapInfo.beatmapset.creator.Value,
                        BPM = Convert.ToDecimal(beatmapItem.BeatmapInfo.beatmapset.bpm.Value),
                        AR = Convert.ToDecimal(beatmapItem.BeatmapInfo.ar.Value),
                        CS = Convert.ToDecimal(beatmapItem.BeatmapInfo.cs.Value)
                    };
                    Database.Database.AddBeatmap(beatmap);

                    beatmap = await Database.Database.GetBeatmap(Convert.ToInt32(beatmapItem.ID.Value));

                    MappoolMap mappoolMap = new MappoolMap()
                    {
                        Mappool = mappool,
                        Beatmap = beatmap,
                        Tag = "",
                        ListIndex = Convert.ToInt32(beatmapIndex),
                        PickCommand = true,
                        Mods = Utils.ConvertStringtoGameMods(beatmapItem.Mods.Value)
                    };

                    mappool.Beatmaps.Add(mappoolMap);
                    mappoolMap.Save();

                    beatmapIndex++;
                }
            }

            foreach (var matchItem in importObject.Matches)
            {
                if (matchItem["Team1Acronym"] != null && tournament.Teams.Any(x => x.Name == matchItem.Team1Acronym.Value) && matchItem["Team2Acronym"] != null && tournament.Teams.Any(x => x.Name == matchItem.Team2Acronym.Value))
                {
                    var match = new Match();
                    match.Tournament = tournament;
                    match.TeamRed = tournament.Teams.First(x => x.Name == matchItem.Team1Acronym.Value);
                    match.TeamBlue = tournament.Teams.First(x => x.Name == matchItem.Team2Acronym.Value);
                    match.Name = match.TeamRed.Name + " vs " + match.TeamBlue.Name;
                    match.MatchTime = Convert.ToDateTime(matchItem.Date.Value);
                    match.GameMode = tournament.GameMode;
                    match.TeamMode = tournament.TeamMode;
                    match.WinCondition = tournament.WinCondition;
                    match.TeamSize = tournament.TeamSize;
                    match.RoomSize = tournament.RoomSize;
                    match.BO = Convert.ToInt32(matchItem.PointsToWin.Value) * 2 - 1;
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
                        foreach (int roundMatchItem in roundItem.Matches)
                        {
                            if (roundMatchItem == Convert.ToInt32(matchItem.ID.Value))
                            {
                                match.Mappool = tournament.Mappools.First(x => x.Name == roundItem.Name.Value);
                                goto MappoolFound;
                            }
                        }
                    }
                    MappoolFound:

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
    }
}
