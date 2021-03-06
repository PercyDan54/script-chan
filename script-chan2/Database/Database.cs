using Caliburn.Micro;
using script_chan2.DataTypes;
using script_chan2.Discord;
using script_chan2.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace script_chan2.Database
{
    public static class Database
    {
        private static ILogger localLog = Log.ForContext(typeof(Database));

        public static List<Tournament> Tournaments = new List<Tournament>();
        public static List<Webhook> Webhooks = new List<Webhook>();
        public static List<Mappool> Mappools = new List<Mappool>();
        public static List<Player> Players = new List<Player>();
        public static List<Team> Teams = new List<Team>();
        public static List<Match> Matches = new List<Match>();
        public static List<Beatmap> Beatmaps = new List<Beatmap>();
        public static List<CustomCommand> CustomCommands = new List<CustomCommand>();

        public static void Initialize()
        {
            localLog.Information("init started");
            InitTournaments();
            InitTournamentHeadToHeadPoints();
            InitTeams();
            InitTeamPlayers().Wait();
            InitWebhooks().Wait();
            InitMappools();
            InitMappoolMaps().Wait();
            InitTournamentWebhooks();
            InitMatches().Wait();
            InitMatchPlayers().Wait();
            InitMatchPicks().Wait();
            InitMatchGames().Wait();
            InitCustomCommands();
            localLog.Information("init finished");
        }

        private static SQLiteConnection GetConnection()
        {
            var conn = new SQLiteConnection("Data Source=Database.sqlite;Version=3");
            conn.Open();
            using (var command = new SQLiteCommand("PRAGMA foreign_keys = ON", conn))
            {
                command.ExecuteNonQuery();
            }
            return conn;
        }

        #region Settings
        public static Dictionary<string, string> GetSettings()
        {
            localLog.Information("getting settings");
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
            localLog.Information("updating settings");
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
            localLog.Information("init tournaments");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, name, gameMode, teamMode, winCondition, acronym, teamSize, roomSize, pointsForSecondBan, allPicksFreemod, mpTimerCommand, mpTimerAfterGame, mpTimerAfterPick, welcomeString, brInitialLivesAmount FROM Tournaments", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    var name = reader["name"].ToString();
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
                        case "BattleRoyale": teamMode = TeamModes.BattleRoyale; break;
                    }
                    var winCondition = WinConditions.ScoreV2;
                    switch (reader["winCondition"].ToString())
                    {
                        case "Score": winCondition = WinConditions.Score; break;
                        case "ScoreV2": winCondition = WinConditions.ScoreV2; break;
                        case "Accuracy": winCondition = WinConditions.Accuracy; break;
                        case "Combo": winCondition = WinConditions.Combo; break;
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
                    var brInitialLivesAmount = Convert.ToInt32(reader["brInitialLivesAmount"]);
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
                        WelcomeString = welcomeString,
                        BRInitialLivesAmount = brInitialLivesAmount
                    };
                    Tournaments.Add(tournament);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static void InitTournamentHeadToHeadPoints()
        {
            localLog.Information("init head to head points");
            using (var conn = GetConnection())
            {
                foreach (var tournament in Tournaments.Where(x => x.TeamMode == TeamModes.HeadToHead))
                {
                    using (var command = new SQLiteCommand("SELECT place, points FROM HeadToHeadPoints WHERE tournament = @tournament", conn))
                    {
                        command.Parameters.AddWithValue("@tournament", tournament.Id);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var place = Convert.ToInt32(reader["place"]);
                                var points = Convert.ToInt32(reader["points"]);
                                tournament.HeadToHeadPoints.Add(place, points);
                            }
                            reader.Close();
                        }
                    }
                }
                conn.Close();
            }
        }

        public static int AddTournament(Tournament tournament)
        {
            localLog.Information("add new tournament '{name}'", tournament.Name);
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"INSERT INTO Tournaments (name, gameMode, teamMode, winCondition, acronym, teamSize, roomSize, pointsForSecondBan, allPicksFreemod, mpTimerCommand, mpTimerAfterGame, mpTimerAfterPick, welcomeString, brInitialLivesAmount)
                VALUES (@name, @gameMode, @teamMode, @winCondition, @acronym, @teamSize, @roomSize, @pointsForSecondBan, @allPicksFreemod, @mpTimerCommand, @mpTimerAfterGame, @mpTimerAfterPick, @welcomeString, @brInitialLivesAmount)", conn))
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
                    command.Parameters.AddWithValue("@brInitialLivesAmount", tournament.BRInitialLivesAmount);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("SELECT last_insert_rowid()", conn))
                {
                    resultValue = Convert.ToInt32(command.ExecuteScalar());
                }
                using (var transaction = conn.BeginTransaction())
                {
                    foreach (var headToHeadPoint in tournament.HeadToHeadPoints)
                    {
                        using (var command = new SQLiteCommand("INSERT INTO HeadToHeadPoints (tournament, place, points) VALUES (@tournament, @place, @points)", conn))
                        {
                            command.Parameters.AddWithValue("@tournament", resultValue);
                            command.Parameters.AddWithValue("@place", headToHeadPoint.Key);
                            command.Parameters.AddWithValue("@points", headToHeadPoint.Value);
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                conn.Close();
            }
            Tournaments.Add(tournament);
            return resultValue;
        }

        public static void DeleteTournament(Tournament tournament)
        {
            localLog.Information("delete tournament '{name}'", tournament.Name);
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
            localLog.Information("update tournament '{name}'", tournament.Name);
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"UPDATE Tournaments
                SET name = @name, gameMode = @gameMode, teamMode = @teamMode, winCondition = @winCondition, acronym = @acronym, teamSize = @teamSize, roomSize = @roomSize, pointsForSecondBan = @pointsForSecondBan, allPicksFreemod = @allPicksFreemod, mpTimerCommand = @mpTimerCommand, mpTimerAfterGame = @mpTimerAfterGame, mpTimerAfterPick = @mpTimerAfterPick, welcomeString = @welcomeString, brInitialLivesAmount = @brInitialLivesAmount
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
                    command.Parameters.AddWithValue("@brInitialLivesAmount", tournament.BRInitialLivesAmount);
                    command.Parameters.AddWithValue("@id", tournament.Id);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("DELETE FROM HeadToHeadPoints WHERE tournament = @tournament", conn))
                {
                    command.Parameters.AddWithValue("@tournament", tournament.Id);
                    command.ExecuteNonQuery();
                }
                using (var transaction = conn.BeginTransaction())
                {
                    foreach (var headToHeadPoint in tournament.HeadToHeadPoints)
                    {
                        using (var command = new SQLiteCommand("INSERT INTO HeadToHeadPoints (tournament, place, points) VALUES (@tournament, @place, @points)", conn))
                        {
                            command.Parameters.AddWithValue("@tournament", tournament.Id);
                            command.Parameters.AddWithValue("@place", headToHeadPoint.Key);
                            command.Parameters.AddWithValue("@points", headToHeadPoint.Value);
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                conn.Close();
            }
        }
        #endregion

        #region Webhooks
        public static async Task InitWebhooks()
        {
            localLog.Information("init webhooks");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, name, url, matchCreated, banRecap, pickRecap, gameRecap, footerText, footerIcon, winImage, username, avatar, guild, channel, authorIcon FROM Webhooks", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    var name = reader["name"].ToString();
                    var url = reader["URL"].ToString();
                    var matchCreated = Convert.ToBoolean(reader["matchCreated"]);
                    var banRecap = Convert.ToBoolean(reader["banRecap"]);
                    var pickRecap = Convert.ToBoolean(reader["pickRecap"]);
                    var gameRecap = Convert.ToBoolean(reader["gameRecap"]);
                    var footerText = reader["footerText"].ToString();
                    var footerIcon = reader["footerIcon"].ToString();
                    var winImage = reader["winImage"].ToString();
                    var username = reader["username"].ToString();
                    var avatar = reader["avatar"].ToString();
                    var guild = reader["guild"].ToString();
                    var channel = reader["channel"].ToString();
                    var authorIcon = reader["authorIcon"].ToString();
                    var webhook = new Webhook(id)
                    {
                        Name = name,
                        URL = url,
                        MatchCreated = matchCreated,
                        BanRecap = banRecap,
                        PickRecap = pickRecap,
                        GameRecap = gameRecap,
                        FooterText = footerText,
                        FooterIcon = footerIcon,
                        WinImage = winImage,
                        Username = username,
                        Avatar = avatar,
                        Guild = guild,
                        Channel = channel,
                        AuthorIcon = authorIcon
                    };
                    Webhooks.Add(webhook);
                }
                reader.Close();
                conn.Close();
            }
            foreach (var webhook in Webhooks)
            {
                if (string.IsNullOrEmpty(webhook.Guild))
                {
                    await DiscordApi.SetWebhookChannel(webhook);
                    webhook.Save();
                }
            }
        }

        public static int AddWebhook(Webhook webhook)
        {
            localLog.Information("add new webhook '{name}'", webhook.Name);
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("INSERT INTO Webhooks (name, url, matchCreated, banRecap, pickRecap, gameRecap, footerText, footerIcon, winImage, username, avatar, guild, channel, authorIcon)" +
                    "VALUES (@name, @url, @matchCreated, @banRecap, @pickRecap, @gameRecap, @footerText, @footerIcon, @winImage, @username, @avatar, @guild, @channel, @authorIcon)", conn))
                {
                    command.Parameters.AddWithValue("@name", webhook.Name);
                    command.Parameters.AddWithValue("@url", webhook.URL);
                    command.Parameters.AddWithValue("@matchCreated", webhook.MatchCreated);
                    command.Parameters.AddWithValue("@banRecap", webhook.BanRecap);
                    command.Parameters.AddWithValue("@pickRecap", webhook.PickRecap);
                    command.Parameters.AddWithValue("@gameRecap", webhook.GameRecap);
                    command.Parameters.AddWithValue("@footerText", webhook.FooterText);
                    command.Parameters.AddWithValue("@footerIcon", webhook.FooterIcon);
                    command.Parameters.AddWithValue("@winImage", webhook.WinImage);
                    command.Parameters.AddWithValue("@username", webhook.Username);
                    command.Parameters.AddWithValue("@avatar", webhook.Avatar);
                    command.Parameters.AddWithValue("@guild", webhook.Guild);
                    command.Parameters.AddWithValue("@channel", webhook.Channel);
                    command.Parameters.AddWithValue("@authorIcon", webhook.AuthorIcon);
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
            localLog.Information("delete webhook '{name}'", webhook.Name);
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
            localLog.Information("update webhook '{name}'", webhook.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"UPDATE Webhooks
                SET name = @name, url = @url, matchCreated = @matchCreated, banRecap = @banRecap, pickRecap = @pickRecap, gameRecap = @gameRecap, footerText = @footerText, footerIcon = @footerIcon, winImage = @winImage, username = @username, avatar = @avatar, guild = @guild, channel = @channel, authorIcon = @authorIcon
                WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@name", webhook.Name);
                command.Parameters.AddWithValue("@url", webhook.URL);
                command.Parameters.AddWithValue("@matchCreated", webhook.MatchCreated);
                command.Parameters.AddWithValue("@banRecap", webhook.BanRecap);
                command.Parameters.AddWithValue("@pickRecap", webhook.PickRecap);
                command.Parameters.AddWithValue("@gameRecap", webhook.GameRecap);
                command.Parameters.AddWithValue("@footerText", webhook.FooterText);
                command.Parameters.AddWithValue("@footerIcon", webhook.FooterIcon);
                command.Parameters.AddWithValue("@winImage", webhook.WinImage);
                command.Parameters.AddWithValue("@username", webhook.Username);
                command.Parameters.AddWithValue("@avatar", webhook.Avatar);
                command.Parameters.AddWithValue("@guild", webhook.Guild);
                command.Parameters.AddWithValue("@channel", webhook.Channel);
                command.Parameters.AddWithValue("@id", webhook.Id);
                command.Parameters.AddWithValue("@authorIcon", webhook.AuthorIcon);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        private static void InitTournamentWebhooks()
        {
            localLog.Information("init tournament webhooks");
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
            localLog.Information("add webhook '{webhook}' to tournament '{tournament}'", webhook.Name, tournament.Name);
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
            localLog.Information("remove webhook '{webhook}' from tournament '{tournament}'", webhook.Name, tournament.Name);
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
            localLog.Information("init mappools");
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
            localLog.Information("add new mappool '{name}'", mappool.Name);
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
            localLog.Information("delete mappool '{name}'", mappool.Name);
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
            localLog.Information("update mappool '{name}'", mappool.Name);
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
            localLog.Information("add new beatmap '{id}'", beatmap.Id);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"INSERT INTO Beatmaps (id, beatmapsetId, artist, title, version, creator, bpm, ar, cs)
                SELECT @id, @beatmapsetId, @artist, @title, @version, @creator, @bpm, @ar, @cs
                WHERE NOT EXISTS (SELECT 1 FROM Beatmaps WHERE id = @id2)", conn))
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
                command.Parameters.AddWithValue("@id2", beatmap.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static async Task<Beatmap> GetBeatmap(int id)
        {
            localLog.Information("get beatmap '{id}'", id);
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
                        returnValue = await OsuApi.OsuApi.GetBeatmap(id);
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

        public static async Task InitMappoolMaps()
        {
            localLog.Information("init mappool maps");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, mappool, beatmap, listIndex, mods, tag, pickCommand FROM MappoolMaps ORDER BY listIndex ASC", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    var mappool = Mappools.First(x => x.Id == Convert.ToInt32(reader["mappool"]));
                    var beatmap = await GetBeatmap(Convert.ToInt32(reader["beatmap"]));
                    var listIndex = Convert.ToInt32(reader["listIndex"]);
                    var tag = reader["tag"].ToString();
                    var pickCommand = Convert.ToBoolean(reader["pickCommand"]);
                    var mappoolMap = new MappoolMap(id)
                    {
                        Mappool = mappool,
                        Beatmap = beatmap,
                        ListIndex = listIndex,
                        Tag = tag,
                        PickCommand = pickCommand
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
            localLog.Information("add map '{map}' to mappool '{mappool}'", map.Beatmap.Id, map.Mappool.Name);
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("INSERT INTO MappoolMaps (mappool, beatmap, listIndex, mods, tag, pickCommand) VALUES (@mappool, @beatmap, @listIndex, @mods, @tag, @pickCommand)", conn))
                {
                    command.Parameters.AddWithValue("@mappool", map.Mappool.Id);
                    command.Parameters.AddWithValue("@beatmap", map.Beatmap.Id);
                    command.Parameters.AddWithValue("@listIndex", map.ListIndex);
                    command.Parameters.AddWithValue("@mods", string.Join(",", map.Mods));
                    command.Parameters.AddWithValue("@tag", map.Tag);
                    command.Parameters.AddWithValue("@pickCommand", map.PickCommand);
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
            localLog.Information("remove mappool map '{map}' from mappool '{mappool}'", map.Beatmap.Id, map.Mappool.Name);
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
            localLog.Information("update map '{map}' in mappool '{mappool}'", map.Beatmap.Id, map.Mappool.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"UPDATE MappoolMaps
                SET beatmap = @beatmap, listIndex = @listIndex, mods = @mods, tag = @tag, pickCommand = @pickCommand
                WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@beatmap", map.Beatmap.Id);
                command.Parameters.AddWithValue("@listIndex", map.ListIndex);
                command.Parameters.AddWithValue("@mods", string.Join(",", map.Mods));
                command.Parameters.AddWithValue("@id", map.Id);
                command.Parameters.AddWithValue("@tag", map.Tag);
                command.Parameters.AddWithValue("@pickCommand", map.PickCommand);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        #endregion

        #region Players
        public static void AddPlayer(Player player)
        {
            localLog.Information("add new player '{name}'", player.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"INSERT INTO Players (id, name, country)
                SELECT @id, @name, @country
                WHERE NOT EXISTS (SELECT 1 FROM Players WHERE id = @id2)", conn))
            {
                command.Parameters.AddWithValue("@id", player.Id);
                command.Parameters.AddWithValue("@name", player.Name);
                command.Parameters.AddWithValue("@country", player.Country);
                command.Parameters.AddWithValue("@id2", player.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static async Task<Player> GetPlayer(string idOrName)
        {
            localLog.Information("get player '{name}'", idOrName);
            if (string.IsNullOrEmpty(idOrName))
                return null;
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
                        returnValue = await OsuApi.OsuApi.GetPlayer(idOrName);
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
            localLog.Information("add player '{player}' to team '{team}'", player.Name, team.Name);
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
            localLog.Information("remove player '{player}' from team '{team}'", player.Name, team.Name);
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
            localLog.Information("init teams");
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
            localLog.Information("add team '{name}'", team.Name);
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
            localLog.Information("update team '{name}'", team.Name);
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
            localLog.Information("delete team '{name}'", team.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("DELETE FROM Teams WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@id", team.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
            Teams.Remove(team);
        }

        public static async Task InitTeamPlayers()
        {
            localLog.Information("init team players");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT player, team FROM TeamPlayers", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var playerId = Convert.ToInt32(reader["player"]);
                    var teamId = Convert.ToInt32(reader["team"]);
                    Teams.First(x => x.Id == teamId).Players.Add(await GetPlayer(playerId.ToString()));
                }
                reader.Close();
                conn.Close();
            }
        }
        #endregion

        #region Matches
        public static async Task InitMatches()
        {
            localLog.Information("init matches");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"SELECT id, tournament, mappool, name, roomId, gameMode, teamMode, winCondition, teamBlue, teamBluePoints, teamRed, teamRedPoints, teamSize, roomSize,
                rollWinner, firstPicker, BO, viewerMode, mpTimerCommand, mpTimerAfterGame, mpTimerAfterPick, pointsForSecondBan, allPicksFreemod, status, warmupMode, matchTime FROM Matches", conn))
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
                        case "BattleRoyale": teamMode = TeamModes.BattleRoyale; break;
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
                            rollWinnerPlayer = await GetPlayer(reader["rollWinner"].ToString());
                        if (reader["firstPicker"] != DBNull.Value)
                            firstPickerPlayer = await GetPlayer(reader["firstPicker"].ToString());
                    }
                    var teamRedPoints = Convert.ToInt32(reader["teamRedPoints"]);
                    var teamBluePoints = Convert.ToInt32(reader["teamBluePoints"]);
                    var teamSize = Convert.ToInt32(reader["teamSize"]);
                    var roomSize = Convert.ToInt32(reader["roomSize"]);
                    var bo = Convert.ToInt32(reader["BO"]);
                    var viewerMode = Convert.ToBoolean(reader["viewerMode"]);
                    var mpTimerCommand = Convert.ToInt32(reader["mpTimerCommand"]);
                    var mpTimerAfterGame = Convert.ToInt32(reader["mpTimerAfterGame"]);
                    var mpTimerAfterPick = Convert.ToInt32(reader["mpTimerAfterPick"]);
                    var pointsForSecondBan = Convert.ToInt32(reader["pointsForSecondBan"]);
                    var allPicksFreemod = Convert.ToBoolean(reader["allPicksFreemod"]);
                    var status = MatchStatus.New;
                    var warmupMode = Convert.ToBoolean(reader["warmupMode"]);
                    DateTime? matchTime = null;
                    if (reader["matchTime"] != DBNull.Value)
                        matchTime = DateTime.Parse(reader["matchTime"].ToString());
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
                        ViewerMode = viewerMode,
                        MpTimerCommand = mpTimerCommand,
                        MpTimerAfterGame = mpTimerAfterGame,
                        MpTimerAfterPick = mpTimerAfterPick,
                        PointsForSecondBan = pointsForSecondBan,
                        AllPicksFreemod = allPicksFreemod,
                        Status = status,
                        WarmupMode = warmupMode,
                        MatchTime = matchTime
                    };
                    Matches.Add(match);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddMatch(Match match)
        {
            localLog.Information("add new match '{name}'", match.Name);
            int resultValue;
            using (var conn = GetConnection())
            using (var transaction = conn.BeginTransaction())
            {
                using (var command = new SQLiteCommand("INSERT INTO Matches (tournament, mappool, name, roomId, gameMode, teamMode, winCondition, teamBlue, teamBluePoints, teamRed, teamRedPoints, teamSize, roomSize, rollWinner, firstPicker, BO, viewerMode, mpTimerCommand, mpTimerAfterGame, mpTimerAfterPick, pointsForSecondBan, allPicksFreemod, status, warmupMode, matchTime)" +
                    "VALUES (@tournament, @mappool, @name, @roomId, @gameMode, @teamMode, @winCondition, @teamBlue, @teamBluePoints, @teamRed, @teamRedPoints, @teamSize, @roomSize, @rollWinner, @firstPicker, @BO, @viewerMode, @mpTimerCommand, @mpTimerAfterGame, @mpTimerAfterPick, @pointsForSecondBan, @allPicksFreemod, @status, @warmupMode, @matchTime)", conn))
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
                    command.Parameters.AddWithValue("@viewerMode", match.ViewerMode);
                    command.Parameters.AddWithValue("@mpTimerCommand", match.MpTimerCommand);
                    command.Parameters.AddWithValue("@mpTimerAfterGame", match.MpTimerAfterGame);
                    command.Parameters.AddWithValue("@mpTimerAfterPick", match.MpTimerAfterPick);
                    command.Parameters.AddWithValue("@pointsForSecondBan", match.PointsForSecondBan);
                    command.Parameters.AddWithValue("@allPicksFreemod", match.AllPicksFreemod);
                    command.Parameters.AddWithValue("@status", match.Status.ToString());
                    command.Parameters.AddWithValue("@warmupMode", match.WarmupMode);
                    if (match.MatchTime != null)
                        command.Parameters.AddWithValue("@matchTime", match.MatchTime);
                    else
                        command.Parameters.AddWithValue("@matchTime", DBNull.Value);
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
                if (match.TeamMode == TeamModes.BattleRoyale)
                {
                    foreach (var team in match.TeamsBR)
                    {
                        using (var command = new SQLiteCommand("INSERT INTO MatchTeamsBR (match, team, lives) VALUES (@match, @team, @lives)", conn))
                        {
                            command.Parameters.AddWithValue("@match", resultValue);
                            command.Parameters.AddWithValue("@team", team.Key.Id);
                            command.Parameters.AddWithValue("@lives", team.Value);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                foreach (var pick in match.Picks)
                {
                    using (var command = new SQLiteCommand("INSERT INTO MatchPicks (match, beatmap, picker, ban, listIndex) VALUES (@match, @beatmap, @picker, False, @listIndex)", conn))
                    {
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.Parameters.AddWithValue("@beatmap", pick.Map.Id);
                        if (match.TeamMode == TeamModes.TeamVS)
                            command.Parameters.AddWithValue("@picker", pick.Team.Id);
                        else if (match.TeamMode == TeamModes.HeadToHead)
                            command.Parameters.AddWithValue("@picker", pick.Player.Id);
                        command.Parameters.AddWithValue("@listIndex", pick.ListIndex);
                        command.ExecuteNonQuery();
                    }
                }
                foreach (var pick in match.Bans)
                {
                    using (var command = new SQLiteCommand("INSERT INTO MatchPicks (match, beatmap, picker, ban, listIndex) VALUES (@match, @beatmap, @picker, True, @listIndex)", conn))
                    {
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.Parameters.AddWithValue("@beatmap", pick.Map.Id);
                        if (match.TeamMode == TeamModes.TeamVS)
                            command.Parameters.AddWithValue("@picker", pick.Team.Id);
                        else if (match.TeamMode == TeamModes.HeadToHead)
                            command.Parameters.AddWithValue("@picker", pick.Player.Id);
                        command.Parameters.AddWithValue("@listIndex", pick.ListIndex);
                        command.ExecuteNonQuery();
                    }
                }
                foreach (var game in match.Games)
                {
                    using (var command = new SQLiteCommand("INSERT INTO Games (id, match, beatmap, mods, counted, warmup) VALUES (@id, @match, @beatmap, @mods, @counted, @warmup)", conn))
                    {
                        command.Parameters.AddWithValue("@id", game.Id);
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.Parameters.AddWithValue("@beatmap", game.Beatmap.Id);
                        command.Parameters.AddWithValue("@mods", string.Join(",", game.Mods));
                        command.Parameters.AddWithValue("@counted", game.Counted);
                        command.Parameters.AddWithValue("@warmup", game.Warmup);
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
            localLog.Information("update match '{name}'", match.Name);
            using (var conn = GetConnection())
            using (var transaction = conn.BeginTransaction())
            {
                using (var command = new SQLiteCommand(@"UPDATE Matches
                SET tournament = @tournament, mappool = @mappool, name = @name, roomId = @roomId, gameMode = @gameMode, teamMode = @teamMode, winCondition = @winCondition, teamBlue = @teamBlue, teamBluePoints = @teamBluePoints,
                teamRed = @teamRed, teamRedPoints = @teamRedPoints, teamSize = @teamSize, roomSize = @roomSize, rollWinner = @rollWinner, firstPicker = @firstPicker, BO = @BO,
                viewerMode = @viewerMode, mpTimerCommand = @mpTimerCommand, mpTimerAfterGame = @mpTimerAfterGame, mpTimerAfterPick = @mpTimerAfterPick, pointsForSecondBan = @pointsForSecondBan,
                allPicksFreemod = @allPicksFreemod, status = @status, warmupMode = @warmupMode, matchTime = @matchTime
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
                        command.Parameters.AddWithValue("@teamBlue", DBNull.Value);
                        command.Parameters.AddWithValue("@teamRed", DBNull.Value);
                    }
                    command.Parameters.AddWithValue("@teamBluePoints", match.TeamBluePoints);
                    command.Parameters.AddWithValue("@teamRedPoints", match.TeamRedPoints);
                    command.Parameters.AddWithValue("@teamSize", match.TeamSize);
                    command.Parameters.AddWithValue("@roomSize", match.RoomSize);
                    command.Parameters.AddWithValue("@BO", match.BO);
                    command.Parameters.AddWithValue("@viewerMode", match.ViewerMode);
                    command.Parameters.AddWithValue("@mpTimerCommand", match.MpTimerCommand);
                    command.Parameters.AddWithValue("@mpTimerAfterGame", match.MpTimerAfterGame);
                    command.Parameters.AddWithValue("@mpTimerAfterPick", match.MpTimerAfterPick);
                    command.Parameters.AddWithValue("@pointsForSecondBan", match.PointsForSecondBan);
                    command.Parameters.AddWithValue("@allPicksFreemod", match.AllPicksFreemod);
                    command.Parameters.AddWithValue("@status", match.Status.ToString());
                    command.Parameters.AddWithValue("@warmupMode", match.WarmupMode);
                    if (match.MatchTime != null)
                        command.Parameters.AddWithValue("@matchTime", match.MatchTime);
                    else
                        command.Parameters.AddWithValue("@matchTime", DBNull.Value);
                    command.Parameters.AddWithValue("@id", match.Id);
                    command.ExecuteNonQuery();
                }
                if (match.TeamMode == TeamModes.HeadToHead)
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
                if (match.TeamMode == TeamModes.BattleRoyale)
                {
                    using (var command = new SQLiteCommand("DELETE FROM MatchTeamsBR WHERE match = @match", conn))
                    {
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.ExecuteNonQuery();
                    }
                    foreach (var team in match.TeamsBR)
                    {
                        using (var command = new SQLiteCommand("INSERT INTO MatchTeamsBR (match, team, lives) VALUES (@match, @team, @lives)", conn))
                        {
                            command.Parameters.AddWithValue("@match", match.Id);
                            command.Parameters.AddWithValue("@team", team.Key.Id);
                            command.Parameters.AddWithValue("@lives", team.Value);
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
                    using (var command = new SQLiteCommand("INSERT INTO MatchPicks (match, beatmap, picker, ban, listIndex) VALUES (@match, @beatmap, @picker, False, @listIndex)", conn))
                    {
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.Parameters.AddWithValue("@beatmap", pick.Map.Id);
                        if (match.TeamMode == TeamModes.TeamVS)
                            command.Parameters.AddWithValue("@picker", pick.Team.Id);
                        else if (match.TeamMode == TeamModes.HeadToHead && pick.Player != null)
                            command.Parameters.AddWithValue("@picker", pick.Player.Id);
                        else
                            command.Parameters.AddWithValue("@picker", DBNull.Value);
                        command.Parameters.AddWithValue("@listIndex", pick.ListIndex);
                        command.ExecuteNonQuery();
                    }
                }
                foreach (var pick in match.Bans)
                {
                    using (var command = new SQLiteCommand("INSERT INTO MatchPicks (match, beatmap, picker, ban, listIndex) VALUES (@match, @beatmap, @picker, True, @listIndex)", conn))
                    {
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.Parameters.AddWithValue("@beatmap", pick.Map.Id);
                        if (match.TeamMode == TeamModes.TeamVS)
                            command.Parameters.AddWithValue("@picker", pick.Team.Id);
                        else if (match.TeamMode == TeamModes.HeadToHead)
                            command.Parameters.AddWithValue("@picker", pick.Player.Id);
                        command.Parameters.AddWithValue("@listIndex", pick.ListIndex);
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
                    using (var command = new SQLiteCommand("INSERT INTO Games (id, match, beatmap, mods, counted, warmup) VALUES (@id, @match, @beatmap, @mods, @counted, @warmup)", conn))
                    {
                        command.Parameters.AddWithValue("@id", game.Id);
                        command.Parameters.AddWithValue("@match", match.Id);
                        command.Parameters.AddWithValue("@beatmap", game.Beatmap.Id);
                        command.Parameters.AddWithValue("@mods", string.Join(",", game.Mods));
                        command.Parameters.AddWithValue("@counted", game.Counted);
                        command.Parameters.AddWithValue("@warmup", game.Warmup);
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
            localLog.Information("delete match '{name}'", match.Name);
            using (var conn = GetConnection())
            using (var transaction = conn.BeginTransaction())
            {
                using (var command = new SQLiteCommand("DELETE FROM MatchPlayers WHERE match = @match", conn))
                {
                    command.Parameters.AddWithValue("@match", match.Id);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("DELETE FROM MatchTeamsBR WHERE match = @match", conn))
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

        public static async Task InitMatchPlayers()
        {
            localLog.Information("init match players and teams");
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
                                    match.Players.Add(await GetPlayer(reader["player"].ToString()), Convert.ToInt32(reader["points"]));
                                }
                                reader.Close();
                            }
                        }
                    }
                    if (match.TeamMode == TeamModes.BattleRoyale)
                    {
                        using (var command = new SQLiteCommand("SELECT team, lives FROM MatchTeamsBR WHERE match = @match", conn))
                        {
                            command.Parameters.AddWithValue("@match", match.Id);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    match.TeamsBR.Add(Teams.First(x => x.Id == Convert.ToInt32(reader["team"])), Convert.ToInt32(reader["lives"]));
                                }
                                reader.Close();
                            }
                        }
                    }
                }
                conn.Close();
            }
        }

        public static async Task InitMatchPicks()
        {
            localLog.Information("init match picks");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT match, beatmap, picker, ban, listIndex FROM MatchPicks ORDER BY listIndex ASC", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var match = Matches.First(x => x.Id == Convert.ToInt32(reader["match"]));
                    var beatmap = match.Mappool.Beatmaps.FirstOrDefault(x => x.Id == Convert.ToInt32(reader["beatmap"]));
                    if (beatmap == null)
                        continue;
                    Team team = null;
                    Player player = null;
                    if (match.TeamMode == TeamModes.TeamVS)
                        team = Teams.First(x => x.Id == Convert.ToInt32(reader["picker"]));
                    else if (match.TeamMode == TeamModes.HeadToHead)
                        player = await GetPlayer(reader["picker"].ToString());
                    var ban = Convert.ToBoolean(reader["ban"]);
                    int listIndex;
                    if (reader["listIndex"] != DBNull.Value)
                        listIndex = Convert.ToInt32(reader["listIndex"]);
                    else
                        listIndex = 0;
                    var pick = new MatchPick()
                    {
                        Match = match,
                        Map = beatmap,
                        Team = team,
                        Player = player,
                        ListIndex = listIndex
                    };
                    if (ban)
                    {
                        if (pick.ListIndex == 0)
                            pick.ListIndex = match.Bans.Count + 1;
                        match.Bans.Add(pick);
                    }
                    else
                    {
                        if (pick.ListIndex == 0)
                            pick.ListIndex = match.Picks.Count + 1;
                        match.Picks.Add(pick);
                    }
                }
                conn.Close();
            }
        }

        public static async Task InitMatchGames()
        {
            localLog.Information("init match games");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, match, beatmap, mods, counted, warmup FROM Games", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    var match = Matches.First(x => x.Id == Convert.ToInt32(reader["match"]));
                    var beatmap = await GetBeatmap(Convert.ToInt32(reader["beatmap"]));
                    var counted = Convert.ToBoolean(reader["counted"]);
                    var warmup = Convert.ToBoolean(reader["warmup"]);
                    var game = new Game()
                    {
                        Id = id,
                        Match = match,
                        Beatmap = beatmap,
                        Counted = counted,
                        Warmup = warmup
                    };
                    foreach (string mod in reader["mods"].ToString().Split(','))
                    {
                        if (Enum.TryParse(mod, out GameMods gameMod))
                        {
                            game.Mods.Add(gameMod);
                        }
                    }
                    using (var command2 = new SQLiteCommand("SELECT player, mods, score, team, passed FROM Scores WHERE game = @game", conn))
                    {
                        command2.Parameters.AddWithValue("@game", game.Id);
                        using (var reader2 = command2.ExecuteReader())
                        {
                            while (reader2.Read())
                            {
                                var player = await GetPlayer(reader2["player"].ToString());
                                var points = Convert.ToInt32(reader2["score"]);
                                var passed = Convert.ToBoolean(reader2["passed"]);
                                var score = new Score()
                                {
                                    Game = game,
                                    Player = player,
                                    Points = points,
                                    Passed = passed
                                };
                                foreach (string mod in reader2["mods"].ToString().Split(','))
                                {
                                    if (Enum.TryParse(mod, out GameMods gameMod))
                                    {
                                        score.Mods.Add(gameMod);
                                    }
                                }
                                if (Enum.TryParse(reader2["team"].ToString(), out LobbyTeams team))
                                {
                                    score.Team = team;
                                }
                                game.Scores.Add(score);
                            }
                        }
                    }
                    match.Games.Add(game);
                }
                conn.Close();
            }
        }

        public static void AddIrcMessages(List<IrcMessage> messages)
        {
            localLog.Information("add irc messages");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand(@"INSERT INTO IrcMessages (match, timestamp, channel, user, message)
                SELECT match, timestamp, channel, user, message
                FROM (SELECT @match as match, @timestamp as timestamp, @channel as channel, @user as user, @message as message)
                WHERE EXISTS
                (
                    SELECT id FROM Matches WHERE id = @match
                ) OR @match IS NULL", conn))
            using (var transaction = conn.BeginTransaction())
            {
                foreach (var message in messages)
                {
                    if (message.Match != null)
                        command.Parameters.AddWithValue("@match", message.Match.Id);
                    else
                        command.Parameters.AddWithValue("@match", DBNull.Value);
                    command.Parameters.AddWithValue("@timestamp", message.Timestamp);
                    command.Parameters.AddWithValue("@channel", message.Channel);
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
            localLog.Information("get irc messages for match '{match}'", match.Name);
            var messages = new List<IrcMessage>();
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT timestamp, channel, user, message FROM IrcMessages WHERE match = @match", conn))
            {
                command.Parameters.AddWithValue("@match", match.Id);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var message = new IrcMessage()
                        {
                            Timestamp = DateTime.Parse(reader["timestamp"].ToString()),
                            Channel = reader["channel"].ToString(),
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

        public static List<IrcMessage> GetIrcMessages(string channel)
        {
            localLog.Information("get irc messages for channel '{channel}'", channel);
            var messages = new List<IrcMessage>();
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT timestamp, user, message FROM IrcMessages WHERE match IS NULL AND channel = @channel", conn))
            {
                command.Parameters.AddWithValue("@channel", channel);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var message = new IrcMessage()
                        {
                            Timestamp = DateTime.Parse(reader["timestamp"].ToString()),
                            Channel = channel,
                            User = reader["user"].ToString(),
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

        #region Custom Commands
        public static void InitCustomCommands()
        {
            localLog.Information("init custom commands");
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT id, name, command, tournament FROM CustomCommands", conn))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["id"]);
                    Tournament tournament = null;
                    if (reader["tournament"] != DBNull.Value)
                        tournament = Tournaments.First(x => x.Id == Convert.ToInt32(reader["tournament"]));
                    var name = reader["name"].ToString();
                    var commandText = reader["command"].ToString();
                    var customCommand = new CustomCommand(id)
                    {
                        Tournament = tournament,
                        Name = name,
                        Command = commandText
                    };
                    CustomCommands.Add(customCommand);
                }
                reader.Close();
                conn.Close();
            }
        }

        public static int AddCustomCommand(CustomCommand customCommand)
        {
            localLog.Information("add custom command '{name}'", customCommand.Name);
            int resultValue;
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("INSERT INTO CustomCommands (tournament, name, command) VALUES (@tournament, @name, @command)", conn))
                {
                    if (customCommand.Tournament == null)
                        command.Parameters.AddWithValue("@tournament", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@tournament", customCommand.Tournament.Id);
                    command.Parameters.AddWithValue("@name", customCommand.Name);
                    command.Parameters.AddWithValue("@command", customCommand.Command);
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("SELECT last_insert_rowid()", conn))
                {
                    resultValue = Convert.ToInt32(command.ExecuteScalar());
                }
                conn.Close();
            }
            CustomCommands.Add(customCommand);
            return resultValue;
        }

        public static void UpdateCustomCommand(CustomCommand customCommand)
        {
            localLog.Information("update custom command '{name}'", customCommand.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("UPDATE CustomCommands SET name = @name, command = @command, tournament = @tournament WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@name", customCommand.Name);
                command.Parameters.AddWithValue("@command", customCommand.Command);
                if (customCommand.Tournament == null)
                    command.Parameters.AddWithValue("@tournament", DBNull.Value);
                else
                    command.Parameters.AddWithValue("@tournament", customCommand.Tournament.Id);
                command.Parameters.AddWithValue("@id", customCommand.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void DeleteCustomCommand(CustomCommand customCommand)
        {
            localLog.Information("delete custom command '{name}'", customCommand.Name);
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("DELETE FROM CustomCommands WHERE id = @id", conn))
            {
                command.Parameters.AddWithValue("@id", customCommand.Id);
                command.ExecuteNonQuery();
                conn.Close();
            }
            CustomCommands.Remove(customCommand);
        }
        #endregion
    }
}
