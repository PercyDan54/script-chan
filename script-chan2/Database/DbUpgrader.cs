using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
