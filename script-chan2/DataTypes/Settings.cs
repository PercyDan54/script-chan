using Caliburn.Micro;
using Newtonsoft.Json;
using script_chan2.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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
            public string ircIpPrivate { get; set; }
            public List<ConfigColor> colors { get; set; }
        }

        private class ConfigColor
        {
            public string key { get; set; }
            public string color { get; set; }
        }

        public static void SaveConfig()
        {
            var config = new Config
            {
                apiKey = ApiKey,
                ircUsername = IrcUsername,
                ircPassword = IrcPassword,
                ircIpPrivate = IrcIpPrivate,
                colors = new List<ConfigColor>()
            };
            foreach (var colorData in UserColors)
            {
                config.colors.Add(new ConfigColor { key = colorData.Key, color = colorData.Color.ToString() });
            }
            File.WriteAllText(CONFIG_PATH + "\\config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        internal static void Initialize()
        {
            Directory.CreateDirectory(CONFIG_PATH);
            if (!File.Exists(CONFIG_PATH + "\\config.json"))
            {
                var configNew = new Config
                {
                    apiKey = "",
                    ircUsername = "",
                    ircPassword = "",
                    ircIpPrivate = "",
                    colors = new List<ConfigColor>
                    {
                        new ConfigColor { key = "BanchoBot", color = Colors.Pink.ToString() },
                        new ConfigColor { key = "Self", color = Colors.Green.ToString() },
                        new ConfigColor { key = "HD", color = "#FFF2CC" },
                        new ConfigColor { key = "HR", color = "#f4cccc" },
                        new ConfigColor { key = "DT", color = "#cfe2f3" },
                        new ConfigColor { key = "FL", color = "#bdbdbd" },
                        new ConfigColor { key = "Freemod", color = "#d9d2e9" },
                        new ConfigColor { key = "Tiebreaker", color = "#d9ead3" },
                        new ConfigColor { key = "NoFail", color = "#f97ae4" }
                    }
                };
                File.WriteAllText(CONFIG_PATH + "\\config.json", JsonConvert.SerializeObject(configNew, Formatting.Indented));
            }

            var settings = Database.Database.GetSettings();
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(CONFIG_PATH + "\\config.json"));
            lang = settings["lang"];
            apiKey = config.apiKey;
            ircUsername = config.ircUsername;
            ircPassword = config.ircPassword;
            ircIpPrivate = config.ircIpPrivate;
            ircTimeout = Convert.ToInt32(settings["ircTimeout"]);
            enablePrivateIrc = Convert.ToBoolean(settings["enablePrivateIrc"]);
            defaultBO = Convert.ToInt32(settings["defaultBO"]);
            var defaultTournamentId = settings["defaultTournament"];
            if (!string.IsNullOrEmpty(defaultTournamentId))
                defaultTournament = Database.Database.Tournaments.First(x => x.Id == Convert.ToInt32(defaultTournamentId));
            defaultTimerCommand = Convert.ToInt32(settings["defaultTimerCommand"]);
            defaultTimerAfterGame = Convert.ToInt32(settings["defaultTimerAfterGame"]);
            defaultTimerAfterPick = Convert.ToInt32(settings["defaultTimerAfterPick"]);
            UserColors = new List<UserColor>();
            
            if(config.colors != null)
            {
                foreach (var configColor in config.colors)
                {
                    UserColors.Add(new UserColor { Key = configColor.key, Color = (Color)ColorConverter.ConvertFromString(configColor.color) });
                }
            }
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
                    OsuIrc.OsuIrc.Login();
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
                    OsuIrc.OsuIrc.Login();
                }
            }
        }

        private static string ircIpPrivate;
        public static string IrcIpPrivate
        {
            get { return ircIpPrivate; }
        }

        private static bool enablePrivateIrc;
        public static bool EnablePrivateIrc
        {
            get { return enablePrivateIrc; }
            set
            {
                if (value != enablePrivateIrc)
                {
                    enablePrivateIrc = value;
                    OsuIrc.OsuIrc.Login();
                    Database.Database.UpdateSettings("enablePrivateIrc", value.ToString());
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
                    Events.Aggregator.PublishOnUIThread("UpdateDefaultTournament");
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

        public static List<UserColor> UserColors;
    }
}
