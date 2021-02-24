using Serilog;
using System;
using System.Data.SQLite;

namespace script_chan2.Database
{
    public static class DbUpgrader
    {
        private static ILogger localLog = Log.ForContext(typeof(DbUpgrader));

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

        public static void Upgrade()
        {
            localLog.Information("database upgrade started");
            var dbVersion = 1;
            using (var conn = GetConnection())
            using (var command = new SQLiteCommand("SELECT value FROM UserSettings WHERE name = 'dbVersion'", conn))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                dbVersion = Convert.ToInt32(reader["value"]);
                reader.Close();
                conn.Close();
            }

            if (dbVersion == 1)
            {
                UpgradeV1();
                dbVersion = 2;
            }
            if (dbVersion == 2)
            {
                UpgradeV2();
                dbVersion = 3;
            }
            if (dbVersion == 3)
            {
                UpgradeV3();
                dbVersion = 4;
            }
            if (dbVersion == 4)
            {
                UpgradeV4();
                dbVersion = 5;
            }
            if (dbVersion == 5)
            {
                UpgradeV5();
                dbVersion = 6;
            }
            if (dbVersion == 6)
            {
                UpgradeV6();
                dbVersion = 7;
            }
            if (dbVersion == 7)
            {
                UpgradeV7();
                dbVersion = 8;
            }
            if (dbVersion == 8)
            {
                UpgradeV8();
                dbVersion = 9;
            }
            if (dbVersion == 9)
            {
                UpgradeV9();
                dbVersion = 10;
            }
            if (dbVersion == 10)
            {
                UpgradeV10();
                dbVersion = 11;
            }
            if (dbVersion == 11)
            {
                UpgradeV11();
                dbVersion = 12;
            }

            localLog.Information("database upgrade finished");
        }

        private static void UpgradeV1()
        {
            localLog.Information("upgrade database to v2");
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand("ALTER TABLE Games ADD COLUMN warmup BOOL", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("UPDATE UserSettings SET value = 2 WHERE name = 'dbVersion'", conn))
                {
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        private static void UpgradeV2()
        {
            localLog.Information("upgrade database to v3");
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"CREATE TABLE CustomCommands
                    (id INTEGER NOT NULL PRIMARY KEY,
                    tournament INTEGER,
                    name TEXT,
                    command TEXT,
                    FOREIGN KEY (tournament) REFERENCES Tournaments(id) ON DELETE CASCADE)", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("UPDATE UserSettings SET value = 3 WHERE name = 'dbVersion'", conn))
                {
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        private static void UpgradeV3()
        {
            localLog.Information("upgrade database to v4");
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"ALTER TABLE Webhooks ADD COLUMN footerText TEXT", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(@"ALTER TABLE Webhooks ADD COLUMN footerIcon TEXT", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(@"UPDATE Webhooks SET footerText = 'Woah! So cool!', footerIcon = 'https://cdn.frankerfacez.com/emoticon/243789/4'", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("UPDATE UserSettings SET value = 4 WHERE name = 'dbVersion'", conn))
                {
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        private static void UpgradeV4()
        {
            localLog.Information("upgrade database to v5");
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"ALTER TABLE Webhooks ADD COLUMN winImage TEXT", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(@"UPDATE Webhooks SET winImage = 'https://78.media.tumblr.com/b94193615145d12bfb64aa77b677269e/tumblr_njzqukOpBP1ti1gm1o1_500.gif'", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("UPDATE UserSettings SET value = 5 WHERE name = 'dbVersion'", conn))
                {
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        private static void UpgradeV5()
        {
            localLog.Information("upgrade database to v6");
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"ALTER TABLE Matches ADD COLUMN viewerMode BOOL", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(@"UPDATE Matches SET viewerMode = ((enableWebhooks | 1) - (enableWebhooks & 1))", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(@"UPDATE Matches SET viewerMode = ((enableWebhooks | 1) - (enableWebhooks & 1))", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("UPDATE UserSettings SET value = 6 WHERE name = 'dbVersion'", conn))
                {
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        private static void UpgradeV6()
        {
            localLog.Information("upgrade database to v7");
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"CREATE TABLE MatchTeamsBR
                (match INTEGER NOT NULL,
                team INTEGER NOT NULL,
                lives INTEGER,
                PRIMARY KEY(match, team),
                FOREIGN KEY(match) REFERENCES Matches(id) ON DELETE CASCADE,
                FOREIGN KEY(team) REFERENCES Teams(id))", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(@"ALTER TABLE Tournaments ADD COLUMN brInitialLivesAmount INTEGER", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("UPDATE UserSettings SET value = 7 WHERE name = 'dbVersion'", conn))
                {
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        private static void UpgradeV7()
        {
            localLog.Information("upgrade database to v8");
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"ALTER TABLE Matches ADD COLUMN matchTime TEXT", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("UPDATE UserSettings SET value = 8 WHERE name = 'dbVersion'", conn))
                {
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        private static void UpgradeV8()
        {
            localLog.Information("upgrade database to v9");
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"ALTER TABLE MatchPicks ADD COLUMN listIndex INTEGER", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("UPDATE UserSettings SET value = 9 WHERE name = 'dbVersion'", conn))
                {
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        private static void UpgradeV9()
        {
            localLog.Information("upgrade database to v10");
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"ALTER TABLE Webhooks ADD COLUMN username TEXT", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(@"ALTER TABLE Webhooks ADD COLUMN avatar TEXT", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("UPDATE UserSettings SET value = 10 WHERE name = 'dbVersion'", conn))
                {
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        private static void UpgradeV10()
        {
            localLog.Information("upgrade database to v11");
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"ALTER TABLE Webhooks ADD COLUMN guild TEXT", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(@"ALTER TABLE Webhooks ADD COLUMN channel TEXT", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("UPDATE UserSettings SET value = 11 WHERE name = 'dbVersion'", conn))
                {
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        private static void UpgradeV11()
        {
            localLog.Information("upgrade database to v12");
            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(@"ALTER TABLE MappoolMaps ADD COLUMN pickCommand BOOL", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(@"UPDATE MappoolMaps SET pickCommand = true", conn))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("UPDATE UserSettings SET value = 12 WHERE name = 'dbVersion'", conn))
                {
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }
    }
}
