﻿using Caliburn.Micro;
using Newtonsoft.Json;
using script_chan2.GUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media;

namespace script_chan2.DataTypes
{
    public static class Settings
    {
        private static ILogger localLog = Log.ForContext(typeof(Settings));

        public static string CONFIG_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "script_chan");

        private static FileSystemWatcher configFileWatcher;

        private class Config
        {
            public string apiKey { get; set; }
            public string ircUsername { get; set; }
            public string ircPassword { get; set; }
            public string ircIpPrivate { get; set; }
            public List<ConfigColor> colors { get; set; }
            public string notificationSoundFile { get; set; }
            public bool enableNotifications { get; set; }
            public double notificationVolume { get; set; }
            public List<ConfigSize> matchSizes { get; set; }
        }

        private class ConfigColor
        {
            public string key { get; set; }
            public string color { get; set; }
        }

        private class ConfigSize
        {
            public string key { get; set; }
            public double size { get; set; }
        }

        public static void SaveConfig()
        {
            localLog.Information("save config");
            configFileWatcher.EnableRaisingEvents = false;
            var config = new Config
            {
                apiKey = ApiKey,
                ircUsername = IrcUsername,
                ircPassword = IrcPassword,
                ircIpPrivate = IrcIpPrivate,
                colors = new List<ConfigColor>(),
                notificationSoundFile = NotificationSoundFile,
                enableNotifications = EnableNotifications,
                notificationVolume = NotificationVolume,
                matchSizes = new List<ConfigSize>
                {
                    new ConfigSize { key = "height", size = WindowHeight },
                    new ConfigSize { key = "width", size = WindowWidth },
                    new ConfigSize { key = "overview2", size = Overview2Height },
                    new ConfigSize { key = "column1", size = Column1Width },
                    new ConfigSize { key = "column2", size = Column2Width }
                }
            };
            foreach (var colorData in UserColors)
            {
                config.colors.Add(new ConfigColor { key = colorData.Key, color = colorData.Color.ToString() });
            }
            File.WriteAllText(CONFIG_PATH + "\\config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
            configFileWatcher.EnableRaisingEvents = true;
        }

        internal static void Initialize()
        {
            localLog.Information("initialize");
            Directory.CreateDirectory(CONFIG_PATH);
            if (!File.Exists(CONFIG_PATH + "\\config.json"))
            {
                localLog.Information("create config file");
                var configNew = new Config
                {
                    apiKey = string.Empty,
                    ircUsername = string.Empty,
                    ircPassword = string.Empty,
                    ircIpPrivate = string.Empty,
                    colors = new List<ConfigColor>
                    {
                        new ConfigColor { key = "BanchoBot", color = DefaultColors.GetDefaultColor("BanchoBot") },
                        new ConfigColor { key = "Self", color = DefaultColors.GetDefaultColor("Self") },
                        new ConfigColor { key = "Default", color = DefaultColors.GetDefaultColor("Default") },
                        new ConfigColor { key = "HD", color = DefaultColors.GetDefaultColor("HD") },
                        new ConfigColor { key = "HR", color = DefaultColors.GetDefaultColor("HR") },
                        new ConfigColor { key = "DT", color = DefaultColors.GetDefaultColor("DT") },
                        new ConfigColor { key = "FL", color = DefaultColors.GetDefaultColor("FL") },
                        new ConfigColor { key = "Freemod", color = DefaultColors.GetDefaultColor("Freemod") },
                        new ConfigColor { key = "Tiebreaker", color = DefaultColors.GetDefaultColor("Tiebreaker") },
                        new ConfigColor { key = "NoFail", color = DefaultColors.GetDefaultColor("NoFail") },
                        new ConfigColor { key = "Protect", color = DefaultColors.GetDefaultColor("Protect") },
                    },
                    notificationSoundFile = string.Empty,
                    enableNotifications = true,
                    notificationVolume = 0.5,
                    matchSizes = new List<ConfigSize>
                    {
                        new ConfigSize { key = "height", size = 700 },
                        new ConfigSize { key = "width", size = 1000 },
                        new ConfigSize { key = "overview2", size = double.NaN },
                        new ConfigSize { key = "column1", size = double.NaN },
                        new ConfigSize { key = "column2", size = double.NaN }
                    }
                };
                File.WriteAllText(CONFIG_PATH + "\\config.json", JsonConvert.SerializeObject(configNew, Formatting.Indented));
            }

            localLog.Information("load settings from database");
            var settings = Database.Database.GetSettings();

            localLog.Information("load config file");
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(CONFIG_PATH + "\\config.json"));

            localLog.Information("initialize misc settings");
            lang = settings["lang"];
            apiKey = config.apiKey;
            ircUsername = config.ircUsername;
            ircPassword = config.ircPassword;
            ircIpPrivate = config.ircIpPrivate;
            if (config.notificationSoundFile == null)
                config.notificationSoundFile = string.Empty;
            notificationSoundFile = config.notificationSoundFile;
            enableNotifications = config.enableNotifications;
            if (config.notificationVolume == 0)
                config.notificationVolume = 0.5;
            notificationVolume = config.notificationVolume;
            ircTimeout = Convert.ToInt32(settings["ircTimeout"]);
            enablePrivateIrc = Convert.ToBoolean(settings["enablePrivateIrc"]);
            defaultBO = Convert.ToInt32(settings["defaultBO"]);
            var defaultTournamentId = settings["defaultTournament"];
            if (!string.IsNullOrEmpty(defaultTournamentId))
                defaultTournament = Database.Database.Tournaments.First(x => x.Id == Convert.ToInt32(defaultTournamentId));
            defaultTimerCommand = Convert.ToInt32(settings["defaultTimerCommand"]);
            defaultTimerAfterGame = Convert.ToInt32(settings["defaultTimerAfterGame"]);
            defaultTimerAfterPick = Convert.ToInt32(settings["defaultTimerAfterPick"]);

            localLog.Information("initialize user colors");
            if (config.colors != null)
            {
                if (!config.colors.Any(x => x.key == "Default"))
                    config.colors.Add(new ConfigColor { key = "Default", color = Colors.Black.ToString() });
                if (!config.colors.Any(x => x.key == "Protect"))
                    config.colors.Add(new ConfigColor { key = "Protect", color = DefaultColors.GetDefaultColor("Protect") });
            }
            else
            {
                config.colors = new List<ConfigColor>
                {
                    new ConfigColor { key = "BanchoBot", color = DefaultColors.GetDefaultColor("BanchoBot") },
                    new ConfigColor { key = "Self", color = DefaultColors.GetDefaultColor("Self") },
                    new ConfigColor { key = "Default", color = DefaultColors.GetDefaultColor("Default") },
                    new ConfigColor { key = "HD", color = DefaultColors.GetDefaultColor("HD") },
                    new ConfigColor { key = "HR", color = DefaultColors.GetDefaultColor("HR") },
                    new ConfigColor { key = "DT", color = DefaultColors.GetDefaultColor("DT") },
                    new ConfigColor { key = "FL", color = DefaultColors.GetDefaultColor("FL") },
                    new ConfigColor { key = "Freemod", color = DefaultColors.GetDefaultColor("Freemod") },
                    new ConfigColor { key = "Tiebreaker", color = DefaultColors.GetDefaultColor("Tiebreaker") },
                    new ConfigColor { key = "NoFail", color = DefaultColors.GetDefaultColor("NoFail") },
                    new ConfigColor { key = "Protect", color = DefaultColors.GetDefaultColor("Protect") },
                };
            }

            UserColors = new List<UserColor>();
            foreach (var configColor in config.colors)
            {
                UserColors.Add(new UserColor { Key = configColor.key, Color = (Color)ColorConverter.ConvertFromString(configColor.color) });
            }

            localLog.Information("initialize window sizes");
            if (config.matchSizes != null)
            {
                if (config.matchSizes.Any(x => x.key == "overview2"))
                    Overview2Height = config.matchSizes.First(x => x.key == "overview2").size;
                else
                    Overview2Height = double.NaN;
                if (config.matchSizes.Any(x => x.key == "column1"))
                    Column1Width = config.matchSizes.First(x => x.key == "column1").size;
                else
                    Column1Width = double.NaN;
                if (config.matchSizes.Any(x => x.key == "column2"))
                    Column2Width = config.matchSizes.First(x => x.key == "column2").size;
                else
                    Column2Width = double.NaN;
                if (config.matchSizes.Any(x => x.key == "height"))
                    WindowHeight = config.matchSizes.First(x => x.key == "height").size;
                else
                    WindowHeight = 700;
                if (config.matchSizes.Any(x => x.key == "width"))
                    WindowWidth = config.matchSizes.First(x => x.key == "width").size;
                else
                    WindowWidth = 1000;
            }
            else
            {
                Overview2Height = double.NaN;
                Column1Width = double.NaN;
                Column2Width = double.NaN;
                WindowHeight = 700;
                WindowWidth = 1000;
            }

            configFileWatcher = new FileSystemWatcher(CONFIG_PATH);
            configFileWatcher.Filter = "config.json";
            configFileWatcher.Changed += ConfigFileWatcher_Changed;
            configFileWatcher.EnableRaisingEvents = true;

            localLog.Information("initialized");
        }

        private static void ConfigFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
                return;

            localLog.Information("config.json got changed outside of script-chan");

            var retry = true;
            while (retry)
            {
                try
                {
                    var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(CONFIG_PATH + "\\config.json"));
                    if (ircIpPrivate != config.ircIpPrivate)
                    {
                        localLog.Information("ircIpPrivate changed");
                        ircIpPrivate = config.ircIpPrivate;
                        OsuIrc.OsuIrc.Login();
                    }
                    retry = false;
                }
                catch
                {
                    //horrible IO stuff happened
                    Thread.Sleep(1000);
                }
            }
            Events.Aggregator.PublishOnUIThread("ConfigFileChanged");
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
                        Database.Database.UpdateSettings("defaultTournament", string.Empty);
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

        private static string notificationSoundFile;
        public static string NotificationSoundFile
        {
            get { return notificationSoundFile; }
            set
            {
                if (value != notificationSoundFile)
                {
                    notificationSoundFile = value;
                    SaveConfig();
                    NotificationPlayer.Refresh();
                }
            }
        }

        private static bool enableNotifications;
        public static bool EnableNotifications
        {
            get { return enableNotifications; }
            set
            {
                if (value != enableNotifications)
                {
                    enableNotifications = value;
                    SaveConfig();
                }
            }
        }

        private static double notificationVolume;
        public static double NotificationVolume
        {
            get { return notificationVolume; }
            set
            {
                if (value != notificationVolume)
                {
                    notificationVolume = value;
                    SaveConfig();
                    NotificationPlayer.Refresh();
                }
            }
        }

        private static double windowHeight;
        public static double WindowHeight
        {
            get { return windowHeight; }
            set
            {
                if (value != windowHeight)
                {
                    windowHeight = value;
                }
            }
        }

        private static double windowWidth;
        public static double WindowWidth
        {
            get { return windowWidth; }
            set
            {
                if (value != windowWidth)
                {
                    windowWidth = value;
                }
            }
        }

        private static double overview2Height;
        public static double Overview2Height
        {
            get { return overview2Height; }
            set
            {
                if (value != overview2Height)
                {
                    overview2Height = value;
                }
            }
        }

        private static double column1Width;
        public static double Column1Width
        {
            get { return column1Width; }
            set
            {
                if (value != column1Width)
                {
                    column1Width = value;
                }
            }
        }

        private static double column2Width;
        public static double Column2Width
        {
            get { return column2Width; }
            set
            {
                if (value != column2Width)
                {
                    column2Width = value;
                }
            }
        }
    }
}
