using Newtonsoft.Json;
using script_chan2.DataTypes;
using script_chan2.Enums;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.OsuApi
{
    public static class OsuApi
    {
        #region API calls
        public static bool CheckApiKey(string key)
        {
            Log.Information("API check api key");
            var request = (HttpWebRequest)WebRequest.Create("https://osu.ppy.sh/api/get_user?u=2&k=" + key);
            try
            {
                _ = request.GetResponse();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static Beatmap GetBeatmap(int beatmapId)
        {
            Log.Information("API get beatmap {id}", beatmapId);
            var response = SendRequest("get_beatmaps", "b=" + beatmapId);
            var data = JsonConvert.DeserializeObject<List<ApiBeatmap>>(response);
            if (data.Count == 0)
            {
                Log.Information("API beatmap {id} not found", beatmapId);
                return null;
            }
            var beatmap = data[0];
            return new Beatmap(Convert.ToInt32(beatmap.beatmap_id), Convert.ToInt32(beatmap.beatmapset_id), beatmap.artist, beatmap.title, beatmap.version, beatmap.creator);
        }

        public static Player GetPlayer(string playerId)
        {
            Log.Information("API get player {id}", playerId);
            var response = SendRequest("get_user", "u=" + playerId);
            var data = JsonConvert.DeserializeObject<List<ApiPlayer>>(response);
            if (data.Count == 0)
            {
                Log.Information("API player {id} not found", playerId);
                return null;
            }
            var player = data[0];
            return new Player(player.username, player.country, Convert.ToInt32(player.user_id));
        }

        public static Room GetMatch(int matchId)
        {
            Log.Information("API get match {id}", matchId);
            ApiMatch data;
            using (var webClient = new WebClient())
            {
                var response = webClient.DownloadString("https://osu.ppy.sh/community/matches/" + matchId + "/history?after=0");
                data = JsonConvert.DeserializeObject<ApiMatch>(response);
                var room = new Room(matchId);
                foreach (var eventData in data.events)
                {
                    if (eventData.detail.type == "other" && eventData.game.end_time != null)
                    {
                        room.Name = eventData.detail.text;
                        var beatmapData = eventData.game.beatmap;
                        var beatmap = new Beatmap(beatmapData.id, beatmapData.beatmapset.id, beatmapData.beatmapset.artist, beatmapData.beatmapset.title, beatmapData.version, beatmapData.beatmapset.creator);
                        var game = new Game(room, eventData.id, beatmap);
                        game.Mods = ModsFromList(eventData.game.mods);

                        foreach (var scoreData in eventData.game.scores)
                        {
                            var playerData = data.users.First(x => x.id == scoreData.user_id);
                            var player = new Player(playerData.username, playerData.country_code, playerData.id);
                            Database.Database.AddPlayer(player);
                            var score = new Score(game, player, scoreData.score, TeamFromString(scoreData.multiplayer.team), scoreData.multiplayer.pass == 1);
                            score.Mods = ModsFromList(scoreData.mods);

                            game.Scores.Add(score);
                        }

                        room.Games.Add(game);
                    }

                    room.LastEventId = eventData.id;
                }

                if (room.LastEventId != data.latest_event_id)
                    UpdateRoom(room);

                return room;
            }
        }

        public static void UpdateRoom(Room room)
        {
            Log.Information("API refresh match {id}", room.Id);
            using (var webClient = new WebClient())
            {
                var response = webClient.DownloadString("https://osu.ppy.sh/community/matches/" + room.Id + "/history?after=" + room.LastEventId);
                var data = JsonConvert.DeserializeObject<ApiMatch>(response);
                foreach (var eventData in data.events)
                {
                    if (eventData.detail.type == "other" && eventData.game.end_time != null)
                    {
                        room.Name = eventData.detail.text;
                        var beatmapData = eventData.game.beatmap;
                        var beatmap = new Beatmap(beatmapData.id, beatmapData.beatmapset.id, beatmapData.beatmapset.artist, beatmapData.beatmapset.title, beatmapData.version, beatmapData.beatmapset.creator);
                        var game = new Game(room, eventData.id, beatmap);
                        game.Mods = ModsFromList(eventData.game.mods);

                        foreach (var scoreData in eventData.game.scores)
                        {
                            var playerData = data.users.First(x => x.id == scoreData.user_id);
                            var player = new Player(playerData.username, playerData.country_code, playerData.id);
                            var score = new Score(game, player, scoreData.score, TeamFromString(scoreData.multiplayer.team), scoreData.multiplayer.pass == 1);
                            score.Mods = ModsFromList(scoreData.mods);

                            game.Scores.Add(score);
                        }

                        room.Games.Add(game);
                    }

                    room.LastEventId = eventData.id;
                }

                if (room.LastEventId != data.latest_event_id)
                    UpdateRoom(room);
            }
        }
        #endregion

        #region Helper methods
        private static string SendRequest(string method, string parameters)
        {
            using (var webClient = new WebClient())
            {
                return webClient.DownloadString("https://osu.ppy.sh/api/" + method + "?k=" + Settings.ApiKey + "&" + parameters);
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
                case "red": return LobbyTeams.Red;
                case "blue": return LobbyTeams.Blue;
            }
            return LobbyTeams.None;
        }
        #endregion
    }
}
