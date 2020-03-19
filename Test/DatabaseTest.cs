using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using script_chan2.Database;
using script_chan2.DataTypes;
using script_chan2.OsuIrc;

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
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();
            Assert.AreEqual(1, Database.Tournaments.Count, "Tournament was not created");
            Assert.AreEqual(tournament.Name, Database.Tournaments[0].Name, "Tournament was not created");
        }

        [TestMethod]
        public void GetTournaments()
        {
            Assert.AreEqual(0, Database.Tournaments.Count, "Database is not empty");
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();
            Assert.AreEqual(1, Database.Tournaments.Count, "Tournament was not created");
        }

        [TestMethod]
        public void DeleteTournament()
        {
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();
            Assert.AreEqual(1, Database.Tournaments.Count, "Tournament was not created");
            tournament.Delete();
            Assert.AreEqual(0, Database.Tournaments.Count, "Tournament was not deleted");
        }

        [TestMethod]
        public void UpdateTournament()
        {
            var tournament = new Tournament()
            {
                Name = "TestTournament",
                GameMode = script_chan2.Enums.GameModes.Standard
            };
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
            var webhook = new Webhook()
            {
                Name = "TestWebhook",
                URL = "http://localhost"
            };
            webhook.Save();
            Assert.AreEqual(1, Database.Webhooks.Count, "Webhook was not created");
        }

        [TestMethod]
        public void GetWebhooks()
        {
            Assert.AreEqual(0, Database.Webhooks.Count, "Database is not empty");
            var webhook = new Webhook()
            {
                Name = "TestWebhook",
                URL = "http://localhost"
            };
            webhook.Save();
            Assert.AreEqual(1, Database.Webhooks.Count, "Webhook was not created");
        }

        [TestMethod]
        public void DeleteWebhook()
        {
            var webhook = new Webhook()
            {
                Name = "TestWebhook",
                URL = "http://localhost"
            };
            webhook.Save();
            Assert.AreEqual(1, Database.Webhooks.Count, "Webhook was not created");
            webhook.Delete();
            Assert.AreEqual(0, Database.Webhooks.Count, "Webhook was not deleted");
        }

        [TestMethod]
        public void UpdateWebhook()
        {
            var webhook = new Webhook()
            {
                Name = "TestWebhook",
                URL = "http://localhost"
            };
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
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();
            var webhook = new Webhook()
            {
                Name = "TestWebhook",
                URL = "http://localhost"
            };
            webhook.Save();
            tournament.AddWebhook(webhook);
            Assert.AreEqual(1, tournament.Webhooks.Count, "Webhook was not added to tournament");
        }

        [TestMethod]
        public void RemoveWebhookFromTournament()
        {
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();
            var webhook = new Webhook()
            {
                Name = "TestWebhook",
                URL = "http://localhost"
            };
            webhook.Save();
            tournament.AddWebhook(webhook);
            Assert.AreEqual(1, tournament.Webhooks.Count, "Webhook was not added to tournament");
            tournament.RemoveWebhook(webhook);
            Assert.AreEqual(0, tournament.Webhooks.Count, "Webhook was not removed from tournament");
        }

        [TestMethod]
        public void CreateMappool()
        {
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();
            var mappool = new Mappool()
            {
                Name = "TestMappool",
                Tournament = tournament
            };
            mappool.Save();
            Assert.AreEqual(1, Database.Mappools.Count, "Mappools were not created");
        }

        [TestMethod]
        public void DeleteMappool()
        {
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();
            var mappool = new Mappool()
            {
                Name = "TestMappool",
                Tournament = tournament
            };
            mappool.Save();
            Assert.AreEqual(1, Database.Mappools.Count, "Mappool was not created");
            mappool.Delete();
            Assert.AreEqual(0, Database.Mappools.Count, "Mappools was not deleted");
        }

        [TestMethod]
        public void UpdateMappool()
        {
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();
            var mappool = new Mappool()
            {
                Name = "TestMappool",
                Tournament = tournament
            };
            mappool.Save();
            mappool.Name = "Renamed";
            mappool.Save();
            Assert.AreEqual("Renamed", Database.Mappools[0].Name, "Mappool was not updated");
        }

        [TestMethod]
        public void GetSettings()
        {
            Assert.AreEqual(3, Settings.DefaultBO, "Unexpected default BO");
            Assert.AreEqual(1000, Settings.IrcTimeout, "Unexpected default irc timeout");
            Assert.AreEqual("en-US", Settings.Lang, "Unexpected default language");
            Assert.AreEqual(120, Settings.DefaultTimerCommand, "Unexpected default mp timer duration");
            Assert.AreEqual(120, Settings.DefaultTimerAfterGame, "Unexpected default timer after game duration");
            Assert.AreEqual(120, Settings.DefaultTimerAfterPick, "Unexpected default timer after pick duration");
        }

        [TestMethod]
        public void UpdateSettings()
        {
            Settings.IrcUsername = "Borengar";
            Settings.Lang = "de-DE";
            Settings.DefaultTimerCommand = 300;
            Settings.DefaultTimerAfterGame = 400;
            Settings.DefaultTimerAfterPick = 500;

            Assert.AreEqual("Borengar", Settings.IrcUsername, "Unexpected irc username");
            Assert.AreEqual("de-DE", Settings.Lang, "Unexpected language");
            Assert.AreEqual(300, Settings.DefaultTimerCommand, "Unexpected mp timer duration");
            Assert.AreEqual(400, Settings.DefaultTimerAfterGame, "Unexpected mp timer after game duration");
            Assert.AreEqual(500, Settings.DefaultTimerAfterPick, "Unexpected mp timer after pick duration");
        }

        [TestMethod]
        public void AddBeatmapToMappool()
        {
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();
            var mappool = new Mappool()
            {
                Name = "TestMappool",
                Tournament = tournament
            };
            mappool.Save();
            var beatmap = Database.GetBeatmap(922172);
            var beatmap2 = Database.GetBeatmap(180092);

            var mappoolMap = new MappoolMap()
            {
                Mappool = mappool,
                Beatmap = beatmap
            };
            mappoolMap.Mods.Add(script_chan2.Enums.GameMods.Hidden);
            mappool.AddBeatmap(mappoolMap);
            mappoolMap.Save();

            var mappoolMap2 = new MappoolMap()
            {
                Mappool = mappool,
                Beatmap = beatmap2
            };
            mappoolMap2.Mods.Add(script_chan2.Enums.GameMods.Hidden);
            mappoolMap2.Mods.Add(script_chan2.Enums.GameMods.HardRock);
            mappool.AddBeatmap(mappoolMap2);
            mappoolMap2.Save();

            Assert.AreEqual(2, mappool.Beatmaps.Count, "Unexpected beatmap amount");
            Assert.AreEqual(2, mappoolMap2.ListIndex, "Unexpected list index");
            Assert.AreEqual(2, mappoolMap2.Mods.Count, "Unexpected mod count");
        }

        [TestMethod]
        public void RemoveBeatmapFromMappool()
        {
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();
            var mappool = new Mappool()
            {
                Name = "TestMappool",
                Tournament = tournament
            };
            mappool.Save();
            var beatmap = Database.GetBeatmap(922172);
            var beatmap2 = Database.GetBeatmap(180092);

            var mappoolMap = new MappoolMap()
            {
                Mappool = mappool,
                Beatmap = beatmap
            };
            mappoolMap.Mods.Add(script_chan2.Enums.GameMods.Hidden);
            mappool.AddBeatmap(mappoolMap);
            mappoolMap.Save();

            var mappoolMap2 = new MappoolMap()
            {
                Mappool = mappool,
                Beatmap = beatmap2
            };
            mappoolMap.Mods.Add(script_chan2.Enums.GameMods.Hidden);
            mappoolMap.Mods.Add(script_chan2.Enums.GameMods.HardRock);
            mappool.AddBeatmap(mappoolMap2);
            mappoolMap2.Save();

            mappoolMap.Delete();

            Assert.AreEqual(1, mappool.Beatmaps.Count, "Unexpected beatmap amount");
            Assert.AreEqual(1, mappoolMap2.ListIndex, "Unexpected list index");
        }

        [TestMethod]
        public void CreateTeam()
        {
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();

            var team = new Team()
            {
                Tournament = tournament,
                Name = "TestTeam"
            };
            team.Save();

            Assert.AreEqual(team.Tournament, tournament, "Unexpected tournament");
            Assert.AreEqual(team.Name, "TestTeam", "Unexpected name");
            Assert.IsTrue(tournament.Teams.Contains(team), "Tournament should contain team");
        }

        [TestMethod]
        public void DeleteTeam()
        {
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();

            var team = new Team()
            {
                Tournament = tournament,
                Name = "TestTeam"
            };
            team.Save();
            team.Delete();

            Assert.IsFalse(tournament.Teams.Contains(team), "Tournament should not contain team");
        }

        [TestMethod]
        public void AddPlayerToTeam()
        {
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();

            var team = new Team()
            {
                Tournament = tournament,
                Name = "TestTeam"
            };
            team.Save();

            var player = Database.GetPlayer("3312177");
            team.AddPlayer(player);

            Assert.IsTrue(team.Players.Contains(player), "Team should contain player");
        }

        [TestMethod]
        public void RemovePlayerFromTeam()
        {
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();

            var team = new Team()
            {
                Tournament = tournament,
                Name = "TestTeam"
            };
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

        [TestMethod]
        public void AddIrcMessages()
        {
            var tournament = new Tournament()
            {
                Name = "TestTournament"
            };
            tournament.Save();
            var match = new Match()
            {
                Tournament = tournament,
                Name = "TestMatch"
            };
            match.Save();
            var list = new List<IrcMessage>();
            list.Add(new IrcMessage() { Match = match, User = "User 1", Timestamp = DateTime.Now, Message = "Message 1" });
            list.Add(new IrcMessage() { Match = match, User = "User 2", Timestamp = DateTime.Now, Message = "Message 2" });
            list.Add(new IrcMessage() { Match = match, User = "User 3", Timestamp = DateTime.Now, Message = "Message 3" });
            list.Add(new IrcMessage() { Match = match, User = "User 4", Timestamp = DateTime.Now, Message = "Message 4" });
            list.Add(new IrcMessage() { Match = match, User = "User 5", Timestamp = DateTime.Now, Message = "Message 5" });
            list.Add(new IrcMessage() { Match = null, User = "User 6", Timestamp = DateTime.Now, Message = "Message 6" });
            list.Add(new IrcMessage() { Match = null, User = "User 7", Timestamp = DateTime.Now, Message = "Message 7" });
            list.Add(new IrcMessage() { Match = null, User = "User 8", Timestamp = DateTime.Now, Message = "Message 8" });
            list.Add(new IrcMessage() { Match = null, User = "User 9", Timestamp = DateTime.Now, Message = "Message 9" });
            list.Add(new IrcMessage() { Match = null, User = "User 10", Timestamp = DateTime.Now, Message = "Message 10" });
            Database.AddIrcMessages(list);
            Assert.AreEqual(Database.GetIrcMessages(match).Count, 5, "Unexpected message count");
            Assert.AreEqual(Database.GetIrcMessages("User 7").Count, 1, "Unexpected message count");
            Assert.AreEqual(Database.GetIrcMessages("User 2").Count, 0, "Unexpected message count");
        }
    }
}
