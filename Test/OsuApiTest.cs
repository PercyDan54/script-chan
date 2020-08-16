using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using script_chan2.Database;
using script_chan2.DataTypes;
using script_chan2.Enums;
using script_chan2.OsuApi;

namespace Test
{
    [TestClass]
    public class OsuApiTest
    {
        [TestInitialize]
        public void Init()
        {
            DbCreator.CreateDb();
        }

        [TestMethod]
        public void GetBeatmap()
        {
            var beatmap = OsuApi.GetBeatmap(922172);
            Assert.AreEqual(922172, beatmap.Id, "Unexpected id");
            Assert.AreEqual("Falcom Sound Team jdk", beatmap.Artist, "Unexpected artist");
            Assert.AreEqual("jonathanlfj", beatmap.Creator, "Unexpected creator");
            Assert.AreEqual(427166, beatmap.SetId, "Unexpected set id");
            Assert.AreEqual("The Azure Arbitrator", beatmap.Title, "Unexpected title");
            Assert.AreEqual("Chrono Collapse", beatmap.Version, "Unexpected version");
            Assert.AreEqual(274, beatmap.BPM, "Unexpected BPM");
            Assert.AreEqual(4, beatmap.CS, "Unexpected CS");
            Assert.AreEqual(10, beatmap.AR, "Unexpected AR");
        }

        [TestMethod]
        public void GetPlayer()
        {
            var player = OsuApi.GetPlayer("3312177");
            Assert.AreEqual(3312177, player.Id, "Unexpected id");
            Assert.AreEqual("Borengar", player.Name, "Unexpected name");
            Assert.AreEqual("DE", player.Country, "Unexpected country");

            var player2 = OsuApi.GetPlayer("Borengar");
            Assert.AreEqual(3312177, player2.Id, "Unexpected id");
            Assert.AreEqual("Borengar", player2.Name, "Unexpected name");
            Assert.AreEqual("DE", player2.Country, "Unexpected country");
        }

        [TestMethod]
        public void UpdateMatch()
        {
            var match = new Match()
            {
                Name = "TestMatch",
                RoomId = 48149847
            };
            OsuApi.UpdateGames(match);
            Assert.AreEqual("With a Dance Number", match.Games[0].Beatmap.Title, "Unexpected first map");
            Assert.AreEqual(60224, match.Games[0].Scores.Find(x => x.Player.Name == "Vaxei").Points, "Unexpected score from Vaxei");
            Assert.AreEqual(12, match.Games.Count, "Unexpected game count");
            CollectionAssert.AreEqual(new List<GameMods> { GameMods.Hidden }, match.Games[0].Scores.Find(x => x.Player.Name == "Karthy").Mods, "Unexpected mods from Karthy");
        }
    }
}
