using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public static class Settings
    {
        public static string CONFIG_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "script_chan");

        private class Config
        {
            public string apiKey { get; set; }
            public string ircUsername { get; set; }
            public string ircPassword { get; set; }
        }

        private static void SaveConfig()
        {
            var config = new Config
            {
                apiKey = ApiKey,
                ircUsername = IrcUsername,
                ircPassword = IrcPassword
            };
            File.WriteAllText(CONFIG_PATH + "\\config.json", JsonConvert.SerializeObject(config));
        }

        static Settings()
        {
            Directory.CreateDirectory(CONFIG_PATH);
            if (!File.Exists(CONFIG_PATH + "\\config.json"))
                File.WriteAllText(CONFIG_PATH + "\\config.json", "{\"apiKey\":\"\",\"ircUsername\":\"\",\"ircPassword\":\"\"}");

            var settings = Database.Database.GetSettings();
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(CONFIG_PATH + "\\config.json"));
            lang = settings["lang"];
            apiKey = config.apiKey;
            ircUsername = config.ircUsername;
            ircPassword = config.ircPassword;
            ircTimeout = Convert.ToInt32(settings["ircTimeout"]);
            defaultBO = Convert.ToInt32(settings["defaultBO"]);
            var defaultTournamentId = settings["defaultTournament"];
            if (!string.IsNullOrEmpty(defaultTournamentId))
                defaultTournament = Database.Database.Tournaments.First(x => x.Id == Convert.ToInt32(defaultTournamentId));
            defaultTimerCommand = Convert.ToInt32(settings["defaultTimerCommand"]);
            defaultTimerAfterGame = Convert.ToInt32(settings["defaultTimerAfterGame"]);
            defaultTimerAfterPick = Convert.ToInt32(settings["defaultTimerAfterPick"]);
        }

        private static string lang;
        public static string Lang
        {
            get { return lang; }
            set
            {
                if (value != lang)
                {
                    lang = value;
                    Database.Database.UpdateSettings("lang", value);
                }
            }
        }

        private static string apiKey;
        public static string ApiKey
        {
            get { return apiKey; }
            set
            {
                if (value != apiKey)
                {
                    apiKey = value;
                    SaveConfig();
                }
            }
        }

        private static string ircUsername;
        public static string IrcUsername
        {
            get { return ircUsername; }
            set
            {
                if (value != ircUsername)
                {
                    ircUsername = value;
                    SaveConfig();
                }
            }
        }

        private static string ircPassword;
        public static string IrcPassword
        {
            get { return ircPassword; }
            set
            {
                if (value != ircPassword)
                {
                    ircPassword = value;
                    SaveConfig();
                }
            }
        }

        private static int ircTimeout;
        public static int IrcTimeout
        {
            get { return ircTimeout; }
            set
            {
                if (value != ircTimeout)
                {
                    ircTimeout = value;
                    Database.Database.UpdateSettings("ircTimeout", value.ToString());
                }
            }
        }

        private static int defaultBO;
        public static int DefaultBO
        {
            get { return defaultBO; }
            set
            {
                if (value != defaultBO)
                {
                    defaultBO = value;
                    Database.Database.UpdateSettings("defaultBO", value.ToString());
                }
            }
        }

        private static Tournament defaultTournament;
        public static Tournament DefaultTournament
        {
            get { return defaultTournament; }
            set
            {
                if (value != defaultTournament)
                {
                    defaultTournament = value;
                    if (value == null)
                        Database.Database.UpdateSettings("defaultTournament", "");
                    else
                        Database.Database.UpdateSettings("defaultTournament", value.Id.ToString());
                }
            }
        }

        private static int defaultTimerCommand;
        public static int DefaultTimerCommand
        {
            get { return defaultTimerCommand; }
            set
            {
                if (value != defaultTimerCommand)
                {
                    defaultTimerCommand = value;
                    Database.Database.UpdateSettings("defaultTimerCommand", value.ToString());
                }
            }
        }

        private static int defaultTimerAfterGame;
        public static int DefaultTimerAfterGame
        {
            get { return defaultTimerAfterGame; }
            set
            {
                if (value != defaultTimerAfterGame)
                {
                    defaultTimerAfterGame = value;
                    Database.Database.UpdateSettings("defaultTimerAfterGame", value.ToString());
                }
            }
        }

        private static int defaultTimerAfterPick;
        public static int DefaultTimerAfterPick
        {
            get { return defaultTimerAfterPick; }
            set
            {
                if (value != defaultTimerAfterPick)
                {
                    defaultTimerAfterPick = value;
                    Database.Database.UpdateSettings("defaultTimerAfterPick", value.ToString());
                }
            }
        }
    }
}
