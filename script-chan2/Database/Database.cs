using script_chan2.DataTypes;
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
        public static List<Tournament> Tournaments;
        public static List<Webhook> Webhooks;
        public static List<Mappool> Mappools;
        public static List<Player> Players;
        public static List<Team> Teams;
        public static List<Match> Matches;

        public static void Initialize()
        {
            InitTournaments();
            InitTeams();
            InitPlayers();
            InitTeamPlayers();
            InitWebhooks();
            InitMappools();
            InitMappoolMaps();
            InitTournamentWebhooks();
            InitMatches();
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
            Tournaments = new List<Tournament>();
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, name, gameMode, teamMode, winCondition, acronym, teamSize, roomSize, pointsForSecondBan, allPicksFreemod FROM Tournaments", conn))
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
                    var tournament = new Tournament(name, gameMode, teamMode, winCondition, acronym, teamSize, roomSize, pointsForSecondBan, allPicksFreemod, id);
                    Tournaments.Add(tournament);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddTournament(Tournament tournament)
        {
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"INSERT INTO Tournaments (name, gameMode, teamMode, winCondition, acronym, teamSize, roomSize, pointsForSecondBan, allPicksFreemod)
                VALUES (@name, @gameMode, @teamMode, @winCondition, @acronym, @teamSize, @roomSize, @pointsForSecondBan, @allPicksFreemod)", conn))
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

        public static void DeleteTournament(int tournament)
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("DELETE FROM Tournaments WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@id", tournament);
                command.ExecuteNonQuery();
                conn.Close();
            }
            Tournaments.Remove(Tournaments.First(x => x.Id == tournament));
        }

        public static void UpdateTournament(Tournament tournament)
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"UPDATE Tournaments
                SET name = @name, gameMode = @gameMode, teamMode = @teamMode, winCondition = @winCondition, acronym = @acronym, teamSize = @teamSize, roomSize = @roomSize, pointsForSecondBan = @pointsForSecondBan, allPicksFreemod = @allPicksFreemod
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
                command.Parameters.AddWithValue("@id", tournament.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        #endregion

        #region Webhooks
        public static void InitWebhooks()
        {
            Webhooks = new List<Webhook>();
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, name, url FROM Webhooks", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    var name = reader["name"].ToString();
                    var url = reader["URL"].ToString();
                    var webhook = new Webhook(name, url, id);
                    Webhooks.Add(webhook);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddWebhook(Webhook webhook)
        {
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

        public static void DeleteWebhook(int webhook)
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("DELETE FROM Webhooks WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@id", webhook);
                command.ExecuteNonQuery();
                conn.Close();
            }
            Webhooks.Remove(Webhooks.First(x => x.Id == webhook));
        }

        public static void UpdateWebhook(Webhook webhook)
        {
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

        public static void AddWebhookToTournament(int webhook, int tournament)
        {
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("SELECT COUNT(tournament) FROM WebhookLinks WHERE tournament = @tournament AND webhook = @webhook", conn))
                {
                    command.Parameters.AddWithValue("@webhook", webhook);
                    command.Parameters.AddWithValue("@tournament", tournament);
                    var rowCount = Convert.ToInt32(command.ExecuteScalar());
                    if (rowCount == 0)
                    {
                        using (var command2 = new SQLiteCommand("INSERT INTO WebhookLinks (tournament, webhook) VALUES (@tournament, @webhook)", conn))
                        {
                            command2.Parameters.AddWithValue("@webhook", webhook);
                            command2.Parameters.AddWithValue("@tournament", tournament);
                            command2.ExecuteNonQuery();
                        }
                    }
                }
                conn.Close();
            }
        }

        public static void RemoveWebhookFromTournament(int webhook, int tournament)
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("DELETE FROM WebhookLinks WHERE webhook = @webhook AND tournament = @tournament", conn))
            {
                command.Parameters.AddWithValue("@webhook", webhook);
                command.Parameters.AddWithValue("@tournament", tournament);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        #endregion

        #region Mappools
        public static void InitMappools()
        {
            Mappools = new List<Mappool>();
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, name, tournament FROM Mappools", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    Tournament tournament = null;
                    if (reader["tournament"] != DBNull.Value)
                        tournament = Tournaments.First(x => x.Id == Convert.ToInt32(reader["tournament"]));
                    var name = reader["name"].ToString();
                    var mappool = new Mappool(name, tournament, id);
                    Mappools.Add(mappool);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddMappool(Mappool mappool)
        {
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("INSERT INTO Mappools (name, tournament) VALUES (@name, @tournament)", conn))
                {
                    if (mappool.Tournament == null)
                        command.Parameters.AddWithValue("@tournament", DBNull.Value);
                    else
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

        public static void AddMappoolToTournament(int mappool, int tournament)
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("UPDATE Mappools SET tournament = @tournament WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@tournament", tournament);
                command.Parameters.AddWithValue("@id", mappool);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void RemoveMappoolFromTournament(int mappool)
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("UPDATE Mappools SET tournament = NULL WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@id", mappool);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void DeleteMappool(int mappool)
        {
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("DELETE FROM MappoolMaps WHERE mappool = @id", conn))
                {
                    command.Parameters.AddWithValue("@id", mappool);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("DELETE FROM Mappools WHERE id = @id", conn))
                {
                    command.Parameters.AddWithValue("@id", mappool);
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
            Mappools.Remove(Mappools.First(x => x.Id == mappool));
        }

        public static void UpdateMappool(Mappool mappool)
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"UPDATE Mappools
                SET name = @name, tournament = @tournament
                WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@name", mappool.Name);
                if (mappool.Tournament == null)
                    command.Parameters.AddWithValue("@tournament", DBNull.Value);
                else
                    command.Parameters.AddWithValue("@tournament", mappool.Tournament.Id);
                command.Parameters.AddWithValue("@id", mappool.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void AddBeatmap(Beatmap beatmap)
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"INSERT INTO Beatmaps (id, beatmapsetId, artist, title, version, creator)
                VALUES (@id, @beatmapsetId, @artist, @title, @version, @creator)", conn))
            {
                command.Parameters.AddWithValue("@id", beatmap.Id);
                command.Parameters.AddWithValue("@beatmapsetId", beatmap.SetId);
                command.Parameters.AddWithValue("@artist", beatmap.Artist);
                command.Parameters.AddWithValue("@title", beatmap.Title);
                command.Parameters.AddWithValue("@version", beatmap.Version);
                command.Parameters.AddWithValue("@creator", beatmap.Creator);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static Beatmap GetBeatmap(int id)
        {
            Beatmap returnValue = null;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("SELECT id, beatmapsetId, artist, title, version, creator FROM Beatmaps WHERE id = @id", conn))
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
                            returnValue = new Beatmap(id, setId, artist, title, version, creator);
                        }
                        else
                        {
                            returnValue = OsuApi.OsuApi.GetBeatmap(id);
                            if (returnValue != null)
                                AddBeatmap(returnValue);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return returnValue;
        }

        public static void InitMappoolMaps()
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, mappool, beatmap, listIndex, mods FROM MappoolMaps ORDER BY listIndex ASC", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    var mappool = Mappools.First(x => x.Id == Convert.ToInt32(reader["mappool"]));
                    var beatmap = GetBeatmap(Convert.ToInt32(reader["beatmap"]));
                    var mappoolMap = new MappoolMap(mappool, beatmap, reader["mods"].ToString(), id);
                    mappool.Beatmaps.Add(mappoolMap);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddMappoolMap(MappoolMap map)
        {
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("INSERT INTO MappoolMaps (mappool, beatmap, listIndex, mods) VALUES (@mappool, @beatmap, @listIndex, @mods)", conn))
                {
                    command.Parameters.AddWithValue("@mappool", map.Mappool.Id);
                    command.Parameters.AddWithValue("@beatmap", map.Beatmap.Id);
                    command.Parameters.AddWithValue("@listIndex", map.ListIndex);
                    command.Parameters.AddWithValue("@mods", string.Join(",", map.Mods));
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

        public static void DeleteMappoolMap(int map)
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("DELETE FROM MappoolMaps WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@id", map);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void UpdateMappoolMap(MappoolMap map)
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"UPDATE MappoolMaps
                SET beatmap = @beatmap, listIndex = @listIndex, mods = @mods
                WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@beatmap", map.Beatmap.Id);
                command.Parameters.AddWithValue("@listIndex", map.ListIndex);
                command.Parameters.AddWithValue("@mods", string.Join(",", map.Mods));
                command.Parameters.AddWithValue("@id", map.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        #endregion

        #region Players
        public static void InitPlayers()
        {
            Players = new List<Player>();
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, name, country FROM Players", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    var name = reader["name"].ToString();
                    var country = reader["country"].ToString();
                    var player = new Player(name, country, id);
                    Players.Add(player);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static void AddPlayer(Player player)
        {
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
            Player returnValue = Players.FirstOrDefault(x => x.Id.ToString() == idOrName || x.Name == idOrName);
            if (returnValue != null)
                return returnValue;
            using (var conn = GetConnection())
            {
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
                            returnValue = new Player(name, country, id);
                        }
                        else
                        {
                            returnValue = OsuApi.OsuApi.GetPlayer(idOrName);
                            AddPlayer(returnValue);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            Players.Add(returnValue);
            return returnValue;
        }

        public static void AddPlayerToTeam(int player, int team)
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("INSERT INTO TeamPlayers (player, team) VALUES (@player, @team)", conn))
            {
                command.Parameters.AddWithValue("@player", player);
                command.Parameters.AddWithValue("@team", team);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void RemovePlayerFromTeam(int player, int team)
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("DELETE FROM TeamPlayers WHERE player = @player AND team = @team", conn))
            {
                command.Parameters.AddWithValue("@player", player);
                command.Parameters.AddWithValue("@team", team);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        #endregion

        #region Teams
        public static void InitTeams()
        {
            Teams = new List<Team>();
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, name, tournament FROM Teams", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    Tournament tournament = null;
                    if (reader["tournament"] != DBNull.Value)
                        tournament = Tournaments.First(x => x.Id == Convert.ToInt32(reader["tournament"]));
                    var name = reader["name"].ToString();
                    var team = new Team(tournament, name, id);
                    Teams.Add(team);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddTeam(Team team)
        {
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("INSERT INTO Teams (tournament, name) VALUES (@tournament, @name)", conn))
                {
                    if (team.Tournament == null)
                        command.Parameters.AddWithValue("@tournament", DBNull.Value);
                    else
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
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("UPDATE Teams SET name = @name WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@name", team.Name);
                command.Parameters.AddWithValue("@id", team.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void DeleteTeam(int id)
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("DELETE FROM Teams WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
                conn.Close();
            }
            Teams.RemoveAll(x => x.Id == id);
        }

        public static void InitTeamPlayers()
        {
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT player, team FROM TeamPlayers", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var playerId = Convert.ToInt32(reader["player"]);
                    var teamId = Convert.ToInt32(reader["team"]);
                    Teams.First(x => x.Id == teamId).Players.Add(Players.First(x => x.Id == playerId));
                }
                reader.Close();
                conn.Close();
            }
        }
        #endregion

        #region Matches
        public static void InitMatches()
        {
            Matches = new List<Match>();
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, tournament, mappool, name, gameMode, teamMode, winCondition, rollWinner, firstPicker, BO, enableWebhooks, mpTimer, pointsForSecondBan, allPicksFreemod, status FROM Matches", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    Tournament tournament = null;
                    if (reader["tournament"] != DBNull.Value)
                        tournament = Tournaments.First(x => x.Id == Convert.ToInt32(reader["tournament"]));
                    Mappool mappool = null;
                    if (reader["mappool"] != DBNull.Value)
                        mappool = Mappools.First(x => x.Id == Convert.ToInt32(reader["mappool"]));
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
                    Player rollWinnerPlayer = null;
                    Team rollWinnerTeam = null;
                    if (reader["rollWinner"] != DBNull.Value)
                    {
                        if (teamMode == Enums.TeamModes.TeamVS)
                            rollWinnerTeam = Teams.First(x => x.Id == Convert.ToInt32(reader["rollWinner"]));
                        else
                            rollWinnerPlayer = Players.First(x => x.Id == Convert.ToInt32(reader["rollWinner"]));
                    }
                    Player firstPickerPlayer = null;
                    Team firstPickerTeam = null;
                    if (reader["firstPicker"] != DBNull.Value)
                    {
                        if (teamMode == Enums.TeamModes.TeamVS)
                            firstPickerTeam = Teams.First(x => x.Id == Convert.ToInt32(reader["firstPicker"]));
                        else
                            firstPickerPlayer = Players.First(x => x.Id == Convert.ToInt32(reader["firstPicker"]));
                    }
                    var bo = Convert.ToInt32(reader["BO"]);
                    var enableWebhooks = Convert.ToBoolean(reader["enableWebhooks"]);
                    var mpTimer = Convert.ToInt32(reader["mpTimer"]);
                    var pointsForSecondBan = Convert.ToInt32(reader["pointsForSecondBan"]);
                    var allPicksFreemod = Convert.ToBoolean(reader["allPicksFreemod"]);
                    var status = Enums.MatchStatus.New;
                    switch (reader["status"].ToString())
                    {
                        case "New": status = Enums.MatchStatus.New; break;
                        case "InProgress": status = Enums.MatchStatus.InProgress; break;
                        case "Finished": status = Enums.MatchStatus.Finished; break;
                    }
                    var match = new Match(tournament, mappool, name, gameMode, teamMode, winCondition, rollWinnerTeam, rollWinnerPlayer, firstPickerTeam, firstPickerPlayer, bo, enableWebhooks, mpTimer, pointsForSecondBan, allPicksFreemod, status, id);
                    Matches.Add(match);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddMatch(Match match)
        {
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("INSERT INTO Matches (tournament, mappool, name, gameMode, teamMode, winCondition, rollWinner, firstPicker, BO, enableWebhooks, mpTimer, pointsForSecondBan, allPicksFreemod, status)" +
                    "VALUES (@tournament, @mappool, @name, @gameMode, @teamMode, @winCondition, @rollWinner, @firstPicker, @BO, @enableWebhooks, @mpTimer, @pointsForSecondBan, @allPicksFreemod, @status)", conn))
                {
                    if (match.Tournament == null)
                        command.Parameters.AddWithValue("@tournament", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@tournament", match.Tournament.Id);
                    if (match.Mappool == null)
                        command.Parameters.AddWithValue("@mappool", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@mappool", match.Mappool.Id);
                    command.Parameters.AddWithValue("@name", match.Name);
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
                    command.Parameters.AddWithValue("@BO", match.BO);
                    command.Parameters.AddWithValue("@enableWebhooks", match.EnableWebhooks);
                    command.Parameters.AddWithValue("@mpTimer", match.MpTimer);
                    command.Parameters.AddWithValue("@pointsForSecondBan", match.PointsForSecondBan);
                    command.Parameters.AddWithValue("@allPicksFreemod", match.AllPicksFreemod);
                    command.Parameters.AddWithValue("@status", match.Status.ToString());
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("SELECT last_insert_rowid()", conn))
                {
                    resultValue = Convert.ToInt32(command.ExecuteScalar());
                }
                if (match.TeamMode == Enums.TeamModes.TeamVS)
                {
                    foreach (var team in match.Teams)
                    {
                        using (var command = new SQLiteCommand("INSERT INTO MatchTeams (match, team) VALUES (@match, @team)", conn))
                        {
                            command.Parameters.AddWithValue("@match", resultValue);
                            command.Parameters.AddWithValue("@team", team.Id);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    foreach (var player in match.Players)
                    {
                        using (var command = new SQLiteCommand("INSERT INTO MatchPlayers (match, player) VALUES (@match, @player)", conn))
                        {
                            command.Parameters.AddWithValue("@match", resultValue);
                            command.Parameters.AddWithValue("@player", player.Id);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                conn.Close();
            }
            Matches.Add(match);
            return resultValue;
        }

        public static void UpdateMatch(Match match)
        {
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"UPDATE Matches
                SET tournament = @tournament, mappool = @mappool, name = @name, gameMode = @gameMode, teamMode = @teamMode, winCondition = @winCondition, rollWinner = @rollWinner, firstPicker = @firstPicker, BO = @BO,
                enableWebhooks = @enableWebhooks, mpTimer = @mpTimer, pointsForSecondBan = @pointsForSecondBan, allPicksFreemod = @allPicksFreemod, status = @status
                WHERE id = @id", conn))
                {
                    if (match.Tournament == null)
                        command.Parameters.AddWithValue("@tournament", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@tournament", match.Tournament.Id);
                    if (match.Mappool == null)
                        command.Parameters.AddWithValue("@mappool", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@mappool", match.Mappool.Id);
                    command.Parameters.AddWithValue("@name", match.Name);
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
                    command.Parameters.AddWithValue("@BO", match.BO);
                    command.Parameters.AddWithValue("@enableWebhooks", match.EnableWebhooks);
                    command.Parameters.AddWithValue("@mpTimer", match.MpTimer);
                    command.Parameters.AddWithValue("@pointsForSecondBan", match.PointsForSecondBan);
                    command.Parameters.AddWithValue("@allPicksFreemod", match.AllPicksFreemod);
                    command.Parameters.AddWithValue("@status", match.Status.ToString());
                    command.Parameters.AddWithValue("@id", match.Id);
                    command.ExecuteNonQuery();
                }
                if (match.TeamMode == Enums.TeamModes.TeamVS)
                {
                    using (var command = new SQLiteCommand("DELETE FROM MatchTeams WHERE match = @match", conn))
                    {
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.ExecuteNonQuery();
                    }
                    foreach (var team in match.Teams)
                    {
                        using (var command = new SQLiteCommand("INSERT INTO MatchTeams (match, team) VALUES (@match, @team)", conn))
                        {
                            command.Parameters.AddWithValue("@match", match.Id);
                            command.Parameters.AddWithValue("@team", team.Id);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    using (var command = new SQLiteCommand("DELETE FROM MatchPlayers WHERE match = @match", conn))
                    {
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.ExecuteNonQuery();
                    }
                    foreach (var player in match.Players)
                    {
                        using (var command = new SQLiteCommand("INSERT INTO MatchPlayers (match, player) VALUES (@match, @player)", conn))
                        {
                            command.Parameters.AddWithValue("@match", match.Id);
                            command.Parameters.AddWithValue("@player", player.Id);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                conn.Close();
            }
        }

        public static void DeleteMatch(int id)
        {
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("DELETE FROM MatchTeams WHERE match = @match", conn))
                {
                    command.Parameters.AddWithValue("@match", id);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("DELETE FROM MatchPlayers WHERE match = @match", conn))
                {
                    command.Parameters.AddWithValue("@match", id);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("DELETE FROM Matches WHERE id = @id", conn))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
            Matches.RemoveAll(x => x.Id == id);
        }
        #endregion
    }
}
