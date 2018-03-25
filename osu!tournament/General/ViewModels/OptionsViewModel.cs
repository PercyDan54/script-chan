using Caliburn.Micro;
using Osu.Api;
using Osu.Ircbot;
using Osu.Mvvm.Mappools.ViewModels;
using Osu.Utils;
using Osu.Mvvm.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Osu.Scores;
using System.Linq;
using Osu.Utils.Bans;
using MahApps.Metro;
using System.Windows;
using Osu.Utils.Info;

namespace Osu.Mvvm.General.ViewModels
{
    /// <summary>
    /// Represents the options view model
    /// </summary>
    public class OptionsViewModel : Conductor<MappoolViewModel>.Collection.OneActive
    {
        #region Attributes
        /// <summary>
        /// The linked cache
        /// </summary>
        protected Cache cache;

        /// <summary>
        /// The osu!ircbot
        /// </summary>
        private OsuIrcBot bot;

        private OsuIrcBot botpublic;

        private OsuMode gm;

        private Osu.Scores.Mappool mappool;

        private string colorMode;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public OptionsViewModel()
        {
            cache = Cache.GetCache("osu!options.db");
            DisplayName = "Options";
            bot = OsuIrcBot.GetInstancePrivate();
            botpublic = OsuIrcBot.GetInstancePublic();
            string t = cache.Get("wctype", "Standard");
            string cMode = cache.Get("colormode", "BaseLight");
            //SelectedColorMode = cMode;

            string pool = Cache.GetCache("osu!options.db").Get("defaultmappool", "");
            switch (t)
            {
                case "Standard":
                    SelectedGameMode = OsuMode.Standard;
                    break;
                case "Taiko":
                    SelectedGameMode = OsuMode.Taiko;
                    break;
                case "CTB":
                    SelectedGameMode = OsuMode.CTB;
                    break;
                case "Mania":
                    SelectedGameMode = OsuMode.Mania;
                    break;
            }

            if(!string.IsNullOrEmpty(pool))
                SelectedMappool = Mappool.Mappools.FirstOrDefault(x => x.Name == pool);

            Mappool.ChangeEvent += e_ChangeEvent(); 

            LoadBot();
        }

        private EventHandler<EventArgs> e_ChangeEvent()
        {
            NotifyOfPropertyChange(() => Mappools);
            SelectedMappool = null;
            NotifyOfPropertyChange(() => SelectedMappool);

            return null;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Username property
        /// </summary>
        public string Username
        {
            get
            {
                return bot.Username;
            }
            set
            {
                if (value != bot.Username)
                {
                    bot.Username = value;
                    botpublic.Username = value;
                    NotifyOfPropertyChange(() => Username);
                    NotifyOfPropertyChange(() => CanConnect);
                }
            }
        }

        /// <summary>
        /// Username enabled property
        /// </summary>
        public bool UsernameEnabled
        {
            get
            {
                return !bot.IsConnected;
            }
        }

        /// <summary>
        /// Password property
        /// </summary>
        public string Password
        {
            get
            {
                return bot.Password;
            }
            set
            {
                if (value != bot.Password)
                {
                    bot.Password = value;
                    botpublic.Password = value;
                    NotifyOfPropertyChange(() => Password);
                    NotifyOfPropertyChange(() => CanConnect);
                }
            }
        }

        /// <summary>
        /// RateLimit property
        /// </summary>
        public long RateLimit
        {
            get
            {
                return bot.RateLimit;
            }
            set
            {
                if (value != bot.RateLimit && value > 0)
                {
                    bot.RateLimit = value;
                    botpublic.RateLimit = value;
                    NotifyOfPropertyChange(() => RateLimit);
                }
            }
        }

        /// <summary>
        /// Password enabled property
        /// </summary>
        public bool PasswordEnabled
        {
            get
            {
                return !bot.IsConnected;
            }
        }

        /// <summary>
        /// ApiKeyInput property
        /// </summary>
        public string ApiKey
        {
            get
            {
                return OsuApi.Key;
            }
            set
            {
                if (value != OsuApi.Key)
                {
                    OsuApi.Key = value;
                    NotifyOfPropertyChange(() => ApiKey);
                    NotifyOfPropertyChange(() => CanSave);
                }
            }
        }

        public List<string> ColorMode
        {
            get
            {
                return new List<string> { "BaseLight", "BaseDark" };
            }
        }

        public string test
        {
            get;set;
        }

        /// <summary>
        /// The selected BO property
        /// </summary>
        public string SelectedColorMode
        {
            get
            {
                return colorMode;
            }

            set
            {
                var theme = ThemeManager.DetectAppStyle(Application.Current);
                var bob = ThemeManager.GetAccent(value);
                ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, ThemeManager.GetAppTheme(value));
                NotifyOfPropertyChange(() => SelectedColorMode);
            }
        }

        /// <summary>
        /// Connect button text property
        /// </summary>
        public string ConnectButtonText
        {
            get
            {
                return bot.IsConnected ? "Disconnect" : "Connect";
            }
        }

        /// <summary>
        /// The list of allowed BO number property
        /// </summary>
        public List<string> Modes
        {
            get
            {
                return new List<string> { "3", "5", "7", "9", "11", "13" };
            }
        }

        public List<OsuMode> GameModes
        {
            get
            {
                return new List<OsuMode> { OsuMode.Standard, OsuMode.Taiko, OsuMode.CTB, OsuMode.Mania };
            }
        }

        public List<Osu.Scores.Mappool> Mappools
        {
            get
            {
                return Mappool.Mappools.ToList();
            }
        }
        /// <summary>
        /// The selected BO property
        /// </summary>
        public string SelectedMode
        {
            get
            {
                return cache.Get("mode", "3");
            }

            set
            {
                if (cache.Get("mode", "3") != value)
                {
                    cache["mode"] = value;
                    NotifyOfPropertyChange(() => SelectedMode);
                }

            }
        }

        public OsuMode SelectedGameMode
        {
            get
            {
                return gm;
            }

            set
            {
                if (gm != value)
                {
                    gm = value;
                    cache["wctype"] = value.ToString();
                }
            }
        }

        public Osu.Scores.Mappool SelectedMappool
        {
            get
            {
                return mappool;
            }
            set
            {
                if (mappool != value)
                {
                    mappool = value;
                    cache["defaultmappool"] = value == null ? null : value.Name;
                }
            }
        }

        public string DefaultWebhook
        {
            get
            {
                return InfosHelper.UserDataInfos.WebhookDefault;
            }
            set
            {
                InfosHelper.UserDataInfos.WebhookDefault = value;
            }
        }

        public string AdminWebhook
        {
            get
            {
                return InfosHelper.UserDataInfos.WebhookAdmins;
            }
            set
            {
                InfosHelper.UserDataInfos.WebhookAdmins = value;
            }
        }

        public int Timer
        {
            get { return InfosHelper.TourneyInfos.Timer; }
            set
            {
                InfosHelper.TourneyInfos.Timer = Math.Abs(value);
                NotifyOfPropertyChange(() => Timer);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Loads the osu!ircbot
        /// </summary>
        private async void LoadBot()
        {
            await bot.Connect();
            await botpublic.Connect();

            NotifyOfPropertyChange(() => UsernameEnabled);
            NotifyOfPropertyChange(() => PasswordEnabled);
            NotifyOfPropertyChange(() => ConnectButtonText);

            bot.TokenEvent += new EventHandler<EventArgs>(OnBadToken);
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Checks if the save button can be pressed
        /// </summary>
        public bool CanSave
        {
            get
            {
                return !string.IsNullOrEmpty(OsuApi.Key) && OsuApi.Key.Length == 40;
            }
        }

        /// <summary>
        /// Saves the api key
        /// </summary>
        public async void Save()
        {
            await OsuApi.CheckKey();

            if (OsuApi.Valid)
                Dialog.ShowDialog("Good!", "The osu!api key is valid!");
            else
                Dialog.ShowDialog("Whoops!", "The provided osu!api key is not valid!");
        }

        public void SavePath()
        {
            ObsBanHelper.CheckPath();

            if(ObsBanHelper.IsValid)
                Dialog.ShowDialog("Good!", "The obs folder is valid!");
            else
                Dialog.ShowDialog("Whoops!", "The provided obs folder is not valid!");
        }

        /// <summary>
        /// Checks if the connect button can be pressed
        /// </summary>
        public bool CanConnect
        {
            get
            {
                return !string.IsNullOrEmpty(bot.Username) && !string.IsNullOrEmpty(bot.Password);
            }
        }

        /// <summary>
        /// Connects the irc bot
        /// </summary>
        public async void Connect()
        {
            if (bot.IsConnected)
            {
                bot.Disconnect();
            }
            else if (!await bot.Connect())
            {
                Dialog.ShowDialog("Whoops!", "Your username or password isn't working!");
            }

            // Connect button text has changed
            NotifyOfPropertyChange(() => UsernameEnabled);
            NotifyOfPropertyChange(() => PasswordEnabled);
            NotifyOfPropertyChange(() => ConnectButtonText);
        }
        #endregion

        #region Handlers
        /// <summary>
        /// Called when a bad username/password couple is entered in the irc bot
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the arguments</param>
        public void OnBadToken(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => PasswordEnabled);
            NotifyOfPropertyChange(() => UsernameEnabled);
            NotifyOfPropertyChange(() => ConnectButtonText);
        }
        #endregion
    }
}
