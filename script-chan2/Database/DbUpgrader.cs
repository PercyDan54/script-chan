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
                UpgradeV1();
        }

        private static void UpgradeV1()
        {
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
    }
}
