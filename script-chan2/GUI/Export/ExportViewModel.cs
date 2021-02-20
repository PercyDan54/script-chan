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

namespace script_chan2.GUI
{
    public class ExportViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<ExportViewModel>();

        #region Constructor
        protected override void OnActivate()
        {
            localLog.Information("init tree view");
            ExportItems = new BindableCollection<ExportItem>();
            foreach (var tournament in Database.Database.Tournaments)
            {
                var tournamentItem = new ExportItem { Id = tournament.Id, Name = tournament.Name };
                var teamsCategory = new ExportCategory { Name = "Teams", Parent = tournamentItem };
                foreach (var team in tournament.Teams)
                {
                    var teamItem = new ExportItem { Id = team.Id, Name = team.Name, ParentItem = teamsCategory };
                    teamsCategory.ExportItems.Add(teamItem);
                }
                tournamentItem.ExportCategories.Add(teamsCategory);

                var webhooksCategory = new ExportCategory { Name = "Webhooks", Parent = tournamentItem };
                foreach (var webhook in tournament.Webhooks)
                {
                    var webhookItem = new ExportItem { Id = webhook.Id, Name = webhook.Name, ParentItem = webhooksCategory };
                    webhooksCategory.ExportItems.Add(webhookItem);
                }
                tournamentItem.ExportCategories.Add(webhooksCategory);

                var mappoolsCategory = new ExportCategory { Name = "Mappools", Parent = tournamentItem };
                foreach (var mappool in tournament.Mappools)
                {
                    var mappoolItem = new ExportItem { Id = mappool.Id, Name = mappool.Name, ParentItem = mappoolsCategory };
                    mappoolsCategory.ExportItems.Add(mappoolItem);
                }
                tournamentItem.ExportCategories.Add(mappoolsCategory);

                var matchesCategory = new ExportCategory { Name = "Matches", Parent = tournamentItem };
                foreach (var match in Database.Database.Matches.Where(x => x.Tournament == tournament))
                {
                    var matchItem = new ExportItem { Id = match.Id, Name = match.Name, ParentItem = matchesCategory };
                    matchesCategory.ExportItems.Add(matchItem);
                }
                tournamentItem.ExportCategories.Add(matchesCategory);

                ExportItems.Add(tournamentItem);
            }
            NotifyOfPropertyChange(() => ExportItems);
            localLog.Information("tree view initialized");
        }
        #endregion

        #region Properties
        public BindableCollection<ExportItem> ExportItems { get; set; }
        #endregion

        #region Actions
        public async void Export()
        {
            localLog.Information("start export");
            var anythingToSave = false;
            foreach (var tournament in ExportItems)
            {
                if (tournament.Export)
                {
                    anythingToSave = true;
                    break;
                }
            }
            if (!anythingToSave)
                return;

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "json file (*.json)|*.json";
            if (saveFileDialog.ShowDialog() != true)
                return;
            var filePath = saveFileDialog.FileName;
            localLog.Information("export to '{file}'", filePath);

            var tournaments = new List<object>();
            foreach (var tournamentItem in ExportItems.Where(x => x.Export))
            {
                var tournamentData = Database.Database.Tournaments.First(x => x.Id == tournamentItem.Id);
                var tournamentObject = new
                {
                    Name = tournamentData.Name,
                    GameMode = tournamentData.GameMode.ToString(),
                    TeamMode = tournamentData.TeamMode.ToString(),
                    WinCondition = tournamentData.WinCondition.ToString(),
                    Acronym = tournamentData.Acronym,
                    TeamSize = tournamentData.TeamSize,
                    RoomSize = tournamentData.RoomSize,
                    PointsForSecondBan = tournamentData.PointsForSecondBan,
                    AllPicksFreemod = tournamentData.AllPicksFreemod,
                    MpTimerCommand = tournamentData.MpTimerCommand,
                    MpTimerAfterGame = tournamentData.MpTimerAfterGame,
                    MpTimerAfterPick = tournamentData.MpTimerAfterPick,
                    WelcomeString = tournamentData.WelcomeString,
                    Teams = new List<object>(),
                    Webhooks = new List<object>(),
                    Mappools = new List<object>(),
                    Matches = new List<object>(),
                    Head2HeadPoints = new List<object>()
                };

                if (tournamentData.TeamMode == Enums.TeamModes.HeadToHead)
                {
                    foreach (var head2headPoint in tournamentData.HeadToHeadPoints)
                    {
                        var head2headPointObject = new
                        {
                            Place = head2headPoint.Key,
                            Points = head2headPoint.Value
                        };

                        tournamentObject.Head2HeadPoints.Add(head2headPointObject);
                    }
                }

                foreach (var teamItem in tournamentItem.ExportCategories.First(x => x.Name == "Teams").ExportItems.Where(x => x.Export))
                {
                    var teamData = tournamentData.Teams.First(x => x.Id == teamItem.Id);
                    var teamObject = new
                    {
                        Name = teamData.Name,
                        Players = new List<object>()
                    };

                    foreach (var player in teamData.Players)
                    {
                        var playerObject = new
                        {
                            Id = player.Id,
                            Name = player.Name,
                            Country = player.Country
                        };

                        teamObject.Players.Add(playerObject);
                    }

                    tournamentObject.Teams.Add(teamObject);
                }

                foreach (var webhookItem in tournamentItem.ExportCategories.First(x => x.Name == "Webhooks").ExportItems.Where(x => x.Export))
                {
                    var webhookData = tournamentData.Webhooks.First(x => x.Id == webhookItem.Id);
                    var webhookObject = new
                    {
                        Name = webhookData.Name,
                        URL = webhookData.URL,
                        MatchCreated = webhookData.MatchCreated,
                        BanRecap = webhookData.BanRecap,
                        PickRecap = webhookData.PickRecap,
                        GameRecap = webhookData.GameRecap,
                        FooterText = webhookData.FooterText,
                        FooterIcon = webhookData.FooterIcon,
                        WinImage = webhookData.WinImage,
                        Username = webhookData.Username,
                        Avatar = webhookData.Avatar
                    };

                    tournamentObject.Webhooks.Add(webhookObject);
                }

                foreach (var mappoolItem in tournamentItem.ExportCategories.First(x => x.Name == "Mappools").ExportItems.Where(x => x.Export))
                {
                    var mappoolData = tournamentData.Mappools.First(x => x.Id == mappoolItem.Id);
                    var mappoolObject = new
                    {
                        Name = mappoolData.Name,
                        Maps = new List<object>()
                    };

                    foreach (var mappoolMap in mappoolData.Beatmaps)
                    {
                        var mappoolMapObject = new
                        {
                            Beatmap = new
                            {
                                Id = mappoolMap.Beatmap.Id,
                                SetId = mappoolMap.Beatmap.SetId,
                                Artist = mappoolMap.Beatmap.Artist,
                                Title = mappoolMap.Beatmap.Title,
                                Version = mappoolMap.Beatmap.Version,
                                Creator = mappoolMap.Beatmap.Creator,
                                BPM = mappoolMap.Beatmap.BPM,
                                AR = mappoolMap.Beatmap.AR,
                                CS = mappoolMap.Beatmap.CS
                            },
                            Tag = mappoolMap.Tag,
                            ListIndex = mappoolMap.ListIndex,
                            Mods = new List<string>()
                        };

                        foreach (var mod in mappoolMap.Mods)
                        {
                            mappoolMapObject.Mods.Add(mod.ToString());
                        }

                        mappoolObject.Maps.Add(mappoolMapObject);
                    }

                    tournamentObject.Mappools.Add(mappoolObject);
                }

                foreach (var matchItem in tournamentItem.ExportCategories.First(x => x.Name == "Matches").ExportItems.Where(x => x.Export))
                {
                    var matchData = Database.Database.Matches.First(x => x.Id == matchItem.Id);
                    var matchObject = new
                    {
                        Name = matchData.Name,
                        Mappool = matchData.Mappool != null ? matchData.Mappool.Name : "",
                        GameMode = matchData.GameMode.ToString(),
                        TeamMode = matchData.TeamMode.ToString(),
                        WinCondition = matchData.WinCondition.ToString(),
                        TeamBlue = matchData.TeamBlue != null ? matchData.TeamBlue.Name : "",
                        TeamRed = matchData.TeamRed != null ? matchData.TeamRed.Name : "",
                        TeamSize = matchData.TeamSize,
                        RoomSize = matchData.RoomSize,
                        RollWinnerTeam = matchData.RollWinnerTeam != null ? matchData.RollWinnerTeam.Name : "",
                        RollWinnerPlayer = matchData.RollWinnerPlayer != null ? matchData.RollWinnerPlayer.Name : "",
                        FirstPickerTeam = matchData.FirstPickerTeam != null ? matchData.FirstPickerTeam.Name : "",
                        FirstPickerPlayer = matchData.FirstPickerPlayer != null ? matchData.FirstPickerPlayer.Name : "",
                        BO = matchData.BO,
                        ViewerMode = matchData.ViewerMode,
                        MpTimerCommand = matchData.MpTimerCommand,
                        MpTimerAfterGame = matchData.MpTimerAfterGame,
                        MpTimerAfterPick = matchData.MpTimerAfterPick,
                        PointsForSecondBan = matchData.PointsForSecondBan,
                        AllPicksFreemod = matchData.AllPicksFreemod,
                        WarmupMode = matchData.WarmupMode,
                        MatchTime = matchData.MatchTime,
                        Players = new List<object>()
                    };

                    foreach (var player in matchData.Players)
                    {
                        var playerObject = new
                        {
                            Id = player.Key.Id,
                            Name = player.Key.Name,
                            Country = player.Key.Country,
                            Points = player.Value
                        };

                        matchObject.Players.Add(playerObject);
                    }

                    tournamentObject.Matches.Add(matchObject);
                }

                tournaments.Add(tournamentObject);
            }

            var jObject = JArray.FromObject(tournaments);
            File.WriteAllText(filePath, jObject.ToString());
            localLog.Information("export finished");

            var model = new ExportSuccessDialogViewModel("Export successfull");
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);
            await DialogHost.Show(view, "MainDialogHost");
        }

        public async void Import()
        {
            localLog.Information("start import");
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "json file (*.json)|*.json";
            if (openFileDialog.ShowDialog() != true)
                return;
            var filePath = openFileDialog.FileName;
            localLog.Information("import from '{file}'", filePath);

            dynamic importOject = JArray.Parse(File.ReadAllText(filePath));

            foreach (var tournamentItem in importOject)
            {
                Tournament tournament = Database.Database.Tournaments.FirstOrDefault(x => x.Name == tournamentItem.Name.Value);
                if (tournament == null)
                {
                    tournament = new Tournament();
                    localLog.Information("import new tournament '{tournament}'", tournamentItem.Name.Value);
                }
                else
                {
                    localLog.Information("update existing tournament '{tournament}'", tournamentItem.Name.Value);
                }

                tournament.Name = tournamentItem.Name.Value;
                if (Enum.TryParse(tournamentItem.GameMode.Value, out Enums.GameModes gameMode))
                    tournament.GameMode = gameMode;
                else
                    tournament.GameMode = Enums.GameModes.Standard;
                if (Enum.TryParse(tournamentItem.TeamMode.Value, out Enums.TeamModes teamMode))
                    tournament.TeamMode = teamMode;
                else
                    tournament.TeamMode = Enums.TeamModes.TeamVS;
                if (Enum.TryParse(tournamentItem.WinCondition.Value, out Enums.WinConditions winCondition))
                    tournament.WinCondition = winCondition;
                else
                    tournament.WinCondition = Enums.WinConditions.ScoreV2;
                tournament.Acronym = tournamentItem.Acronym.Value;
                tournament.TeamSize = Convert.ToInt32(tournamentItem.TeamSize.Value);
                tournament.RoomSize = Convert.ToInt32(tournamentItem.RoomSize.Value);
                tournament.PointsForSecondBan = Convert.ToInt32(tournamentItem.PointsForSecondBan.Value);
                tournament.AllPicksFreemod = tournamentItem.AllPicksFreemod.Value;
                tournament.MpTimerCommand = Convert.ToInt32(tournamentItem.MpTimerCommand.Value);
                tournament.MpTimerAfterGame = Convert.ToInt32(tournamentItem.MpTimerAfterGame.Value);
                tournament.MpTimerAfterPick = Convert.ToInt32(tournamentItem.MpTimerAfterPick.Value);
                tournament.WelcomeString = tournamentItem.WelcomeString.Value;

                tournament.HeadToHeadPoints.Clear();
                foreach (var head2headPointItem in tournamentItem.Head2HeadPoints)
                {
                    tournament.HeadToHeadPoints.Add(Convert.ToInt32(head2headPointItem.Place.Value), Convert.ToInt32(head2headPointItem.Points.Value));
                }

                tournament.Save();

                foreach (var teamItem in tournamentItem.Teams)
                {
                    Team team = tournament.Teams.FirstOrDefault(x => x.Name == teamItem.Name.Value);
                    if (team == null)
                    {
                        team = new Team() { Tournament = tournament };
                        localLog.Information("import new team '{team}' in tournament '{tournament}'", teamItem.Name.Value, tournament.Name);
                    }
                    else
                    {
                        localLog.Information("update existing team '{team}' in tournament '{tournament}'", teamItem.Name.Value, tournament.Name);
                    }

                    team.Name = teamItem.Name.Value;
                    team.Save();

                    foreach (var playerItem in teamItem.Players)
                    {
                        Player player = new Player()
                        {
                            Id = Convert.ToInt32(playerItem.Id.Value),
                            Name = playerItem.Name.Value,
                            Country = playerItem.Country.Value
                        };
                        Database.Database.AddPlayer(player);

                        player = await Database.Database.GetPlayer(playerItem.Name.Value);
                        team.AddPlayer(player);
                    }
                }

                foreach (var webhookItem in tournamentItem.Webhooks)
                {
                    Webhook webhook = Database.Database.Webhooks.FirstOrDefault(x => x.Name == webhookItem.Name.Value);
                    if (webhook == null)
                    {
                        webhook = new Webhook();
                        localLog.Information("import new webhook '{webhook}' in tournament '{tournament}'", webhookItem.Name.Value, tournament.Name);
                    }
                    else
                    {
                        localLog.Information("update existing webhook '{webhook}' in tournament '{tournament}'", webhookItem.Name.Value, tournament.Name);
                    }

                    webhook.Name = webhookItem.Name.Value;
                    webhook.URL = webhookItem.URL.Value;
                    webhook.MatchCreated = webhookItem.MatchCreated.Value;
                    webhook.BanRecap = webhookItem.BanRecap.Value;
                    webhook.PickRecap = webhookItem.PickRecap.Value;
                    webhook.GameRecap = webhookItem.GameRecap.Value;
                    webhook.FooterText = webhookItem.FooterText.Value;
                    webhook.FooterIcon = webhookItem.FooterIcon.Value;
                    webhook.WinImage = webhookItem.WinImage.Value;
                    webhook.Username = webhookItem.Username.Value;
                    webhook.Avatar = webhookItem.Avatar.Value;
                    webhook.Save();

                    tournament.AddWebhook(webhook);
                }

                foreach (var mappoolItem in tournamentItem.Mappools)
                {
                    localLog.Information("import mappool '{mappool}' in tournament '{tournament}'", mappoolItem.Name.Value, tournament.Name);
                    string mappoolName = mappoolItem.Name.Value;
                    if (tournament.Mappools.Any(x => x.Name == mappoolName))
                    {
                        var i = 1;
                        while (tournament.Mappools.Any(x => x.Name == mappoolName + " (" + i + ")"))
                            i++;
                        mappoolName = mappoolName + " (" + i + ")";
                        localLog.Information("rename mappool '{oldName}' to '{newName}' because of name collision", mappoolItem.Name.Value, mappoolName);
                    }

                    Mappool mappool = new Mappool() { Tournament = tournament };
                    mappool.Name = mappoolName;
                    mappool.Save();

                    foreach (var mappoolMapItem in mappoolItem.Maps)
                    {
                        localLog.Information("add beatmap '{beatmap}' to mappool '{newName}' ", mappoolMapItem.Beatmap.Id.Value, mappoolName);
                        Beatmap beatmap = new Beatmap()
                        {
                            Id = Convert.ToInt32(mappoolMapItem.Beatmap.Id.Value),
                            SetId = Convert.ToInt32(mappoolMapItem.Beatmap.SetId.Value),
                            Artist = mappoolMapItem.Beatmap.Artist.Value,
                            Title = mappoolMapItem.Beatmap.Title.Value,
                            Version = mappoolMapItem.Beatmap.Version.Value,
                            Creator = mappoolMapItem.Beatmap.Creator.Value,
                            BPM = Convert.ToDecimal(mappoolMapItem.Beatmap.BPM.Value),
                            AR = Convert.ToDecimal(mappoolMapItem.Beatmap.AR.Value),
                            CS = Convert.ToDecimal(mappoolMapItem.Beatmap.CS.Value)
                        };
                        Database.Database.AddBeatmap(beatmap);

                        beatmap = await Database.Database.GetBeatmap(Convert.ToInt32(mappoolMapItem.Beatmap.Id.Value));

                        MappoolMap mappoolMap = new MappoolMap()
                        {
                            Mappool = mappool,
                            Beatmap = beatmap,
                            Tag = mappoolMapItem.Tag.Value,
                            ListIndex = Convert.ToInt32(mappoolMapItem.ListIndex.Value)
                        };

                        foreach (var modItem in mappoolMapItem.Mods)
                        {
                            if (Enum.TryParse(modItem.Value, out Enums.GameMods gameMod))
                                mappoolMap.Mods.Add(gameMod);
                        }

                        mappool.Beatmaps.Add(mappoolMap);
                        mappoolMap.Save();
                    }
                }

                foreach (var matchItem in tournamentItem.Matches)
                {
                    localLog.Information("import new match '{match}' in tournament '{tournament}'", matchItem.Name.Value, tournament.Name);
                    Match match = new Match();

                    match.Name = matchItem.Name.Value;
                    match.Tournament = tournament;
                    match.Mappool = tournament.Mappools.FirstOrDefault(x => x.Name == matchItem.Mappool.Value);
                    if (Enum.TryParse(matchItem.GameMode.Value, out Enums.GameModes matchGameMode))
                        match.GameMode = matchGameMode;
                    else
                        match.GameMode = Enums.GameModes.Standard;
                    if (Enum.TryParse(matchItem.TeamMode.Value, out Enums.TeamModes matchTeamMode))
                        match.TeamMode = matchTeamMode;
                    else
                        match.TeamMode = Enums.TeamModes.TeamVS;
                    if (Enum.TryParse(matchItem.WinCondition.Value, out Enums.WinConditions matchWinCondition))
                        match.WinCondition = matchWinCondition;
                    else
                        match.WinCondition = Enums.WinConditions.ScoreV2;
                    match.TeamBlue = tournament.Teams.FirstOrDefault(x => x.Name == matchItem.TeamBlue.Value);
                    match.TeamRed = tournament.Teams.FirstOrDefault(x => x.Name == matchItem.TeamRed.Value);
                    match.TeamSize = Convert.ToInt32(matchItem.TeamSize.Value);
                    match.RoomSize = Convert.ToInt32(matchItem.RoomSize.Value);
                    match.RollWinnerTeam = tournament.Teams.FirstOrDefault(x => x.Name == matchItem.RollWinnerTeam.Value);
                    match.FirstPickerTeam = tournament.Teams.FirstOrDefault(x => x.Name == matchItem.FirstPickerTeam.Value);
                    match.BO = Convert.ToInt32(matchItem.BO.Value);
                    match.ViewerMode = matchItem.ViewerMode.Value;
                    match.MpTimerCommand = Convert.ToInt32(matchItem.MpTimerCommand.Value);
                    match.MpTimerAfterGame = Convert.ToInt32(matchItem.MpTimerAfterGame.Value);
                    match.MpTimerAfterPick = Convert.ToInt32(matchItem.MpTimerAfterPick.Value);
                    match.PointsForSecondBan = Convert.ToInt32(matchItem.PointsForSecondBan.Value);
                    match.AllPicksFreemod = matchItem.AllPicksFreemod.Value;
                    match.WarmupMode = matchItem.WarmupMode.Value;
                    match.MatchTime = Convert.ToDateTime(matchItem.MatchTime);

                    foreach (var playerItem in matchItem.Players)
                    {
                        Player player = new Player()
                        {
                            Id = Convert.ToInt32(playerItem.Id.Value),
                            Name = playerItem.Name.Value,
                            Country = playerItem.Country.Value
                        };
                        Database.Database.AddPlayer(player);

                        player = await Database.Database.GetPlayer(playerItem.Name.Value);
                        match.Players.Add(player, Convert.ToInt32(playerItem.Points.Value));
                    }

                    match.RollWinnerPlayer = match.Players.Keys.FirstOrDefault(x => x.Name == matchItem.RollWinnerPlayer.Value);
                    match.FirstPickerPlayer = match.Players.Keys.FirstOrDefault(x => x.Name == matchItem.FirstPickerPlayer.Value);
                    match.Save();
                }
            }

            localLog.Information("import finished");
            var model = new ExportSuccessDialogViewModel("Import successfull");
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);
            await DialogHost.Show(view, "MainDialogHost");
        }
        #endregion
    }

    public class ExportItem : Screen
    {
        public ExportItem()
        {
            ExportCategories = new BindableCollection<ExportCategory>();
            Export = false;
        }

        private bool export;
        public bool Export
        {
            get { return export; }
            set
            {
                if (value != export)
                {
                    export = value;
                    if (value)
                    {
                        if (ParentItem != null && ParentItem.Parent != null)
                        {
                            ParentItem.Parent.SetExportFromChild(value);
                        }
                    }
                    foreach (var category in ExportCategories)
                    {
                        foreach (var item in category.ExportItems)
                        {
                            item.SetExportFromParent(value);
                        }
                    }
                    NotifyOfPropertyChange(() => Export);
                }
            }
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public BindableCollection<ExportCategory> ExportCategories { get; set; }
        public ExportCategory ParentItem { get; set; }

        public void SetExportFromChild(bool export)
        {
            this.export = export;
            NotifyOfPropertyChange(() => Export);
            if (ParentItem != null && ParentItem.Parent != null)
            {
                ParentItem.Parent.SetExportFromChild(export);
            }
        }

        public void SetExportFromParent(bool export)
        {
            this.export = export;
            NotifyOfPropertyChange(() => Export);
            foreach (var category in ExportCategories)
            {
                foreach (var item in category.ExportItems)
                {
                    item.SetExportFromParent(export);
                }
            }
        }
    }

    public class ExportCategory
    {
        public ExportCategory()
        {
            ExportItems = new BindableCollection<ExportItem>();
        }

        public string Name { get; set; }
        public BindableCollection<ExportItem> ExportItems { get; set; }
        public ExportItem Parent { get; set; }
    }
}
