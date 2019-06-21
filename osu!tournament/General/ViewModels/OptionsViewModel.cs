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
using MahApps.Metro;
using System.Windows;
using osu_discord;
using Osu.Utils.Info;
using System.Windows.Markup;

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

        /// <summary>
        /// The osu!ircbot public instance
        /// </summary>
        private OsuIrcBot botpublic;

        /// <summary>
        /// The osu! mode
        /// </summary>
        private OsuMode gm;

        /// <summary>
        /// The default mappool when you create a room
        /// </summary>
        private Osu.Scores.Mappool mappool;

        /// <summary>
        /// Color mode for the application (removed for now)
        /// </summary>
        private string colorMode;

        public event EventHandler ChangedWebhookEvent;
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

            bot.BadAuthentificationTokenCatched += new EventHandler(OnBadToken);

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
                SelectedMappool = MappoolManager.Mappools.FirstOrDefault(x => x.Name == pool);

            MappoolManager.ChangeEvent += e_ChangeEvent(); 

            LoadBot();
        }

        private EventHandler<EventArgs> e_ChangeEvent()
        {
            NotifyOfPropertyChange(() => Mappools);
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

                    bot.Username = bot.Username.Trim().Replace(' ', '_');
                    botpublic.Username = bot.Username;

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

        /// <summary>
        /// The color mode property (not used anymore)
        /// </summary>
        public List<string> ColorMode
        {
            get
            {
                return new List<string> { "BaseLight", "BaseDark" };
            }
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
                return bot.IsConnected ? Utils.Properties.Resources.Button_Disconnect : Utils.Properties.Resources.Button_Disconnect;
            }
        }

        /// <summary>
        /// The list of allowed BO number property
        /// </summary>
        public List<string> Modes
        {
            get
            {
                return new List<string> { "3", "5", "7", "9", "11", "13", "15" };
            }
        }

        /// <summary>
        /// The GameModes property
        /// </summary>
        public List<OsuMode> GameModes
        {
            get
            {
                return new List<OsuMode> { OsuMode.Standard, OsuMode.Taiko, OsuMode.CTB, OsuMode.Mania };
            }
        }

        public List<string> Languages
        {
            get
            {
                return new List<string> { "en-US", "de-DE" };
            }
        }

        public string SelectedLanguage
        {
            get
            {
                return cache.Get("language", "en-US");
            }
            set
            {
                if (cache.Get("language", "en-US") != value)
                {
                    cache["language"] = value;
                    NotifyOfPropertyChange(() => SelectedLanguage);
                    Dialog.ShowDialog("Language", "Please restart the app to apply language changes.");
                }
            }
        }

        /// <summary>
        /// Mappools list property
        /// </summary>
        public List<Osu.Scores.Mappool> Mappools
        {
            get
            {
                return MappoolManager.Mappools.ToList();
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

        /// <summary>
        /// SelectedGameMode property
        /// </summary>
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

        /// <summary>
        /// Selected mappool property
        /// </summary>
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

        /// <summary>
        /// Default webhook URL
        /// </summary>
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

        /// <summary>
        /// Admin webhook URL
        /// </summary>
        public string AdminWebhook
        {
            get
            {
                return InfosHelper.UserDataInfos.WebhookAdmins;
            }
            set
            {
                InfosHelper.UserDataInfos.WebhookAdmins = value;
                if (DiscordClient.GetInstance().IsEnabled != DiscordClient.GetInstance().UpdateEnabled())
                {
                    ChangedWebhookEvent?.Invoke(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Timer interval property
        /// </summary>
        public int Timer
        {
            get { return InfosHelper.TourneyInfos.Timer; }
            set
            {
                InfosHelper.TourneyInfos.Timer = Math.Abs(value);
                NotifyOfPropertyChange(() => Timer);
            }
        }

        /// <summary>
        /// Timer interval property
        /// </summary>
        public int SecondBanCount
        {
            get { return InfosHelper.TourneyInfos.SecondBanCount; }
            set
            {
                InfosHelper.TourneyInfos.SecondBanCount = Math.Abs(value);
                NotifyOfPropertyChange(() => SecondBanCount);
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
        /// Check if the bot is connected
        /// </summary>
        public bool IsConnected
        {
            get { return bot.IsConnected; }
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

        public void IrcPasswordHelp()
        {
            System.Diagnostics.Process.Start("https://osu.ppy.sh/p/irc");
        }

        public void ApiKeyHelp()
        {
            System.Diagnostics.Process.Start("https://osu.ppy.sh/p/api");
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

            Execute.OnUIThread(() =>
            {
                Dialog.ShowDialog("Whoops!", "The irc token provided is not valid!");
            });
        }
        #endregion
    }
}
