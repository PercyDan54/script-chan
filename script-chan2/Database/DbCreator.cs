using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.Database
{
    public static class DbCreator
    {
        private static ILogger localLog = Log.ForContext(typeof(DbCreator));

        private static SQLiteConnection conn;

        public static void CreateDb()
        {
            localLog.Information("database creation started");
            SQLiteConnection.CreateFile("Database.sqlite");
            conn = new SQLiteConnection("Data Source=Database.sqlite;Version=3");
            conn.Open();
            CreateUserSettingsTable();
            CreateTournamentsTable();
            CreateWebhooksTable();
            CreateWebhookLinksTable();
            CreateMappoolsTable();
            CreateBeatmapsTable();
            CreateMappoolMapsTable();
            CreateTeamsTable();
            CreatePlayersTable();
            CreateTeamPlayersTable();
            CreateMatchesTable();
            CreateMatchPlayersTable();
            CreateMatchPicksTable();
            CreateGamesTable();
            CreateScoresTable();
            CreateIrcMessagesTable();
            CreateHeadToHeadPointsTable();
            CreateCustomCommandsTable();
            conn.Close();
            localLog.Information("database creation finished");
        }

        public static bool DbExists
        {
            get { return File.Exists("Database.sqlite"); }
        }

        private static void CreateUserSettingsTable()
        {
            localLog.Information("create table UserSettings");
            using (var command = new SQLiteCommand(@"CREATE TABLE UserSettings
                (name TEXT NOT NULL PRIMARY KEY,
                value TEXT)", conn))
            {
                command.ExecuteNonQuery();
            }
            using (var command = new SQLiteCommand(@"INSERT INTO UserSettings (name, value) VALUES
                ('lang', 'en-US'),
                ('ircTimeout', '1000'),
                ('enablePrivateIrc', 'False'),
                ('defaultBO', '3'),
                ('defaultTournament', ''),
                ('defaultTimerCommand', '120'),
                ('defaultTimerAfterGame', '120'),
                ('defaultTimerAfterPick', '120'),
                ('dbVersion', '5')", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateTournamentsTable()
        {
            localLog.Information("create table Tournaments");
            using (var command = new SQLiteCommand(@"CREATE TABLE Tournaments
                (id INTEGER NOT NULL PRIMARY KEY,
                name TEXT NOT NULL,
                gameMode TEXT,
                teamMode TEXT,
                winCondition TEXT,
                acronym TEXT,
                teamSize INTEGER,
                roomSize INTEGER,
                pointsForSecondBan INTEGER,
                allPicksFreemod BOOL,
                mpTimerCommand INTEGER,
                mpTimerAfterGame INTEGER,
                mpTimerAfterPick INTEGER,
                welcomeString TEXT)", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateWebhooksTable()
        {
            localLog.Information("create table Webhooks");
            using (var command = new SQLiteCommand(@"CREATE TABLE Webhooks
                (id INTEGER NOT NULL PRIMARY KEY,
                name TEXT NOT NULL,
                url TEXT,
                matchCreated BOOL,
                banRecap BOOL,
                pickRecap BOOL,
                gameRecap BOOL,
                footerText TEXT,
                footerIcon TEXT,
                winImage TEXT)", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateWebhookLinksTable()
        {
            localLog.Information("create table WebhookLinks");
            using (var command = new SQLiteCommand(@"CREATE TABLE WebhookLinks
                (tournament INTEGER NOT NULL,
                webhook INTEGER NOT NULL,
                PRIMARY KEY(tournament, webhook),
                FOREIGN KEY(tournament) REFERENCES Tournaments(id) ON DELETE CASCADE,
                FOREIGN KEY(webhook) REFERENCES Webhooks(id) ON DELETE CASCADE)", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateMappoolsTable()
        {
            localLog.Information("create table Mappools");
            using (var command = new SQLiteCommand(@"CREATE TABLE Mappools
                (id INTEGER NOT NULL PRIMARY KEY,
                name TEXT NOT NULL,
                tournament INTEGER,
                FOREIGN KEY(tournament) REFERENCES Tournaments(id) ON DELETE CASCADE)", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateBeatmapsTable()
        {
            localLog.Information("create table Beatmaps");
            using (var command = new SQLiteCommand(@"CREATE TABLE Beatmaps
                (id INTEGER NOT NULL PRIMARY KEY,
                beatmapsetId INT,
                artist TEXT,
                title TEXT,
                version TEXT,
                creator TEXT,
                bpm REAL,
                ar REAL,
                cs REAL)", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateMappoolMapsTable()
        {
            localLog.Information("create table MappoolMaps");
            using (var command = new SQLiteCommand(@"CREATE TABLE MappoolMaps
                (id INTEGER NOT NULL PRIMARY KEY,
                mappool INTEGER NOT NULL,
                beatmap INTEGER NOT NULL,
                listIndex INTEGER,
                mods TEXT,
                tag TEXT,
                FOREIGN KEY(mappool) REFERENCES Mappools(id) ON DELETE CASCADE,
                FOREIGN KEY(beatmap) REFERENCES Beatmaps(id))", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateTeamsTable()
        {
            localLog.Information("create table Teams");
            using (var command = new SQLiteCommand(@"CREATE TABLE Teams
                (id INTEGER NOT NULL PRIMARY KEY,
                name TEXT NOT NULL,
                tournament INTEGER,
                FOREIGN KEY(tournament) REFERENCES Tournaments(id) ON DELETE CASCADE,
                CONSTRAINT unique_team UNIQUE (name, tournament))", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreatePlayersTable()
        {
            localLog.Information("create table Players");
            using (var command = new SQLiteCommand(@"CREATE TABLE Players
                (id INTEGER NOT NULL PRIMARY KEY,
                name TEXT NOT NULL,
                country TEXT)", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateTeamPlayersTable()
        {
            localLog.Information("create table TeamPlayers");
            using (var command = new SQLiteCommand(@"CREATE TABLE TeamPlayers
                (player INTEGER NOT NULL,
                team INTEGER NOT NULL,
                PRIMARY KEY (player, team),
                FOREIGN KEY(player) REFERENCES Players(id),
                FOREIGN KEY(team) REFERENCES Teams(id) ON DELETE CASCADE)", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateMatchesTable()
        {
            localLog.Information("create table Matches");
            using (var command = new SQLiteCommand(@"CREATE TABLE Matches
                (id INTEGER NOT NULL PRIMARY KEY,
                name TEXT,
                roomId INTEGER,
                tournament INTEGER,
                mappool TEXT,
                gameMode TEXT,
                teamMode TEXT,
                winCondition TEXT,
                teamBlue INTEGER,
                teamBluePoints INTEGER,
                teamRed INTEGER,
                teamRedPoints INTEGER,
                teamSize INTEGER,
                roomSize INTEGER,
                rollWinner INTEGER,
                firstPicker INTEGER,
                BO INTEGER,
                enableWebhooks BOOL,
                mpTimerCommand INTEGER,
                mpTimerAfterGame INTEGER,
                mpTimerAfterPick INTEGER,
                pointsForSecondBan INTEGER,
                allPicksFreemod BOOL,
                status TEXT,
                warmupMode BOOL,
                FOREIGN KEY(tournament) REFERENCES Tournaments(id) ON DELETE CASCADE,
                FOREIGN KEY(mappool) REFERENCES Mappools(id) ON DELETE SET NULL,
                FOREIGN KEY(teamBlue) REFERENCES Teams(id) ON DELETE RESTRICT,
                FOREIGN KEY(teamRed) REFERENCES Teams(id) ON DELETE RESTRICT)", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateMatchPlayersTable()
        {
            localLog.Information("create table MatchPlayers");
            using (var command = new SQLiteCommand(@"CREATE TABLE MatchPlayers
                (match INTEGER NOT NULL,
                player INTEGER NOT NULL,
                points INTEGER,
                PRIMARY KEY(match, player),
                FOREIGN KEY(match) REFERENCES Matches(id) ON DELETE CASCADE,
                FOREIGN KEY(player) REFERENCES Players(id))", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateMatchPicksTable()
        {
            localLog.Information("create table MatchPicks");
            using (var command = new SQLiteCommand(@"CREATE TABLE MatchPicks
                (match INTEGER NOT NULL,
                beatmap INTEGER NOT NULL,
                picker INTEGER NOT NULL,
                ban BOOL,
                PRIMARY KEY(match, beatmap),
                FOREIGN KEY(match) REFERENCES Matches(id) ON DELETE CASCADE,
                FOREIGN KEY(beatmap) REFERENCES MappoolMaps(id) ON DELETE CASCADE)", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateGamesTable()
        {
            localLog.Information("create table Games");
            using (var command = new SQLiteCommand(@"CREATE TABLE Games
                (id INTEGER NOT NULL PRIMARY KEY,
                match INTEGER,
                beatmap INTEGER,
                mods TEXT,
                counted BOOL,
                warmup BOOL,
                FOREIGN KEY(match) REFERENCES Matches(id) ON DELETE CASCADE,
                FOREIGN KEY(beatmap) REFERENCES Beatmaps(id))", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateScoresTable()
        {
            localLog.Information("create table Scores");
            using (var command = new SQLiteCommand(@"CREATE TABLE Scores
                (player INTEGER NOT NULL,
                game INTEGER NOT NULL,
                mods TEXT,
                score INTEGER,
                team TEXT,
                passed BOOL,
                FOREIGN KEY(player) REFERENCES Players(id),
                FOREIGN KEY(game) REFERENCES Games(id) ON DELETE CASCADE,
                CONSTRAINT unique_score UNIQUE (player, game))", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateIrcMessagesTable()
        {
            localLog.Information("create table IrcMessages");
            using (var command = new SQLiteCommand(@"CREATE TABLE IrcMessages
                (match INTEGER,
                timestamp TEXT,
                channel TEXT,
                user TEXT,
                message TEXT,
                FOREIGN KEY(match) REFERENCES Matches(id) ON DELETE CASCADE)", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateHeadToHeadPointsTable()
        {
            localLog.Information("create table HeadToHeadPoints");
            using (var command = new SQLiteCommand(@"CREATE TABLE HeadToHeadPoints
                (tournament INTEGER NOT NULL,
                place INTEGER,
                points INTEGER,
                FOREIGN KEY (tournament) REFERENCES Tournaments(id) ON DELETE CASCADE)", conn))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateCustomCommandsTable()
        {
            localLog.Information("create table CustomCommands");
            using (var command = new SQLiteCommand(@"CREATE TABLE CustomCommands
                (id INTEGER NOT NULL PRIMARY KEY,
                tournament INTEGER,
                name TEXT,
                command TEXT,
                FOREIGN KEY (tournament) REFERENCES Tournaments(id) ON DELETE CASCADE)", conn))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
