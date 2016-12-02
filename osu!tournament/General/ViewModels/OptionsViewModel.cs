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

        private OsuMode gm;

        private Osu.Scores.Mappool mappool;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public OptionsViewModel()
        {
            cache = Cache.GetCache("osu!options.db");
            DisplayName = "Options";
            bot = OsuIrcBot.GetInstance();
            string t = Cache.GetCache("osu!options.db").Get("wctype", "Standard");
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
                SelectedMappool = Mappool.Mappools.First(x => x.Name == pool);

            LoadBot();
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
                    NotifyOfPropertyChange(() => Password);
                    NotifyOfPropertyChange(() => CanConnect);
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
                    cache["defaultmappool"] = value.Name;
                }
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
