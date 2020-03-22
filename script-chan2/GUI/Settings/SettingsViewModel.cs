using Caliburn.Micro;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace script_chan2.GUI
{
    public class SettingsViewModel : Screen
    {
        #region Properties
        private bool dirty;
        public bool Dirty
        {
            get { return dirty; }
            set
            {
                if (value != dirty)
                {
                    dirty = value;
                    NotifyOfPropertyChange(() => Dirty);
                }
            }
        }

        public BindableCollection<string> Languages
        {
            get { return new BindableCollection<string>(new string[] { "en-US", "de-DE" }); }
        }

        private string lang;
        public string SelectedLanguage
        {
            get { return lang; }
            set
            {
                if (value != lang)
                {
                    lang = value;
                    NotifyOfPropertyChange(() => SelectedLanguage);
                    Dirty = true;
                }
            }
        }

        private string apiKey;
        public string ApiKey
        {
            get { return apiKey; }
            set
            {
                if (value != apiKey)
                {
                    apiKey = value;
                    NotifyOfPropertyChange(() => ApiKey);
                    Dirty = true;
                }
            }
        }

        public void OpenApiKeyPage()
        {
            Log.Information("SettingsViewModel: open api key page");
            System.Diagnostics.Process.Start("https://osu.ppy.sh/p/api");
        }

        public Visibility ApiWorks
        {
            get
            {
                if (apiStatus)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility ApiError
        {
            get
            {
                if (!apiStatus)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        private bool apiStatus;

        public void CheckApiKey()
        {
            Log.Information("SettingsViewModel: check api key");
            apiStatus = OsuApi.OsuApi.CheckApiKey(apiKey);
            NotifyOfPropertyChange(() => ApiWorks);
            NotifyOfPropertyChange(() => ApiError);
        }

        private string ircUsername;
        public string IrcUsername
        {
            get { return ircUsername; }
            set
            {
                if (value != ircUsername)
                {
                    ircUsername = value;
                    NotifyOfPropertyChange(() => IrcUsername);
                    Dirty = true;
                }
            }
        }

        private string ircPassword;
        public string IrcPassword
        {
            get { return ircPassword; }
            set
            {
                if (value != ircPassword)
                {
                    ircPassword = value;
                    NotifyOfPropertyChange(() => IrcPassword);
                    Dirty = true;
                }
            }
        }

        public void OpenIrcPage()
        {
            Log.Information("SettingsViewModel: open irc credentials page");
            System.Diagnostics.Process.Start("https://osu.ppy.sh/p/irc");
        }

        public void CheckIrc()
        {
            Log.Information("SettingsViewModel: check irc");
            NotifyOfPropertyChange(() => IrcIsConnected);
            NotifyOfPropertyChange(() => IrcIsDisconnected);
        }

        public Visibility IrcIsConnected
        {
            get
            {
                if (OsuIrc.OsuIrc.IsConnected)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility IrcIsDisconnected
        {
            get
            {
                if (!OsuIrc.OsuIrc.IsConnected)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        private int ircTimeout;
        public int IrcTimeout
        {
            get { return ircTimeout; }
            set
            {
                if (value != ircTimeout)
                {
                    ircTimeout = value;
                    NotifyOfPropertyChange(() => IrcTimeout);
                    Dirty = true;
                }
            }
        }

        public Visibility PrivateIrcVisible
        {
            get
            {
                if (!string.IsNullOrEmpty(Settings.IrcIpPrivate))
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        private bool enablePrivateIrc;
        public bool EnablePrivateIrc
        {
            get { return enablePrivateIrc; }
            set
            {
                if (value != enablePrivateIrc)
                {
                    enablePrivateIrc = value;
                    NotifyOfPropertyChange(() => EnablePrivateIrc);
                    Dirty = true;
                }
            }
        }

        private int defaultTimerCommand;
        public int DefaultTimerCommand
        {
            get { return defaultTimerCommand; }
            set
            {
                if (value != defaultTimerCommand)
                {
                    defaultTimerCommand = value;
                    NotifyOfPropertyChange(() => DefaultTimerCommand);
                    Dirty = true;
                }
            }
        }

        private int defaultTimerAfterGame;
        public int DefaultTimerAfterGame
        {
            get { return defaultTimerAfterGame; }
            set
            {
                if (value != defaultTimerAfterGame)
                {
                    defaultTimerAfterGame = value;
                    NotifyOfPropertyChange(() => DefaultTimerAfterGame);
                    Dirty = true;
                }
            }
        }

        private int defaultTimerAfterPick;
        public int DefaultTimerAfterPick
        {
            get { return defaultTimerAfterPick; }
            set
            {
                if (value != defaultTimerAfterPick)
                {
                    defaultTimerAfterPick = value;
                    NotifyOfPropertyChange(() => DefaultTimerAfterPick);
                    Dirty = true;
                }
            }
        }
        #endregion

        #region Constructor
        protected override void OnActivate()
        {
            Discard();
            CheckApiKey();
        }
        #endregion

        #region Actions
        public void Save()
        {
            Log.Information("SettingsViewModel: settings save changes");
            Settings.Lang = lang;
            Settings.ApiKey = apiKey;
            Settings.IrcUsername = ircUsername;
            Settings.IrcPassword = ircPassword;
            Settings.IrcTimeout = ircTimeout;
            Settings.EnablePrivateIrc = enablePrivateIrc;
            Settings.DefaultTimerCommand = defaultTimerCommand;
            Settings.DefaultTimerAfterGame = defaultTimerAfterGame;
            Settings.DefaultTimerAfterPick = defaultTimerAfterPick;
            Dirty = false;
        }

        public void Discard()
        {
            Log.Information("SettingsViewModel: settings discard changes");
            SelectedLanguage = Settings.Lang;
            ApiKey = Settings.ApiKey;
            IrcUsername = Settings.IrcUsername;
            IrcPassword = Settings.IrcPassword;
            IrcTimeout = Settings.IrcTimeout;
            EnablePrivateIrc = Settings.EnablePrivateIrc;
            DefaultTimerCommand = Settings.DefaultTimerCommand;
            DefaultTimerAfterGame = Settings.DefaultTimerAfterGame;
            DefaultTimerAfterPick = Settings.DefaultTimerAfterPick;
            Dirty = false;
        }
        #endregion
    }
}
