using script_chan2.DataTypes;
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
        public static List<Tournament> Tournaments;
        public static List<Webhook> Webhooks;
        public static List<Mappool> Mappools;
        public static List<Player> Players;
        public static List<Team> Teams;
        public static List<Match> Matches;

        public static void Initialize()
        {
            Log.Information("DB init started");
            InitTournaments();
            InitTeams();
            InitPlayers();
            InitTeamPlayers();
            InitWebhooks();
            InitMappools();
            InitMappoolMaps();
            InitTournamentWebhooks();
            InitMatches();
            InitMatchPlayers();
            Log.Information("DB init finished");
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
            Log.Information("DB getting settings");
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
            Log.Information("DB updating settings");
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
            Log.Information("DB init tournaments");
            Tournaments = new List<Tournament>();
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, name, gameMode, teamMode, winCondition, acronym, teamSize, roomSize, pointsForSecondBan, allPicksFreemod, mpTimerCommand, mpTimerAfterGame, mpTimerAfterPick FROM Tournaments", conn))
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
                    var tournament = new Tournament(name, gameMode, teamMode, winCondition, acronym, teamSize, roomSize, pointsForSecondBan, allPicksFreemod, mpTimerCommand, mpTimerAfterGame, mpTimerAfterPick, id);
                    Tournaments.Add(tournament);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddTournament(Tournament tournament)
        {
            Log.Information("DB add new tournament '{name}'", tournament.Name);
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"INSERT INTO Tournaments (name, gameMode, teamMode, winCondition, acronym, teamSize, roomSize, pointsForSecondBan, allPicksFreemod, mpTimerCommand, mpTimerAfterGame, mpTimerAfterPick)
                VALUES (@name, @gameMode, @teamMode, @winCondition, @acronym, @teamSize, @roomSize, @pointsForSecondBan, @allPicksFreemod, @mpTimerCommand, @mpTimerAfterGame, @mpTimerAfterPick)", conn))
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
            Log.Information("DB delete tournament '{name}'", tournament.Name);
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
            Log.Information("DB update tournament '{name}'", tournament.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"UPDATE Tournaments
                SET name = @name, gameMode = @gameMode, teamMode = @teamMode, winCondition = @winCondition, acronym = @acronym, teamSize = @teamSize, roomSize = @roomSize, pointsForSecondBan = @pointsForSecondBan, allPicksFreemod = @allPicksFreemod, mpTimerCommand = @mpTimerCommand, mpTimerAfterGame = @mpTimerAfterGame, mpTimerAfterPick = @mpTimerAfterPick
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
                command.Parameters.AddWithValue("@id", tournament.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        #endregion

        #region Webhooks
        public static void InitWebhooks()
        {
            Log.Information("DB init webhooks");
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
            Log.Information("DB add new webhook '{name}'", webhook.Name);
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
            Log.Information("DB delete webhook '{name}'", webhook.Name);
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
            Log.Information("DB update webhook '{name}'", webhook.Name);
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
            Log.Information("DB init tournament webhooks");
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
            Log.Information("DB add webhook '{webhook}' to tournament '{tournament}'", webhook.Name, tournament.Name);
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("SELECT COUNT(tournament) FROM WebhookLinks WHERE tournament = @tournament AND webhook = @webhook", conn))
                {
                    command.Parameters.AddWithValue("@webhook", webhook.Id);
                    command.Parameters.AddWithValue("@tournament", tournament.Id);
                    var rowCount = Convert.ToInt32(command.ExecuteScalar());
                    if (rowCount == 0)
                    {
                        using (var command2 = new SQLiteCommand("INSERT INTO WebhookLinks (tournament, webhook) VALUES (@tournament, @webhook)", conn))
                        {
                            command2.Parameters.AddWithValue("@webhook", webhook.Id);
                            command2.Parameters.AddWithValue("@tournament", tournament.Id);
                            command2.ExecuteNonQuery();
                        }
                    }
                }
                conn.Close();
            }
        }

        public static void RemoveWebhookFromTournament(Webhook webhook, Tournament tournament)
        {
            Log.Information("DB remove webhook '{webhook}' from tournament '{tournament}'", webhook.Name, tournament.Name);
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
            Log.Information("DB init mappools");
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
            Log.Information("DB add new mappool '{name}'", mappool.Name);
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

        public static void DeleteMappool(Mappool mappool)
        {
            Log.Information("DB delete mappool '{name}'", mappool.Name);
            using (var conn = GetConnection())
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
                conn.Close();
            }
            Mappools.Remove(mappool);
        }

        public static void UpdateMappool(Mappool mappool)
        {
            Log.Information("DB update mappool '{name}'", mappool.Name);
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
            Log.Information("DB add new beatmap '{id}'", beatmap.Id);
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
            Log.Information("DB get beatmap '{id}'", id);
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
            Log.Information("DB init mappool maps");
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
            Log.Information("DB add map '{map}' to mappool '{mappool}'", map.Beatmap.Id, map.Mappool.Name);
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

        public static void DeleteMappoolMap(MappoolMap map)
        {
            Log.Information("DB remove mappool map '{map}' from mappool '{mappool}'", map.Beatmap.Id, map.Mappool.Name);
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
            Log.Information("DB update map '{map}' in mappool '{mappool}'", map.Beatmap.Id, map.Mappool.Name);
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
            Log.Information("DB init players");
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
            Log.Information("DB add new player '{name}'", player.Name);
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
            Log.Information("DB get player '{name}'", idOrName);
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
                            if (returnValue != null)
                                AddPlayer(returnValue);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            if (returnValue != null)
                Players.Add(returnValue);
            return returnValue;
        }

        public static void AddPlayerToTeam(Player player, Team team)
        {
            Log.Information("DB add player '{player}' to team '{team}'", player.Name, team.Name);
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
            Log.Information("DB remove player '{player}' from team '{team}'", player.Name, team.Name);
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
            Log.Information("DB init teams");
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
            Log.Information("DB add team '{name}'", team.Name);
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
            Log.Information("DB update team '{name}'", team.Name);
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
            Log.Information("DB delete team '{name}'", team.Name);
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
            Log.Information("DB init team players");
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
            Log.Information("DB init matches");
            Matches = new List<Match>();
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"SELECT id, tournament, mappool, name, gameMode, teamMode, winCondition, teamBlue, teamBluePoints, teamRed, teamRedPoints, teamSize, roomSize,
                rollWinner, firstPicker, BO, enableWebhooks, mpTimerCommand, mpTimerAfterGame, mpTimerAfterPick, pointsForSecondBan, allPicksFreemod, status FROM Matches", conn))
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
                    Player firstPickerPlayer = null;
                    Team firstPickerTeam = null;
                    Team teamRed = null;
                    Team teamBlue = null;
                    if (teamMode == Enums.TeamModes.TeamVS)
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
                            rollWinnerPlayer = Players.First(x => x.Id == Convert.ToInt32(reader["rollWinner"]));
                        if (reader["firstPicker"] != DBNull.Value)
                            firstPickerPlayer = Players.First(x => x.Id == Convert.ToInt32(reader["firstPicker"]));
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
                    var status = Enums.MatchStatus.New;
                    switch (reader["status"].ToString())
                    {
                        case "New": status = Enums.MatchStatus.New; break;
                        case "InProgress": status = Enums.MatchStatus.InProgress; break;
                        case "Finished": status = Enums.MatchStatus.Finished; break;
                    }
                    var match = new Match(tournament, mappool, name, gameMode, teamMode, winCondition, teamBlue, teamBluePoints, teamRed, teamRedPoints, teamSize, roomSize, rollWinnerTeam, rollWinnerPlayer, firstPickerTeam, firstPickerPlayer, bo, enableWebhooks, mpTimerCommand, mpTimerAfterGame, mpTimerAfterPick, pointsForSecondBan, allPicksFreemod, status, id);
                    Matches.Add(match);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddMatch(Match match)
        {
            Log.Information("DB add new match '{name}'", match.Name);
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("INSERT INTO Matches (tournament, mappool, name, gameMode, teamMode, winCondition, teamBlue, teamBluePoints, teamRed, teamRedPoints, teamSize, roomSize, rollWinner, firstPicker, BO, enableWebhooks, mpTimerCommand, mpTimerAfterGame, mpTimerAfterPick, pointsForSecondBan, allPicksFreemod, status)" +
                    "VALUES (@tournament, @mappool, @name, @gameMode, @teamMode, @winCondition, @teamBlue, @teamBluePoints, @teamRed, @teamRedPoints, @teamSize, @roomSize, @rollWinner, @firstPicker, @BO, @enableWebhooks, @mpTimerCommand, @mpTimerAfterGame, @mpTimerAfterPick, @pointsForSecondBan, @allPicksFreemod, @status)", conn))
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
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("SELECT last_insert_rowid()", conn))
                {
                    resultValue = Convert.ToInt32(command.ExecuteScalar());
                }
                if (match.TeamMode == Enums.TeamModes.HeadToHead)
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
                conn.Close();
            }
            Matches.Add(match);
            return resultValue;
        }

        public static void UpdateMatch(Match match)
        {
            Log.Information("DB update match '{name}'", match.Name);
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"UPDATE Matches
                SET tournament = @tournament, mappool = @mappool, name = @name, gameMode = @gameMode, teamMode = @teamMode, winCondition = @winCondition, teamBlue = @teamBlue, teamBluePoints = @teamBluePoints,
                teamRed = @teamRed, teamRedPoints = @teamRedPoints, teamSize = @teamSize, roomSize = @roomSize, rollWinner = @rollWinner, firstPicker = @firstPicker, BO = @BO,
                enableWebhooks = @enableWebhooks, mpTimerCommand = @mpTimerCommand, mpTimerAfterGame = @mpTimerAfterGame, mpTimerAfterPick = @mpTimerAfterPick, pointsForSecondBan = @pointsForSecondBan,
                allPicksFreemod = @allPicksFreemod, status = @status
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
                conn.Close();
            }
        }

        public static void DeleteMatch(Match match)
        {
            Log.Information("DB delete match '{name}'", match.Name);
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("DELETE FROM MatchPlayers WHERE match = @match", conn))
                {
                    command.Parameters.AddWithValue("@match", match.Id);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("DELETE FROM Matches WHERE id = @id", conn))
                {
                    command.Parameters.AddWithValue("@id", match.Id);
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
            Matches.Remove(match);
        }

        public static void InitMatchPlayers()
        {
            Log.Information("DB init match teams and players");
            using (var conn = GetConnection())
            {
                foreach (var match in Matches)
                {
                    if (match.TeamMode == Enums.TeamModes.HeadToHead)
                    {
                        using (var command = new SQLiteCommand("SELECT player, points FROM MatchPlayers WHERE match = @match", conn))
                        {
                            command.Parameters.AddWithValue("@match", match.Id);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    match.Players.Add(Players.First(x => x.Id == Convert.ToInt32(reader["player"])), Convert.ToInt32(reader["points"]));
                                }
                                reader.Close();
                            }
                        }
                    }
                }
                conn.Close();
            }
        }
        #endregion
    }
}
