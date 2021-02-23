using AngleSharp;
using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using script_chan2.DataTypes;
using script_chan2.Enums;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace script_chan2.OsuApi
{
    public static class OsuApi
    {
        private static ILogger localLog = Log.ForContext(typeof(OsuApi));

        #region API calls
        public static async Task<bool> CheckApiKey(string key)
        {
            localLog.Information("check api key");
            var request = (HttpWebRequest)WebRequest.Create("https://osu.ppy.sh/api/get_user?u=2&k=" + key);
            try
            {
                _ = await request.GetResponseAsync();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static async Task<Beatmap> GetBeatmap(int beatmapId)
        {
            localLog.Information("get beatmap {id}", beatmapId);
            try
            {
                var response = await SendRequest("get_beatmaps", "b=" + beatmapId);
                localLog.Information("GetBeatmap data: " + response);
                var data = JsonConvert.DeserializeObject<List<ApiBeatmap>>(response);
                if (data.Count == 0)
                {
                    localLog.Information("beatmap {id} not found", beatmapId);
                    return null;
                }
                var beatmap = data[0];
                return new Beatmap()
                {
                    Id = Convert.ToInt32(beatmap.beatmap_id),
                    SetId = Convert.ToInt32(beatmap.beatmapset_id),
                    Artist = beatmap.artist,
                    Title = beatmap.title,
                    Version = beatmap.version,
                    Creator = beatmap.creator,
                    BPM = Convert.ToDecimal(beatmap.bpm),
                    AR = Convert.ToDecimal(beatmap.diff_approach),
                    CS = Convert.ToDecimal(beatmap.diff_size)
                };
            }
            catch (ApiException)
            {
                try
                {
                    localLog.Information("using website to get beatmap {id}", beatmapId);
                    using (var webClient = new WebClient())
                    {
                        var config = Configuration.Default;
                        var context = BrowsingContext.New(config);
                        var source = await webClient.DownloadStringTaskAsync("https://osu.ppy.sh/beatmaps/" + beatmapId);
                        var document = await context.OpenAsync(req => req.Content(source));
                        var element = document.QuerySelector("#json-beatmapset").InnerHtml;
                        dynamic json = JObject.Parse(element);
                        foreach (var beatmap in json.beatmaps)
                        {
                            if (beatmap.id != beatmapId)
                                continue;
                            return new Beatmap()
                            {
                                Id = beatmap.id,
                                SetId = Convert.ToInt32(json.id),
                                Artist = json.artist,
                                Title = json.title,
                                Version = beatmap.version,
                                Creator = json.creator,
                                BPM = Convert.ToDecimal(json.bpm),
                                AR = Convert.ToInt32(beatmap.ar),
                                CS = Convert.ToInt32(beatmap.cs)
                            };
                        }
                        return null;
                    }
                }
                catch
                {
                    localLog.Information("website is not reachable");
                    MessageBox.Show(Properties.Resources.OsuApi_TimeoutErrorText, Properties.Resources.OsuApi_TimeoutErrorHeader);
                    return null;
                }
            }
        }

        public static async Task<Player> GetPlayer(string playerId)
        {
            localLog.Information("get player {id}", playerId);
            try
            {
                var response = await SendRequest("get_user", "u=" + playerId);
                localLog.Information("GetPlayer data: " + response);
                var data = JsonConvert.DeserializeObject<List<ApiPlayer>>(response);
                if (data.Count == 0)
                {
                    localLog.Information("player {id} not found", playerId);
                    return null;
                }
                var player = data[0];
                return new Player()
                {
                    Name = player.username,
                    Country = player.country,
                    Id = Convert.ToInt32(player.user_id)
                };
            }
            catch (ApiException)
            {
                try
                {
                    localLog.Information("using website to get player {id}", playerId);
                    using (var webClient = new WebClient())
                    {
                        var config = Configuration.Default;
                        var context = BrowsingContext.New(config);
                        var source = await webClient.DownloadStringTaskAsync("https://osu.ppy.sh/users/" + playerId);
                        var document = await context.OpenAsync(req => req.Content(source));
                        var element = document.QuerySelector("#json-user").InnerHtml;
                        dynamic json = JObject.Parse(element);
                        return new Player()
                        {
                            Name = json.username,
                            Country = json.country_code,
                            Id = json.id
                        };
                    }
                }
                catch
                {
                    localLog.Information("website is not reachable");
                    MessageBox.Show(Properties.Resources.OsuApi_TimeoutErrorText, Properties.Resources.OsuApi_TimeoutErrorHeader);
                    return null;
                }
            }
        }

        public static async Task UpdateGames(Match match)
        {
            localLog.Information("refresh match {id}", match.RoomId);
            try
            {
                var response = await SendRequest("get_match", "mp=" + match.RoomId);
                localLog.Information("GetMatch data: " + response);
                var data = JsonConvert.DeserializeObject<ApiMatch>(response);
                foreach (var gameData in data.games)
                {
                    if (match.Games.Any(x => x.Id == Convert.ToInt32(gameData.game_id)))
                        continue;

                    if (gameData.end_time == null)
                        continue;

                    if (gameData.scores.Count == 0)
                        continue;

                    var beatmap = await Database.Database.GetBeatmap(Convert.ToInt32(gameData.beatmap_id));
                    var game = new Game()
                    {
                        Match = match,
                        Id = Convert.ToInt32(gameData.game_id),
                        Beatmap = beatmap,
                        Mods = ModsFromBitEnum(gameData.mods)
                    };

                    foreach (var scoreData in gameData.scores)
                    {
                        var player = await Database.Database.GetPlayer(scoreData.user_id);
                        var score = new Score()
                        {
                            Game = game,
                            Player = player,
                            Points = Convert.ToInt32(scoreData.score),
                            Team = TeamFromString(scoreData.team),
                            Passed = scoreData.pass == "1",
                            Mods = ModsFromBitEnum(scoreData.enabled_mods)
                        };

                        game.Scores.Add(score);
                    }

                    match.Games.Add(game);
                }
            }
            catch (ApiException)
            {
                try
                {
                    using (var webClient = new WebClient())
                    {
                        var result = await webClient.DownloadStringTaskAsync("https://osu.ppy.sh/community/matches/" + match.RoomId + "/history");
                        WebsiteMatch json = JsonConvert.DeserializeObject<WebsiteMatch>(result);
                        foreach (var eventObject in json.events)
                        {
                            if (eventObject.detail.type == "other")
                            {
                                if (match.Games.Any(x => x.Id == eventObject.game.id))
                                    continue;

                                if (eventObject.game.end_time == null)
                                    continue;

                                if (eventObject.game.scores.Count == 0)
                                    continue;

                                var beatmap = await Database.Database.GetBeatmap(Convert.ToInt32(eventObject.game.beatmap.id));
                                var game = new Game()
                                {
                                    Match = match,
                                    Id = Convert.ToInt32(eventObject.game.id),
                                    Beatmap = beatmap,
                                    Mods = ModsFromList(eventObject.game.mods)
                                };

                                foreach (var scoreData in eventObject.game.scores)
                                {
                                    var player = await Database.Database.GetPlayer(scoreData.user_id.ToString());
                                    var score = new Score()
                                    {
                                        Game = game,
                                        Player = player,
                                        Points = scoreData.score,
                                        Team = TeamFromString(scoreData.match.team),
                                        Passed = scoreData.match.pass == 1,
                                        Mods = ModsFromList(scoreData.mods)
                                    };

                                    game.Scores.Add(score);
                                }

                                match.Games.Add(game);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    localLog.Information("website is not reachable");
                    MessageBox.Show(Properties.Resources.OsuApi_TimeoutErrorText, Properties.Resources.OsuApi_TimeoutErrorHeader);
                }
            }
        }
        #endregion

        #region Helper methods
        private static async Task<string> SendRequest(string method, string parameters)
        {
            using (var webClient = new WebClient())
            {
                localLog.Information("send request 'https://osu.ppy.sh/api/" + method + "?" + parameters + "'");
                for (var i = 0; i < 6; i++)
                {
                    try
                    {
                        return await webClient.DownloadStringTaskAsync("https://osu.ppy.sh/api/" + method + "?k=" + Settings.ApiKey + "&" + parameters);
                    }
                    catch (WebException exception)
                    {
                        localLog.Error(exception, "api call failed");
                        await Task.Delay(5000);
                    }
                }
                throw new ApiException("api call failed after 6 retries");
            }
        }

        public static List<GameMods> ModsFromBitEnum(string bitEnum)
        {
            var mods = new List<GameMods>();
            var bits = new BitArray(new int[] { Convert.ToInt32(bitEnum) });

            if (bits[0])
                mods.Add(GameMods.NoFail);
            if (bits[3])
                mods.Add(GameMods.Hidden);
            if (bits[4])
                mods.Add(GameMods.HardRock);
            if (bits[6])
                mods.Add(GameMods.DoubleTime);
            if (bits[10])
                mods.Add(GameMods.Flashlight);

            return mods;
        }

        public static List<GameMods> ModsFromList(List<string> modList)
        {
            var mods = new List<GameMods>();

            foreach (string mod in modList)
            {
                switch (mod)
                {
                    case "NF": mods.Add(GameMods.NoFail); break;
                    case "HD": mods.Add(GameMods.Hidden); break;
                    case "HR": mods.Add(GameMods.HardRock); break;
                    case "DT": mods.Add(GameMods.DoubleTime); break;
                    case "FL": mods.Add(GameMods.Flashlight); break;
                }
            }

            return mods;
        }

        public static LobbyTeams TeamFromString(string team)
        {
            switch (team)
            {
                case "2":
                case "red":
                    return LobbyTeams.Red;
                case "1":
                case "blue":
                    return LobbyTeams.Blue;
            }
            return LobbyTeams.None;
        }

        public class ApiException : Exception
        {
            public ApiException() { }

            public ApiException(string message) : base(message) { }

            public ApiException(string message, Exception innerException) : base(message, innerException) { }
        }
        #endregion
    }
}
