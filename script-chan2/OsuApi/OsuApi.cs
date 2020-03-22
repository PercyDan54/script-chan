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
            return new Player()
            {
                Name = player.username,
                Country = player.country,
                Id = Convert.ToInt32(player.user_id)
            };
        }

        public static void UpdateGames(Match match)
        {
            Log.Information("API refresh match {id}", match.RoomId);
            var response = SendRequest("get_match", "mp=" + match.RoomId);
            var data = JsonConvert.DeserializeObject<ApiMatch>(response);
            foreach (var gameData in data.games)
            {
                if (match.Games.Any(x => x.Id == Convert.ToInt32(gameData.game_id)))
                    continue;

                if (gameData.end_time == null)
                    continue;

                var beatmap = Database.Database.GetBeatmap(Convert.ToInt32(gameData.beatmap_id));
                var game = new Game()
                {
                    Match = match,
                    Id = Convert.ToInt32(gameData.game_id),
                    Beatmap = beatmap,
                    Mods = ModsFromBitEnum(gameData.mods)
                };

                foreach (var scoreData in gameData.scores)
                {
                    var player = Database.Database.GetPlayer(scoreData.user_id);
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
                case "2": return LobbyTeams.Red;
                case "1": return LobbyTeams.Blue;
            }
            return LobbyTeams.None;
        }
        #endregion
    }
}
