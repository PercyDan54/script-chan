using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using script_chan2.Database;
using script_chan2.DataTypes;

namespace Test
{
    [TestClass]
    public class DatabaseTest
    {
        [TestInitialize]
        public void Init()
        {
            DbCreator.CreateDb();
            Database.Initialize();
        }

        [TestMethod]
        public void CreateDatabase()
        {
            DbCreator.CreateDb();
            Assert.IsTrue(File.Exists("Database.sqlite"), "Database was not created");
        }

        [TestMethod]
        public void CreateTournament()
        {
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();
            Assert.AreEqual(1, Database.Tournaments.Count, "Tournament was not created");
            Assert.AreEqual(tournament.Name, Database.Tournaments[0].Name, "Tournament was not created");
        }

        [TestMethod]
        public void GetTournaments()
        {
            Assert.AreEqual(0, Database.Tournaments.Count, "Database is not empty");
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();
            Assert.AreEqual(1, Database.Tournaments.Count, "Tournament was not created");
        }

        [TestMethod]
        public void DeleteTournament()
        {
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();
            Assert.AreEqual(1, Database.Tournaments.Count, "Tournament was not created");
            tournament.Delete();
            Assert.AreEqual(0, Database.Tournaments.Count, "Tournament was not deleted");
        }

        [TestMethod]
        public void UpdateTournament()
        {
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();
            tournament.Name = "Renamed";
            tournament.GameMode = script_chan2.Enums.GameModes.Mania;
            tournament.Save();
            Assert.AreEqual("Renamed", Database.Tournaments[0].Name, "Tournament was not updated");
            Assert.AreEqual(script_chan2.Enums.GameModes.Mania, Database.Tournaments[0].GameMode, "Tournament was not updated");
        }

        [TestMethod]
        public void CreateWebhook()
        {
            var webhook = new Webhook("TestWebhook", "http://localhost");
            webhook.Save();
            Assert.AreEqual(1, Database.Webhooks.Count, "Webhook was not created");
        }

        [TestMethod]
        public void GetWebhooks()
        {
            Assert.AreEqual(0, Database.Webhooks.Count, "Database is not empty");
            var webhook = new Webhook("TestWebhook", "http://localhost");
            webhook.Save();
            Assert.AreEqual(1, Database.Webhooks.Count, "Webhook was not created");
        }

        [TestMethod]
        public void DeleteWebhook()
        {
            var webhook = new Webhook("TestWebhook", "http://localhost");
            webhook.Save();
            Assert.AreEqual(1, Database.Webhooks.Count, "Webhook was not created");
            webhook.Delete();
            Assert.AreEqual(0, Database.Webhooks.Count, "Webhook was not deleted");
        }

        [TestMethod]
        public void UpdateWebhook()
        {
            var webhook = new Webhook("TestWebhook", "http://localhost");
            webhook.Save();
            webhook.Name = "TestWebhook2";
            webhook.URL = "http://localhost2";
            webhook.Save();
            Assert.AreEqual("TestWebhook2", Database.Webhooks[0].Name, "Webhook was not updated");
            Assert.AreEqual("http://localhost2", Database.Webhooks[0].URL, "Webhook was not updated");
        }

        [TestMethod]
        public void AddWebhookToTournament()
        {
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();
            var webhook = new Webhook("TestWebhook", "http://localhost");
            webhook.Save();
            tournament.AddWebhook(webhook);
            Assert.AreEqual(1, tournament.Webhooks.Count, "Webhook was not added to tournament");
        }

        [TestMethod]
        public void RemoveWebhookFromTournament()
        {
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();
            var webhook = new Webhook("TestWebhook", "http://localhost");
            webhook.Save();
            tournament.AddWebhook(webhook);
            Assert.AreEqual(1, tournament.Webhooks.Count, "Webhook was not added to tournament");
            tournament.RemoveWebhook(webhook);
            Assert.AreEqual(0, tournament.Webhooks.Count, "Webhook was not removed from tournament");
        }

        [TestMethod]
        public void CreateMappool()
        {
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();
            var mappool = new Mappool("TestMappool", tournament);
            mappool.Save();
            var mappool2 = new Mappool("TestMappool2");
            mappool2.Save();
            Assert.AreEqual(2, Database.Mappools.Count, "Mappools were not created");
        }

        [TestMethod]
        public void DeleteMappool()
        {
            var mappool = new Mappool("TestMappool");
            mappool.Save();
            Assert.AreEqual(1, Database.Mappools.Count, "Mappool was not created");
            mappool.Delete();
            Assert.AreEqual(0, Database.Mappools.Count, "Mappools was not deleted");
        }

        [TestMethod]
        public void UpdateMappool()
        {
            var mappool = new Mappool("TestMappool");
            mappool.Save();
            mappool.Name = "Renamed";
            mappool.Save();
            Assert.AreEqual("Renamed", Database.Mappools[0].Name, "Mappool was not updated");
        }

        [TestMethod]
        public void AddMappoolToTournament()
        {
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();
            var mappool = new Mappool("TestMappool", tournament);
            mappool.Save();

            var tournament2 = new Tournament("TestTournament2", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament2.Save();
            var mappool2 = new Mappool("TestMappool2");
            mappool2.Save();
            tournament2.AddMappool(mappool2);

            var tournament3 = new Tournament("TestTournament3", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament3.Save();
            var mappool3 = new Mappool("TestMappool3");
            mappool3.Save();
            mappool3.Tournament = tournament3;

            Assert.AreEqual(1, tournament.Mappools.Count, "Mappool was not added to tournament mappool list");
            Assert.AreEqual("TestMappool", tournament.Mappools[0].Name, "Wrong mappool added?");
            Assert.AreEqual(mappool.Tournament, tournament, "Mappool tournament reference was not set");

            Assert.AreEqual(1, tournament2.Mappools.Count, "Mappool was not added to tournament mappool list");
            Assert.AreEqual("TestMappool2", tournament2.Mappools[0].Name, "Wrong mappool added?");
            Assert.AreEqual(mappool2.Tournament, tournament2, "Mappool tournament reference was not set");

            Assert.AreEqual(1, tournament3.Mappools.Count, "Mappool was not added to tournament mappool list");
            Assert.AreEqual("TestMappool3", tournament3.Mappools[0].Name, "Wrong mappool added?");
            Assert.AreEqual(mappool3.Tournament, tournament3, "Mappool tournament reference was not set");
        }

        [TestMethod]
        public void RemoveMappoolFromTournament()
        {
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();
            var mappool = new Mappool("TestMappool", tournament);
            mappool.Save();

            var tournament2 = new Tournament("TestTournament2", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament2.Save();
            var mappool2 = new Mappool("TestMappool2", tournament2);
            mappool2.Save();

            mappool.Tournament = null;
            tournament2.RemoveMappool(mappool2);

            Assert.AreEqual(0, tournament.Mappools.Count, "Mappool was not removed from tournament mappool list");
            Assert.AreEqual(null, mappool.Tournament, "Mappool tournament reference was not removed");

            Assert.AreEqual(0, tournament2.Mappools.Count, "Mappool was not removed from tournament mappool list");
            Assert.AreEqual(null, mappool2.Tournament, "Mappool tournament reference was not removed");
        }

        [TestMethod]
        public void GetSettings()
        {
            Assert.AreEqual(3, Settings.DefaultBO, "Unexpected default BO");
            Assert.AreEqual(1000, Settings.IrcTimeout, "Unexpected default irc timeout");
            Assert.AreEqual("en-US", Settings.Lang, "Unexpected default language");
            Assert.AreEqual(180, Settings.MpTimerDuration, "Unexpected default mp timer duration");
        }

        [TestMethod]
        public void UpdateSettings()
        {
            Settings.IrcUsername = "Borengar";
            Settings.Lang = "de-DE";
            Settings.MpTimerDuration = 300;

            Assert.AreEqual("Borengar", Settings.IrcUsername, "Unexpected irc username");
            Assert.AreEqual("de-DE", Settings.Lang, "Unexpected language");
            Assert.AreEqual(300, Settings.MpTimerDuration, "Unexpected mp timer duration");
        }

        [TestMethod]
        public void AddBeatmapToMappool()
        {
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();
            var mappool = new Mappool("TestMappool", tournament);
            mappool.Save();
            var beatmap = Database.GetBeatmap(922172);
            var beatmap2 = Database.GetBeatmap(180092);

            var mappoolMap = new MappoolMap(mappool, beatmap, "Hidden");
            mappool.AddBeatmap(mappoolMap);
            mappoolMap.Save();

            var mappoolMap2 = new MappoolMap(mappool, beatmap2, "Hidden,HardRock");
            mappool.AddBeatmap(mappoolMap2);
            mappoolMap2.Save();

            Assert.AreEqual(2, mappool.Beatmaps.Count, "Unexpected beatmap amount");
            Assert.AreEqual(2, mappoolMap2.ListIndex, "Unexpected list index");
            Assert.AreEqual(2, mappoolMap2.Mods.Count, "Unexpected mod count");
        }

        [TestMethod]
        public void RemoveBeatmapFromMappool()
        {
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();
            var mappool = new Mappool("TestMappool", tournament);
            mappool.Save();
            var beatmap = Database.GetBeatmap(922172);
            var beatmap2 = Database.GetBeatmap(180092);

            var mappoolMap = new MappoolMap(mappool, beatmap, "Hidden");
            mappool.AddBeatmap(mappoolMap);
            mappoolMap.Save();

            var mappoolMap2 = new MappoolMap(mappool, beatmap2, "Hidden,HardRock");
            mappool.AddBeatmap(mappoolMap2);
            mappoolMap2.Save();

            mappoolMap.Delete();

            Assert.AreEqual(1, mappool.Beatmaps.Count, "Unexpected beatmap amount");
            Assert.AreEqual(1, mappoolMap2.ListIndex, "Unexpected list index");
        }

        [TestMethod]
        public void CreateTeam()
        {
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();

            var team = new Team(tournament, "TestTeam");
            team.Save();

            Assert.AreEqual(team.Tournament, tournament, "Unexpected tournament");
            Assert.AreEqual(team.Name, "TestTeam", "Unexpected name");
            Assert.IsTrue(tournament.Teams.Contains(team), "Tournament should contain team");
        }

        [TestMethod]
        public void DeleteTeam()
        {
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();

            var team = new Team(tournament, "TestTeam");
            team.Save();
            team.Delete();

            Assert.IsFalse(tournament.Teams.Contains(team), "Tournament should not contain team");
        }

        [TestMethod]
        public void AddPlayerToTeam()
        {
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();

            var team = new Team(tournament, "TestTeam");
            team.Save();

            var player = Database.GetPlayer("3312177");
            team.AddPlayer(player);

            Assert.IsTrue(team.Players.Contains(player), "Team should contain player");
        }

        [TestMethod]
        public void RemovePlayerFromTeam()
        {
            var tournament = new Tournament("TestTournament", script_chan2.Enums.GameModes.Standard, script_chan2.Enums.TeamModes.TeamVS, script_chan2.Enums.WinConditions.ScoreV2, "Test", 4, 8);
            tournament.Save();

            var team = new Team(tournament, "TestTeam");
            team.Save();

            var player = Database.GetPlayer("3312177");
            team.AddPlayer(player);
            team.RemovePlayer(player);

            Assert.IsFalse(team.Players.Contains(player), "Team should not contain player");
        }

        [TestMethod]
        public void GetBeatmap()
        {
            var beatmap = Database.GetBeatmap(922172);
            Assert.AreEqual(922172, beatmap.Id, "Unexpected id");
            Assert.AreEqual("Falcom Sound Team jdk", beatmap.Artist, "Unexpected artist");
            Assert.AreEqual("jonathanlfj", beatmap.Creator, "Unexpected creator");
            Assert.AreEqual(427166, beatmap.SetId, "Unexpected set id");
            Assert.AreEqual("The Azure Arbitrator", beatmap.Title, "Unexpected title");
            Assert.AreEqual("Chrono Collapse", beatmap.Version, "Unexpected version");
        }

        [TestMethod]
        public void GetPlayer()
        {
            var player = Database.GetPlayer("3312177");
            Assert.AreEqual(3312177, player.Id, "Unexpected id");
            Assert.AreEqual("Borengar", player.Name, "Unexpected name");
            Assert.AreEqual("DE", player.Country, "Unexpected country");
        }
    }
}
