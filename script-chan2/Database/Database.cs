using script_chan2.DataTypes;
using script_chan2.Enums;
using script_chan2.OsuIrc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.Database
{
    public static class Database
    {
        public static List<Tournament> Tournaments = new List<Tournament>();
        public static List<Webhook> Webhooks = new List<Webhook>();
        public static List<Mappool> Mappools = new List<Mappool>();
        public static List<Player> Players = new List<Player>();
        public static List<Team> Teams = new List<Team>();
        public static List<Match> Matches = new List<Match>();
        public static List<Beatmap> Beatmaps = new List<Beatmap>();

        public static void Initialize()
        {
            Log.Information("Database: init started");
            InitTournaments();
            InitTeams();
            InitTeamPlayers();
            InitWebhooks();
            InitMappools();
            InitMappoolMaps();
            InitTournamentWebhooks();
            InitMatches();
            InitMatchPlayers();
            InitMatchPicks();
            InitMatchGames();
            Log.Information("Database: init finished");
        }

        private static SQLiteConnection GetConnection()
        {
            var conn = new SQLiteConnection("Data Source=Database.sqlite;Version=3");
            conn.Open();
            return conn;
        }

        #region Settings
        public static Dictionary<string, string> GetSettings()
        {
            Log.Information("Database: getting settings");
            var returnValue = new Dictionary<string, string>();
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT name, value FROM UserSettings", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    returnValue.Add(reader["name"].ToString(), reader["value"].ToString());
                }
                reader.Close();
                conn.Close();
            }
            return returnValue;
        }

        public static void UpdateSettings(string name, string value)
        {
            Log.Information("Database: updating settings");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("UPDATE UserSettings SET value = @value WHERE name = @name", conn))
            {
                command.Parameters.AddWithValue("@value", value);
                command.Parameters.AddWithValue("@name", name);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        #endregion

        #region Tournaments
        private static void InitTournaments()
        {
            Log.Information("Database: init tournaments");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, name, gameMode, teamMode, winCondition, acronym, teamSize, roomSize, pointsForSecondBan, allPicksFreemod, mpTimerCommand, mpTimerAfterGame, mpTimerAfterPick, welcomeString FROM Tournaments", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    var name = reader["name"].ToString();
                    var gameMode = Enums.GameModes.Standard;
                    switch (reader["gameMode"].ToString())
                    {
                        case "Standard": gameMode = Enums.GameModes.Standard; break;
                        case "Taiko": gameMode = Enums.GameModes.Taiko; break;
                        case "Catch": gameMode = Enums.GameModes.Catch; break;
                        case "Mania": gameMode = Enums.GameModes.Mania; break;
                    }
                    var teamMode = Enums.TeamModes.TeamVS;
                    switch (reader["teamMode"].ToString())
                    {
                        case "HeadToHead": teamMode = Enums.TeamModes.HeadToHead; break;
                        case "TeamVS": teamMode = Enums.TeamModes.TeamVS; break;
                    }
                    var winCondition = Enums.WinConditions.ScoreV2;
                    switch (reader["winCondition"].ToString())
                    {
                        case "Score": winCondition = Enums.WinConditions.Score; break;
                        case "ScoreV2": winCondition = Enums.WinConditions.ScoreV2; break;
                        case "Accuracy": winCondition = Enums.WinConditions.Accuracy; break;
                        case "Combo": winCondition = Enums.WinConditions.Combo; break;
                    }
                    var acronym = reader["acronym"].ToString();
                    var teamSize = Convert.ToInt32(reader["teamSize"]);
                    var roomSize = Convert.ToInt32(reader["roomSize"]);
                    var pointsForSecondBan = Convert.ToInt32(reader["pointsForSecondBan"]);
                    var allPicksFreemod = Convert.ToBoolean(reader["allPicksFreemod"]);
                    var mpTimerCommand = Convert.ToInt32(reader["mpTimerCommand"]);
                    var mpTimerAfterGame = Convert.ToInt32(reader["mpTimerAfterGame"]);
                    var mpTimerAfterPick = Convert.ToInt32(reader["mpTimerAfterPick"]);
                    var welcomeString = reader["welcomeString"].ToString();
                    var tournament = new Tournament(id)
                    {
                        Name = name,
                        GameMode = gameMode,
                        TeamMode = teamMode,
                        WinCondition = winCondition,
                        Acronym = acronym,
                        TeamSize = teamSize,
                        RoomSize = roomSize,
                        PointsForSecondBan = pointsForSecondBan,
                        AllPicksFreemod = allPicksFreemod,
                        MpTimerCommand = mpTimerCommand,
                        MpTimerAfterGame = mpTimerAfterGame,
                        MpTimerAfterPick = mpTimerAfterPick,
                        WelcomeString = welcomeString
                    };
                    Tournaments.Add(tournament);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddTournament(Tournament tournament)
        {
            Log.Information("Database: add new tournament '{name}'", tournament.Name);
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"INSERT INTO Tournaments (name, gameMode, teamMode, winCondition, acronym, teamSize, roomSize, pointsForSecondBan, allPicksFreemod, mpTimerCommand, mpTimerAfterGame, mpTimerAfterPick, welcomeString)
                VALUES (@name, @gameMode, @teamMode, @winCondition, @acronym, @teamSize, @roomSize, @pointsForSecondBan, @allPicksFreemod, @mpTimerCommand, @mpTimerAfterGame, @mpTimerAfterPick, @welcomeString)", conn))
                {
                    command.Parameters.AddWithValue("@name", tournament.Name);
                    command.Parameters.AddWithValue("@gameMode", tournament.GameMode.ToString());
                    command.Parameters.AddWithValue("@teamMode", tournament.TeamMode.ToString());
                    command.Parameters.AddWithValue("@winCondition", tournament.WinCondition.ToString());
                    command.Parameters.AddWithValue("@acronym", tournament.Acronym);
                    command.Parameters.AddWithValue("@teamSize", tournament.TeamSize);
                    command.Parameters.AddWithValue("@roomSize", tournament.RoomSize);
                    command.Parameters.AddWithValue("@pointsForSecondBan", tournament.PointsForSecondBan);
                    command.Parameters.AddWithValue("@allPicksFreemod", tournament.AllPicksFreemod);
                    command.Parameters.AddWithValue("@mpTimerCommand", tournament.MpTimerCommand);
                    command.Parameters.AddWithValue("@mpTimerAfterGame", tournament.MpTimerAfterGame);
                    command.Parameters.AddWithValue("@mpTimerAfterPick", tournament.MpTimerAfterPick);
                    command.Parameters.AddWithValue("@welcomeString", tournament.WelcomeString);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("SELECT last_insert_rowid()", conn))
                {
                    resultValue = Convert.ToInt32(command.ExecuteScalar());
                }
                conn.Close();
            }
            Tournaments.Add(tournament);
            return resultValue;
        }

        public static void DeleteTournament(Tournament tournament)
        {
            Log.Information("Database: delete tournament '{name}'", tournament.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("DELETE FROM Tournaments WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@id", tournament.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
            Tournaments.Remove(tournament);
        }

        public static void UpdateTournament(Tournament tournament)
        {
            Log.Information("Database: update tournament '{name}'", tournament.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"UPDATE Tournaments
                SET name = @name, gameMode = @gameMode, teamMode = @teamMode, winCondition = @winCondition, acronym = @acronym, teamSize = @teamSize, roomSize = @roomSize, pointsForSecondBan = @pointsForSecondBan, allPicksFreemod = @allPicksFreemod, mpTimerCommand = @mpTimerCommand, mpTimerAfterGame = @mpTimerAfterGame, mpTimerAfterPick = @mpTimerAfterPick, welcomeString = @welcomeString
                WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@name", tournament.Name);
                command.Parameters.AddWithValue("@gameMode", tournament.GameMode.ToString());
                command.Parameters.AddWithValue("@teamMode", tournament.TeamMode.ToString());
                command.Parameters.AddWithValue("@winCondition", tournament.WinCondition.ToString());
                command.Parameters.AddWithValue("@acronym", tournament.Acronym);
                command.Parameters.AddWithValue("@teamSize", tournament.TeamSize);
                command.Parameters.AddWithValue("@roomSize", tournament.RoomSize);
                command.Parameters.AddWithValue("@pointsForSecondBan", tournament.PointsForSecondBan);
                command.Parameters.AddWithValue("@allPicksFreemod", tournament.AllPicksFreemod);
                command.Parameters.AddWithValue("@mpTimerCommand", tournament.MpTimerCommand);
                command.Parameters.AddWithValue("@mpTimerAfterGame", tournament.MpTimerAfterGame);
                command.Parameters.AddWithValue("@mpTimerAfterPick", tournament.MpTimerAfterPick);
                command.Parameters.AddWithValue("@welcomeString", tournament.WelcomeString);
                command.Parameters.AddWithValue("@id", tournament.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        #endregion

        #region Webhooks
        public static void InitWebhooks()
        {
            Log.Information("Database: init webhooks");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, name, url FROM Webhooks", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    var name = reader["name"].ToString();
                    var url = reader["URL"].ToString();
                    var webhook = new Webhook(id)
                    {
                        Name = name,
                        URL = url
                    };
                    Webhooks.Add(webhook);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddWebhook(Webhook webhook)
        {
            Log.Information("Database: add new webhook '{name}'", webhook.Name);
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("INSERT INTO Webhooks (name, url) VALUES (@name, @url)", conn))
                {
                    command.Parameters.AddWithValue("@name", webhook.Name);
                    command.Parameters.AddWithValue("@url", webhook.URL);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("SELECT last_insert_rowid()", conn))
                {
                    resultValue = Convert.ToInt32(command.ExecuteScalar());
                }
                conn.Close();
            }
            Webhooks.Add(webhook);
            return resultValue;
        }

        public static void DeleteWebhook(Webhook webhook)
        {
            Log.Information("Database: delete webhook '{name}'", webhook.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("DELETE FROM Webhooks WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@id", webhook.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
            Webhooks.Remove(webhook);
        }

        public static void UpdateWebhook(Webhook webhook)
        {
            Log.Information("Database: update webhook '{name}'", webhook.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"UPDATE Webhooks
                SET name = @name, url = @url
                WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@name", webhook.Name);
                command.Parameters.AddWithValue("@url", webhook.URL);
                command.Parameters.AddWithValue("@id", webhook.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        private static void InitTournamentWebhooks()
        {
            Log.Information("Database: init tournament webhooks");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT tournament, webhook FROM WebhookLinks", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var tournamentId = Convert.ToInt32(reader["tournament"]);
                    var tournament = Tournaments.First(x => x.Id == tournamentId);
                    var webhookId = Convert.ToInt32(reader["webhook"]);
                    var webhook = Webhooks.First(x => x.Id == webhookId);
                    tournament.Webhooks.Add(webhook);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static void AddWebhookToTournament(Webhook webhook, Tournament tournament)
        {
            Log.Information("Database: add webhook '{webhook}' to tournament '{tournament}'", webhook.Name, tournament.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"INSERT INTO WebhookLinks (tournament, webhook)
                SELECT @tournament, @webhook
                WHERE NOT EXISTS (SELECT 1 FROM WebhookLinks WHERE tournament = @tournament2 AND webhook = @webhook2)", conn))
            {
                command.Parameters.AddWithValue("@webhook", webhook.Id);
                command.Parameters.AddWithValue("@tournament", tournament.Id);
                command.Parameters.AddWithValue("@webhook2", webhook.Id);
                command.Parameters.AddWithValue("@tournament2", tournament.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void RemoveWebhookFromTournament(Webhook webhook, Tournament tournament)
        {
            Log.Information("Database: remove webhook '{webhook}' from tournament '{tournament}'", webhook.Name, tournament.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("DELETE FROM WebhookLinks WHERE webhook = @webhook AND tournament = @tournament", conn))
            {
                command.Parameters.AddWithValue("@webhook", webhook.Id);
                command.Parameters.AddWithValue("@tournament", tournament.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        #endregion

        #region Mappools
        public static void InitMappools()
        {
            Log.Information("Database: init mappools");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, name, tournament FROM Mappools", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    var tournament = Tournaments.First(x => x.Id == Convert.ToInt32(reader["tournament"]));
                    var name = reader["name"].ToString();
                    var mappool = new Mappool(id)
                    {
                        Name = name,
                        Tournament = tournament
                    };
                    Mappools.Add(mappool);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddMappool(Mappool mappool)
        {
            Log.Information("Database: add new mappool '{name}'", mappool.Name);
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("INSERT INTO Mappools (name, tournament) VALUES (@name, @tournament)", conn))
                {
                    command.Parameters.AddWithValue("@tournament", mappool.Tournament.Id);
                    command.Parameters.AddWithValue("@name", mappool.Name);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("SELECT last_insert_rowid()", conn))
                {
                    resultValue = Convert.ToInt32(command.ExecuteScalar());
                }
                conn.Close();
            }
            Mappools.Add(mappool);
            return resultValue;
        }

        public static void DeleteMappool(Mappool mappool)
        {
            Log.Information("Database: delete mappool '{name}'", mappool.Name);
            using (var conn = GetConnection())
            using (var transaction = conn.BeginTransaction())
            {
                using (var command = new SQLiteCommand("DELETE FROM MappoolMaps WHERE mappool = @id", conn))
                {
                    command.Parameters.AddWithValue("@id", mappool.Id);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("DELETE FROM Mappools WHERE id = @id", conn))
                {
                    command.Parameters.AddWithValue("@id", mappool.Id);
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
                conn.Close();
            }
            Mappools.Remove(mappool);
        }

        public static void UpdateMappool(Mappool mappool)
        {
            Log.Information("Database: update mappool '{name}'", mappool.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"UPDATE Mappools
                SET name = @name, tournament = @tournament
                WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@name", mappool.Name);
                command.Parameters.AddWithValue("@tournament", mappool.Tournament.Id);
                command.Parameters.AddWithValue("@id", mappool.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void AddBeatmap(Beatmap beatmap)
        {
            Log.Information("Database: add new beatmap '{id}'", beatmap.Id);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"INSERT INTO Beatmaps (id, beatmapsetId, artist, title, version, creator, bpm, ar, cs)
                VALUES (@id, @beatmapsetId, @artist, @title, @version, @creator, @bpm, @ar, @cs)", conn))
            {
                command.Parameters.AddWithValue("@id", beatmap.Id);
                command.Parameters.AddWithValue("@beatmapsetId", beatmap.SetId);
                command.Parameters.AddWithValue("@artist", beatmap.Artist);
                command.Parameters.AddWithValue("@title", beatmap.Title);
                command.Parameters.AddWithValue("@version", beatmap.Version);
                command.Parameters.AddWithValue("@creator", beatmap.Creator);
                command.Parameters.AddWithValue("@bpm", beatmap.BPM);
                command.Parameters.AddWithValue("@ar", beatmap.AR);
                command.Parameters.AddWithValue("@cs", beatmap.CS);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static Beatmap GetBeatmap(int id)
        {
            Log.Information("Database: get beatmap '{id}'", id);
            Beatmap returnValue = Beatmaps.FirstOrDefault(x => x.Id == id);
            if (returnValue != null)
                return returnValue;
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, beatmapsetId, artist, title, version, creator, bpm, ar, cs FROM Beatmaps WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@id", id);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        var setId = Convert.ToInt32(reader["beatmapsetId"]);
                        var artist = reader["artist"].ToString();
                        var title = reader["title"].ToString();
                        var version = reader["version"].ToString();
                        var creator = reader["creator"].ToString();
                        var bpm = Convert.ToDecimal(reader["bpm"]);
                        var ar = Convert.ToDecimal(reader["ar"]);
                        var cs = Convert.ToDecimal(reader["cs"]);
                        returnValue = new Beatmap()
                        {
                            Id = id,
                            SetId = setId,
                            Artist = artist,
                            Title = title,
                            Version = version,
                            Creator = creator,
                            BPM = bpm,
                            AR = ar,
                            CS = cs
                        };
                    }
                    else
                    {
                        returnValue = OsuApi.OsuApi.GetBeatmap(id);
                        if (returnValue != null)
                            AddBeatmap(returnValue);
                    }
                    reader.Close();
                }
                conn.Close();
            }
            if (returnValue != null)
                Beatmaps.Add(returnValue);
            return returnValue;
        }

        public static void InitMappoolMaps()
        {
            Log.Information("Database: init mappool maps");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, mappool, beatmap, listIndex, mods, tag FROM MappoolMaps ORDER BY listIndex ASC", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    var mappool = Mappools.First(x => x.Id == Convert.ToInt32(reader["mappool"]));
                    var beatmap = GetBeatmap(Convert.ToInt32(reader["beatmap"]));
                    var listIndex = Convert.ToInt32(reader["listIndex"]);
                    var tag = reader["tag"].ToString();
                    var mappoolMap = new MappoolMap(id)
                    {
                        Mappool = mappool,
                        Beatmap = beatmap,
                        ListIndex = listIndex,
                        Tag = tag
                    };
                    foreach (string mod in reader["mods"].ToString().Split(','))
                    {
                        if (Enum.TryParse(mod, out GameMods gameMod))
                        {
                            mappoolMap.Mods.Add(gameMod);
                        }
                    }
                    mappool.Beatmaps.Add(mappoolMap);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddMappoolMap(MappoolMap map)
        {
            Log.Information("Database: add map '{map}' to mappool '{mappool}'", map.Beatmap.Id, map.Mappool.Name);
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("INSERT INTO MappoolMaps (mappool, beatmap, listIndex, mods, tag) VALUES (@mappool, @beatmap, @listIndex, @mods, @tag)", conn))
                {
                    command.Parameters.AddWithValue("@mappool", map.Mappool.Id);
                    command.Parameters.AddWithValue("@beatmap", map.Beatmap.Id);
                    command.Parameters.AddWithValue("@listIndex", map.ListIndex);
                    command.Parameters.AddWithValue("@mods", string.Join(",", map.Mods));
                    command.Parameters.AddWithValue("@tag", map.Tag);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("SELECT last_insert_rowid()", conn))
                {
                    resultValue = Convert.ToInt32(command.ExecuteScalar());
                }
                conn.Close();
            }
            return resultValue;
        }

        public static void DeleteMappoolMap(MappoolMap map)
        {
            Log.Information("Database: remove mappool map '{map}' from mappool '{mappool}'", map.Beatmap.Id, map.Mappool.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("DELETE FROM MappoolMaps WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@id", map.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void UpdateMappoolMap(MappoolMap map)
        {
            Log.Information("Database: update map '{map}' in mappool '{mappool}'", map.Beatmap.Id, map.Mappool.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"UPDATE MappoolMaps
                SET beatmap = @beatmap, listIndex = @listIndex, mods = @mods, tag = @tag
                WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@beatmap", map.Beatmap.Id);
                command.Parameters.AddWithValue("@listIndex", map.ListIndex);
                command.Parameters.AddWithValue("@mods", string.Join(",", map.Mods));
                command.Parameters.AddWithValue("@id", map.Id);
                command.Parameters.AddWithValue("@tag", map.Tag);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        #endregion

        #region Players
        public static void AddPlayer(Player player)
        {
            Log.Information("Database: add new player '{name}'", player.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("INSERT OR REPLACE INTO Players (id, name, country) VALUES (@id, @name, @country)", conn))
            {
                command.Parameters.AddWithValue("@id", player.Id);
                command.Parameters.AddWithValue("@name", player.Name);
                command.Parameters.AddWithValue("@country", player.Country);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static Player GetPlayer(string idOrName)
        {
            Log.Information("Database: get player '{name}'", idOrName);
            Player returnValue = Players.FirstOrDefault(x => x.Id.ToString() == idOrName || x.Name == idOrName);
            if (returnValue != null)
                return returnValue;
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, name, country FROM Players WHERE id = @id OR name = @name", conn))
            {
                command.Parameters.AddWithValue("@id", idOrName);
                command.Parameters.AddWithValue("@name", idOrName);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        var id = Convert.ToInt32(reader["id"]);
                        var name = reader["name"].ToString();
                        var country = reader["country"].ToString();
                        returnValue = new Player()
                        {
                            Name = name,
                            Country = country,
                            Id = id
                        };
                    }
                    else
                    {
                        returnValue = OsuApi.OsuApi.GetPlayer(idOrName);
                        if (returnValue != null)
                            AddPlayer(returnValue);
                    }
                    reader.Close();
                }
                conn.Close();
            }
            if (returnValue != null)
                Players.Add(returnValue);
            return returnValue;
        }

        public static void AddPlayerToTeam(Player player, Team team)
        {
            Log.Information("Database: add player '{player}' to team '{team}'", player.Name, team.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("INSERT INTO TeamPlayers (player, team) VALUES (@player, @team)", conn))
            {
                command.Parameters.AddWithValue("@player", player.Id);
                command.Parameters.AddWithValue("@team", team.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void RemovePlayerFromTeam(Player player, Team team)
        {
            Log.Information("Database: remove player '{player}' from team '{team}'", player.Name, team.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("DELETE FROM TeamPlayers WHERE player = @player AND team = @team", conn))
            {
                command.Parameters.AddWithValue("@player", player.Id);
                command.Parameters.AddWithValue("@team", team.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        #endregion

        #region Teams
        public static void InitTeams()
        {
            Log.Information("Database: init teams");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, name, tournament FROM Teams", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    var tournament = Tournaments.First(x => x.Id == Convert.ToInt32(reader["tournament"]));
                    var name = reader["name"].ToString();
                    var team = new Team(id)
                    {
                        Tournament = tournament,
                        Name = name,
                    };
                    Teams.Add(team);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddTeam(Team team)
        {
            Log.Information("Database: add team '{name}'", team.Name);
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("INSERT INTO Teams (tournament, name) VALUES (@tournament, @name)", conn))
                {
                    command.Parameters.AddWithValue("@tournament", team.Tournament.Id);
                    command.Parameters.AddWithValue("@name", team.Name);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("SELECT last_insert_rowid()", conn))
                {
                    resultValue = Convert.ToInt32(command.ExecuteScalar());
                }
                conn.Close();
            }
            Teams.Add(team);
            return resultValue;
        }

        public static void UpdateTeam(Team team)
        {
            Log.Information("Database: update team '{name}'", team.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("UPDATE Teams SET name = @name WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@name", team.Name);
                command.Parameters.AddWithValue("@id", team.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void DeleteTeam(Team team)
        {
            Log.Information("Database: delete team '{name}'", team.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("DELETE FROM Teams WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@id", team.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
            Teams.Remove(team);
        }

        public static void InitTeamPlayers()
        {
            Log.Information("Database: init team players");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT player, team FROM TeamPlayers", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var playerId = Convert.ToInt32(reader["player"]);
                    var teamId = Convert.ToInt32(reader["team"]);
                    Teams.First(x => x.Id == teamId).Players.Add(GetPlayer(playerId.ToString()));
                }
                reader.Close();
                conn.Close();
            }
        }
        #endregion

        #region Matches
        public static void InitMatches()
        {
            Log.Information("Database: init matches");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"SELECT id, tournament, mappool, name, roomId, gameMode, teamMode, winCondition, teamBlue, teamBluePoints, teamRed, teamRedPoints, teamSize, roomSize,
                rollWinner, firstPicker, BO, enableWebhooks, mpTimerCommand, mpTimerAfterGame, mpTimerAfterPick, pointsForSecondBan, allPicksFreemod, status, warmupMode FROM Matches", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    var tournament = Tournaments.First(x => x.Id == Convert.ToInt32(reader["tournament"]));
                    Mappool mappool = null;
                    if (reader["mappool"] != DBNull.Value)
                        mappool = Mappools.First(x => x.Id == Convert.ToInt32(reader["mappool"]));
                    var name = reader["name"].ToString();
                    var roomId = Convert.ToInt32(reader["roomId"]);
                    var gameMode = GameModes.Standard;
                    switch (reader["gameMode"].ToString())
                    {
                        case "Standard": gameMode = GameModes.Standard; break;
                        case "Taiko": gameMode = GameModes.Taiko; break;
                        case "Catch": gameMode = GameModes.Catch; break;
                        case "Mania": gameMode = GameModes.Mania; break;
                    }
                    var teamMode = TeamModes.TeamVS;
                    switch (reader["teamMode"].ToString())
                    {
                        case "HeadToHead": teamMode = TeamModes.HeadToHead; break;
                        case "TeamVS": teamMode = TeamModes.TeamVS; break;
                    }
                    var winCondition = WinConditions.ScoreV2;
                    switch (reader["winCondition"].ToString())
                    {
                        case "Score": winCondition = WinConditions.Score; break;
                        case "ScoreV2": winCondition = WinConditions.ScoreV2; break;
                        case "Accuracy": winCondition = WinConditions.Accuracy; break;
                        case "Combo": winCondition = WinConditions.Combo; break;
                    }
                    Player rollWinnerPlayer = null;
                    Team rollWinnerTeam = null;
                    Player firstPickerPlayer = null;
                    Team firstPickerTeam = null;
                    Team teamRed = null;
                    Team teamBlue = null;
                    if (teamMode == TeamModes.TeamVS)
                    {
                        if (reader["rollWinner"] != DBNull.Value)
                            rollWinnerTeam = Teams.First(x => x.Id == Convert.ToInt32(reader["rollWinner"]));
                        if (reader["firstPicker"] != DBNull.Value)
                            firstPickerTeam = Teams.First(x => x.Id == Convert.ToInt32(reader["firstPicker"]));
                        if (reader["teamRed"] != DBNull.Value)
                            teamRed = Teams.First(x => x.Id == Convert.ToInt32(reader["teamRed"]));
                        if (reader["teamBlue"] != DBNull.Value)
                            teamBlue = Teams.First(x => x.Id == Convert.ToInt32(reader["teamBlue"]));
                    }
                    else
                    {
                        if (reader["rollWinner"] != DBNull.Value)
                            rollWinnerPlayer = GetPlayer(reader["rollWinner"].ToString());
                        if (reader["firstPicker"] != DBNull.Value)
                            firstPickerPlayer = GetPlayer(reader["firstPicker"].ToString());
                    }
                    var teamRedPoints = Convert.ToInt32(reader["teamRedPoints"]);
                    var teamBluePoints = Convert.ToInt32(reader["teamBluePoints"]);
                    var teamSize = Convert.ToInt32(reader["teamSize"]);
                    var roomSize = Convert.ToInt32(reader["roomSize"]);
                    var bo = Convert.ToInt32(reader["BO"]);
                    var enableWebhooks = Convert.ToBoolean(reader["enableWebhooks"]);
                    var mpTimerCommand = Convert.ToInt32(reader["mpTimerCommand"]);
                    var mpTimerAfterGame = Convert.ToInt32(reader["mpTimerAfterGame"]);
                    var mpTimerAfterPick = Convert.ToInt32(reader["mpTimerAfterPick"]);
                    var pointsForSecondBan = Convert.ToInt32(reader["pointsForSecondBan"]);
                    var allPicksFreemod = Convert.ToBoolean(reader["allPicksFreemod"]);
                    var status = MatchStatus.New;
                    var warmupMode = Convert.ToBoolean(reader["warmupMode"]);
                    switch (reader["status"].ToString())
                    {
                        case "New": status = MatchStatus.New; break;
                        case "InProgress": status = MatchStatus.InProgress; break;
                        case "Finished": status = MatchStatus.Finished; break;
                    }
                    var match = new Match(id)
                    {
                        Tournament = tournament,
                        Mappool = mappool,
                        Name = name,
                        RoomId = roomId,
                        GameMode = gameMode,
                        TeamMode = teamMode,
                        WinCondition = winCondition,
                        TeamBlue = teamBlue,
                        TeamBluePoints = teamBluePoints,
                        TeamRed = teamRed,
                        TeamRedPoints = teamRedPoints,
                        TeamSize = teamSize,
                        RoomSize = roomSize,
                        RollWinnerTeam = rollWinnerTeam,
                        RollWinnerPlayer = rollWinnerPlayer,
                        FirstPickerTeam = firstPickerTeam,
                        FirstPickerPlayer = firstPickerPlayer,
                        BO = bo,
                        EnableWebhooks = enableWebhooks,
                        MpTimerCommand = mpTimerCommand,
                        MpTimerAfterGame = mpTimerAfterGame,
                        MpTimerAfterPick = mpTimerAfterPick,
                        PointsForSecondBan = pointsForSecondBan,
                        AllPicksFreemod = allPicksFreemod,
                        Status = status,
                        WarmupMode = warmupMode
                    };
                    Matches.Add(match);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddMatch(Match match)
        {
            Log.Information("Database: add new match '{name}'", match.Name);
            int resultValue;
            using (var conn = GetConnection())
            using (var transaction = conn.BeginTransaction())
            {
                using (var command = new SQLiteCommand("INSERT INTO Matches (tournament, mappool, name, roomId, gameMode, teamMode, winCondition, teamBlue, teamBluePoints, teamRed, teamRedPoints, teamSize, roomSize, rollWinner, firstPicker, BO, enableWebhooks, mpTimerCommand, mpTimerAfterGame, mpTimerAfterPick, pointsForSecondBan, allPicksFreemod, status, warmupMode)" +
                    "VALUES (@tournament, @mappool, @name, @roomId, @gameMode, @teamMode, @winCondition, @teamBlue, @teamBluePoints, @teamRed, @teamRedPoints, @teamSize, @roomSize, @rollWinner, @firstPicker, @BO, @enableWebhooks, @mpTimerCommand, @mpTimerAfterGame, @mpTimerAfterPick, @pointsForSecondBan, @allPicksFreemod, @status, @warmupMode)", conn))
                {
                    command.Parameters.AddWithValue("@tournament", match.Tournament.Id);
                    if (match.Mappool == null)
                        command.Parameters.AddWithValue("@mappool", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@mappool", match.Mappool.Id);
                    command.Parameters.AddWithValue("@name", match.Name);
                    command.Parameters.AddWithValue("@roomId", match.RoomId);
                    command.Parameters.AddWithValue("@gameMode", match.GameMode.ToString());
                    command.Parameters.AddWithValue("@teamMode", match.TeamMode.ToString());
                    command.Parameters.AddWithValue("@winCondition", match.WinCondition.ToString());
                    if (match.TeamBlue != null)
                        command.Parameters.AddWithValue("@teamBlue", match.TeamBlue.Id);
                    else
                        command.Parameters.AddWithValue("@teamBlue", DBNull.Value);
                    if (match.TeamRed != null)
                        command.Parameters.AddWithValue("@teamRed", match.TeamRed.Id);
                    else
                        command.Parameters.AddWithValue("@teamRed", DBNull.Value);
                    if (match.TeamMode == TeamModes.TeamVS)
                    {
                        if (match.RollWinnerTeam != null)
                            command.Parameters.AddWithValue("@rollWinner", match.RollWinnerTeam.Id);
                        else
                            command.Parameters.AddWithValue("@rollWinner", DBNull.Value);
                        if (match.FirstPickerTeam != null)
                            command.Parameters.AddWithValue("@firstPicker", match.FirstPickerTeam.Id);
                        else
                            command.Parameters.AddWithValue("@firstPicker", DBNull.Value);
                    }
                    else
                    {
                        if (match.RollWinnerPlayer != null)
                            command.Parameters.AddWithValue("@rollWinner", match.RollWinnerPlayer.Id);
                        else
                            command.Parameters.AddWithValue("@rollWinner", DBNull.Value);
                        if (match.FirstPickerPlayer != null)
                            command.Parameters.AddWithValue("@firstPicker", match.FirstPickerPlayer.Id);
                        else
                            command.Parameters.AddWithValue("@firstPicker", DBNull.Value);
                    }
                    command.Parameters.AddWithValue("@teamBluePoints", match.TeamBluePoints);
                    command.Parameters.AddWithValue("@teamRedPoints", match.TeamRedPoints);
                    command.Parameters.AddWithValue("@teamSize", match.TeamSize);
                    command.Parameters.AddWithValue("@roomSize", match.RoomSize);
                    command.Parameters.AddWithValue("@BO", match.BO);
                    command.Parameters.AddWithValue("@enableWebhooks", match.EnableWebhooks);
                    command.Parameters.AddWithValue("@mpTimerCommand", match.MpTimerCommand);
                    command.Parameters.AddWithValue("@mpTimerAfterGame", match.MpTimerAfterGame);
                    command.Parameters.AddWithValue("@mpTimerAfterPick", match.MpTimerAfterPick);
                    command.Parameters.AddWithValue("@pointsForSecondBan", match.PointsForSecondBan);
                    command.Parameters.AddWithValue("@allPicksFreemod", match.AllPicksFreemod);
                    command.Parameters.AddWithValue("@status", match.Status.ToString());
                    command.Parameters.AddWithValue("@warmupMode", match.WarmupMode);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("SELECT last_insert_rowid()", conn))
                {
                    resultValue = Convert.ToInt32(command.ExecuteScalar());
                }
                if (match.TeamMode == TeamModes.HeadToHead)
                {
                    foreach (var player in match.Players)
                    {
                        using (var command = new SQLiteCommand("INSERT INTO MatchPlayers (match, player, points) VALUES (@match, @player, @points)", conn))
                        {
                            command.Parameters.AddWithValue("@match", resultValue);
                            command.Parameters.AddWithValue("@player", player.Key.Id);
                            command.Parameters.AddWithValue("@points", player.Value);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                foreach (var pick in match.Picks)
                {
                    using (var command = new SQLiteCommand("INSERT INTO MatchPicks (match, beatmap, picker, ban) VALUES (@match, @beatmap, @picker, False)", conn))
                    {
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.Parameters.AddWithValue("@beatmap", pick.Map.Id);
                        if (match.TeamMode == TeamModes.TeamVS)
                            command.Parameters.AddWithValue("@picker", pick.Team.Id);
                        else if (match.TeamMode == TeamModes.HeadToHead)
                            command.Parameters.AddWithValue("@picker", pick.Player.Id);
                        command.ExecuteNonQuery();
                    }
                }
                foreach (var pick in match.Bans)
                {
                    using (var command = new SQLiteCommand("INSERT INTO MatchPicks (match, beatmap, picker, ban) VALUES (@match, @beatmap, @picker, True)", conn))
                    {
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.Parameters.AddWithValue("@beatmap", pick.Map.Id);
                        if (match.TeamMode == TeamModes.TeamVS)
                            command.Parameters.AddWithValue("@picker", pick.Team.Id);
                        else if (match.TeamMode == TeamModes.HeadToHead)
                            command.Parameters.AddWithValue("@picker", pick.Player.Id);
                        command.ExecuteNonQuery();
                    }
                }
                foreach (var game in match.Games)
                {
                    using (var command = new SQLiteCommand("INSERT INTO Games (id, match, beatmap, mods, counted) VALUES (@id, @match, @beatmap, @mods, @counted)", conn))
                    {
                        command.Parameters.AddWithValue("@id", game.Id);
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.Parameters.AddWithValue("@beatmap", game.Beatmap.Id);
                        command.Parameters.AddWithValue("@mods", string.Join(",", game.Mods));
                        command.Parameters.AddWithValue("@counted", game.Counted);
                        command.ExecuteNonQuery();
                    }
                    foreach (var score in game.Scores)
                    {
                        using (var command = new SQLiteCommand("INSERT INTO Scores (player, game, mods, score, team, passed) VALUES (@player, @game, @mods, @score, @team, @passed)", conn))
                        {
                            command.Parameters.AddWithValue("@player", score.Player.Id);
                            command.Parameters.AddWithValue("@game", game.Id);
                            command.Parameters.AddWithValue("@mods", string.Join(",", score.Mods));
                            command.Parameters.AddWithValue("@score", score.Points);
                            command.Parameters.AddWithValue("@team", score.Team.ToString());
                            command.Parameters.AddWithValue("@passed", score.Passed);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                transaction.Commit();
                conn.Close();
            }
            Matches.Add(match);
            return resultValue;
        }

        public static void UpdateMatch(Match match)
        {
            Log.Information("Database: update match '{name}'", match.Name);
            using (var conn = GetConnection())
            using (var transaction = conn.BeginTransaction())
            {
                using (var command = new SQLiteCommand(@"UPDATE Matches
                SET tournament = @tournament, mappool = @mappool, name = @name, roomId = @roomId, gameMode = @gameMode, teamMode = @teamMode, winCondition = @winCondition, teamBlue = @teamBlue, teamBluePoints = @teamBluePoints,
                teamRed = @teamRed, teamRedPoints = @teamRedPoints, teamSize = @teamSize, roomSize = @roomSize, rollWinner = @rollWinner, firstPicker = @firstPicker, BO = @BO,
                enableWebhooks = @enableWebhooks, mpTimerCommand = @mpTimerCommand, mpTimerAfterGame = @mpTimerAfterGame, mpTimerAfterPick = @mpTimerAfterPick, pointsForSecondBan = @pointsForSecondBan,
                allPicksFreemod = @allPicksFreemod, status = @status, warmupMode = @warmupMode
                WHERE id = @id", conn))
                {
                    command.Parameters.AddWithValue("@tournament", match.Tournament.Id);
                    if (match.Mappool == null)
                        command.Parameters.AddWithValue("@mappool", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@mappool", match.Mappool.Id);
                    command.Parameters.AddWithValue("@name", match.Name);
                    command.Parameters.AddWithValue("@roomId", match.RoomId);
                    command.Parameters.AddWithValue("@gameMode", match.GameMode.ToString());
                    command.Parameters.AddWithValue("@teamMode", match.TeamMode.ToString());
                    command.Parameters.AddWithValue("@winCondition", match.WinCondition.ToString());
                    if (match.TeamMode == Enums.TeamModes.TeamVS)
                    {
                        if (match.RollWinnerTeam != null)
                            command.Parameters.AddWithValue("@rollWinner", match.RollWinnerTeam.Id);
                        else
                            command.Parameters.AddWithValue("@rollWinner", DBNull.Value);
                        if (match.FirstPickerTeam != null)
                            command.Parameters.AddWithValue("@firstPicker", match.FirstPickerTeam.Id);
                        else
                            command.Parameters.AddWithValue("@firstPicker", DBNull.Value);
                        if (match.TeamBlue != null)
                            command.Parameters.AddWithValue("@teamBlue", match.TeamBlue.Id);
                        else
                            command.Parameters.AddWithValue("@teamBlue", DBNull.Value);
                        if (match.TeamRed != null)
                            command.Parameters.AddWithValue("@teamRed", match.TeamRed.Id);
                        else
                            command.Parameters.AddWithValue("@teamRed", DBNull.Value);
                    }
                    else
                    {
                        if (match.RollWinnerPlayer != null)
                            command.Parameters.AddWithValue("@rollWinner", match.RollWinnerPlayer.Id);
                        else
                            command.Parameters.AddWithValue("@rollWinner", DBNull.Value);
                        if (match.FirstPickerPlayer != null)
                            command.Parameters.AddWithValue("@firstPicker", match.FirstPickerPlayer.Id);
                        else
                            command.Parameters.AddWithValue("@firstPicker", DBNull.Value);
                    }
                    command.Parameters.AddWithValue("@teamBluePoints", match.TeamBluePoints);
                    command.Parameters.AddWithValue("@teamRedPoints", match.TeamRedPoints);
                    command.Parameters.AddWithValue("@teamSize", match.TeamSize);
                    command.Parameters.AddWithValue("@roomSize", match.RoomSize);
                    command.Parameters.AddWithValue("@BO", match.BO);
                    command.Parameters.AddWithValue("@enableWebhooks", match.EnableWebhooks);
                    command.Parameters.AddWithValue("@mpTimerCommand", match.MpTimerCommand);
                    command.Parameters.AddWithValue("@mpTimerAfterGame", match.MpTimerAfterGame);
                    command.Parameters.AddWithValue("@mpTimerAfterPick", match.MpTimerAfterPick);
                    command.Parameters.AddWithValue("@pointsForSecondBan", match.PointsForSecondBan);
                    command.Parameters.AddWithValue("@allPicksFreemod", match.AllPicksFreemod);
                    command.Parameters.AddWithValue("@status", match.Status.ToString());
                    command.Parameters.AddWithValue("@warmupMode", match.WarmupMode);
                    command.Parameters.AddWithValue("@id", match.Id);
                    command.ExecuteNonQuery();
                }
                if (match.TeamMode == Enums.TeamModes.HeadToHead)
                {
                    using (var command = new SQLiteCommand("DELETE FROM MatchPlayers WHERE match = @match", conn))
                    {
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.ExecuteNonQuery();
                    }
                    foreach (var player in match.Players)
                    {
                        using (var command = new SQLiteCommand("INSERT INTO MatchPlayers (match, player, points) VALUES (@match, @player, @points)", conn))
                        {
                            command.Parameters.AddWithValue("@match", match.Id);
                            command.Parameters.AddWithValue("@player", player.Key.Id);
                            command.Parameters.AddWithValue("@points", player.Value);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                using (var command = new SQLiteCommand("DELETE FROM MatchPicks WHERE match = @match", conn))
                {
                    command.Parameters.AddWithValue("@match", match.Id);
                    command.ExecuteNonQuery();
                }
                foreach (var pick in match.Picks)
                {
                    using (var command = new SQLiteCommand("INSERT INTO MatchPicks (match, beatmap, picker, ban) VALUES (@match, @beatmap, @picker, False)", conn))
                    {
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.Parameters.AddWithValue("@beatmap", pick.Map.Id);
                        if (match.TeamMode == TeamModes.TeamVS)
                            command.Parameters.AddWithValue("@picker", pick.Team.Id);
                        else if (match.TeamMode == TeamModes.HeadToHead)
                            command.Parameters.AddWithValue("@picker", pick.Player.Id);
                        command.ExecuteNonQuery();
                    }
                }
                foreach (var pick in match.Bans)
                {
                    using (var command = new SQLiteCommand("INSERT INTO MatchPicks (match, beatmap, picker, ban) VALUES (@match, @beatmap, @picker, True)", conn))
                    {
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.Parameters.AddWithValue("@beatmap", pick.Map.Id);
                        if (match.TeamMode == TeamModes.TeamVS)
                            command.Parameters.AddWithValue("@picker", pick.Team.Id);
                        else if (match.TeamMode == TeamModes.HeadToHead)
                            command.Parameters.AddWithValue("@picker", pick.Player.Id);
                        command.ExecuteNonQuery();
                    }
                }
                using (var command = new SQLiteCommand("DELETE FROM Games WHERE match = @match", conn))
                {
                    command.Parameters.AddWithValue("@match", match.Id);
                    command.ExecuteNonQuery();
                }
                foreach (var game in match.Games)
                {
                    using (var command = new SQLiteCommand("INSERT INTO Games (id, match, beatmap, mods, counted) VALUES (@id, @match, @beatmap, @mods, @counted)", conn))
                    {
                        command.Parameters.AddWithValue("@id", game.Id);
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.Parameters.AddWithValue("@beatmap", game.Beatmap.Id);
                        command.Parameters.AddWithValue("@mods", string.Join(",", game.Mods));
                        command.Parameters.AddWithValue("@counted", game.Counted);
                        command.ExecuteNonQuery();
                    }
                    using (var command = new SQLiteCommand("DELETE FROM Scores WHERE game = @game", conn))
                    {
                        command.Parameters.AddWithValue("@game", game.Id);
                        command.ExecuteNonQuery();
                    }
                    foreach (var score in game.Scores)
                    {
                        using (var command = new SQLiteCommand("INSERT INTO Scores (player, game, mods, score, team, passed) VALUES (@player, @game, @mods, @score, @team, @passed)", conn))
                        {
                            command.Parameters.AddWithValue("@player", score.Player.Id);
                            command.Parameters.AddWithValue("@game", game.Id);
                            command.Parameters.AddWithValue("@mods", string.Join(",", score.Mods));
                            command.Parameters.AddWithValue("@score", score.Points);
                            command.Parameters.AddWithValue("@team", score.Team.ToString());
                            command.Parameters.AddWithValue("@passed", score.Passed);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                transaction.Commit();
                conn.Close();
            }
        }

        public static void DeleteMatch(Match match)
        {
            Log.Information("Database: delete match '{name}'", match.Name);
            using (var conn = GetConnection())
            using (var transaction = conn.BeginTransaction())
            {
                foreach (var game in match.Games)
                {
                    using (var command = new SQLiteCommand("DELETE FROM Scores WHERE game = @game", conn))
                    {
                        command.Parameters.AddWithValue("@game", game.Id);
                        command.ExecuteNonQuery();
                    }
                }
                using (var command = new SQLiteCommand("DELETE FROM Games WHERE match = @match", conn))
                {
                    command.Parameters.AddWithValue("@match", match.Id);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("DELETE FROM MatchPlayers WHERE match = @match", conn))
                {
                    command.Parameters.AddWithValue("@match", match.Id);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("DELETE FROM MatchPicks WHERE match = @match", conn))
                {
                    command.Parameters.AddWithValue("@match", match.Id);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("DELETE FROM IrcMessages WHERE match = @match", conn))
                {
                    command.Parameters.AddWithValue("@match", match.Id);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("DELETE FROM Matches WHERE id = @id", conn))
                {
                    command.Parameters.AddWithValue("@id", match.Id);
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
                conn.Close();
            }
            Matches.Remove(match);
        }

        public static void InitMatchPlayers()
        {
            Log.Information("Database: init match teams and players");
            using (var conn = GetConnection())
            {
                foreach (var match in Matches)
                {
                    if (match.TeamMode == TeamModes.HeadToHead)
                    {
                        using (var command = new SQLiteCommand("SELECT player, points FROM MatchPlayers WHERE match = @match", conn))
                        {
                            command.Parameters.AddWithValue("@match", match.Id);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    match.Players.Add(GetPlayer(reader["player"].ToString()), Convert.ToInt32(reader["points"]));
                                }
                                reader.Close();
                            }
                        }
                    }
                }
                conn.Close();
            }
        }

        public static void InitMatchPicks()
        {
            Log.Information("Database: init match picks");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT match, beatmap, picker, ban FROM MatchPicks", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var match = Matches.First(x => x.Id == Convert.ToInt32(reader["match"]));
                    var beatmap = match.Mappool.Beatmaps.First(x => x.Id == Convert.ToInt32(reader["beatmap"]));
                    Team team = null;
                    Player player = null;
                    if (match.TeamMode == TeamModes.TeamVS)
                        team = Teams.First(x => x.Id == Convert.ToInt32(reader["picker"]));
                    else if (match.TeamMode == TeamModes.HeadToHead)
                        player = GetPlayer(reader["picker"].ToString());
                    var ban = Convert.ToBoolean(reader["ban"]);
                    var pick = new MatchPick()
                    {
                        Match = match,
                        Map = beatmap,
                        Team = team,
                        Player = player
                    };
                    if (ban)
                        match.Bans.Add(pick);
                    else
                        match.Picks.Add(pick);
                }
                conn.Close();
            }
        }

        public static void InitMatchGames()
        {
            Log.Information("Database: init match games");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, match, beatmap, mods, counted FROM Games", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    var match = Matches.First(x => x.Id == Convert.ToInt32(reader["match"]));
                    var beatmap = GetBeatmap(Convert.ToInt32(reader["beatmap"]));

                    var counted = Convert.ToBoolean(reader["counted"]);
                    var game = new Game()
                    {
                        Id = id,
                        Match = match,
                        Beatmap = beatmap,
                        Counted = counted
                    };
                    foreach (string mod in reader["mods"].ToString().Split(','))
                    {
                        if (Enum.TryParse(mod, out GameMods gameMod))
                        {
                            game.Mods.Add(gameMod);
                        }
                    }
                    match.Games.Add(game);
                }
                conn.Close();
            }
        }

        public static void AddIrcMessages(List<IrcMessage> messages)
        {
            Log.Information("Database: add irc messages");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("INSERT INTO IrcMessages (match, timestamp, user, message) VALUES (@match, @timestamp, @user, @message)", conn))
            using (var transaction = conn.BeginTransaction())
            {
                foreach (var message in messages)
                {
                    if (message.Match != null)
                        command.Parameters.AddWithValue("@match", message.Match.Id);
                    else
                        command.Parameters.AddWithValue("@match", DBNull.Value);
                    command.Parameters.AddWithValue("@timestamp", message.Timestamp);
                    command.Parameters.AddWithValue("@user", message.User);
                    command.Parameters.AddWithValue("@message", message.Message);
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
                conn.Close();
            }
        }

        public static List<IrcMessage> GetIrcMessages(Match match)
        {
            Log.Information("Database: get irc messages for match '{match}'", match.Name);
            var messages = new List<IrcMessage>();
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT timestamp, user, message FROM IrcMessages WHERE match = @match", conn))
            {
                command.Parameters.AddWithValue("@match", match.Id);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var message = new IrcMessage()
                        {
                            Timestamp = DateTime.Parse(reader["timestamp"].ToString()),
                            User = reader["user"].ToString(),
                            Message = reader["message"].ToString(),
                            Match = match
                        };
                        messages.Add(message);
                    }
                    reader.Close();
                }
                conn.Close();
            }
            return messages;
        }

        public static List<IrcMessage> GetIrcMessages(string user)
        {
            Log.Information("Database: get irc messages for user '{user}'", user);
            var messages = new List<IrcMessage>();
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT timestamp, message FROM IrcMessages WHERE match IS NULL AND user = @user", conn))
            {
                command.Parameters.AddWithValue("@user", user);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var message = new IrcMessage()
                        {
                            Timestamp = DateTime.Parse(reader["timestamp"].ToString()),
                            User = user,
                            Message = reader["message"].ToString(),
                            Match = null
                        };
                        messages.Add(message);
                    }
                    reader.Close();
                }
                conn.Close();
            }
            return messages;
        }
        #endregion
    }
}
