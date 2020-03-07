using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            System.Diagnostics.Process.Start("https://osu.ppy.sh/p/api");
        }

        private string apiStatus;
        public string ApiStatus
        {
            get { return apiStatus; }
            set
            {
                if (value != apiStatus)
                {
                    apiStatus = value;
                    NotifyOfPropertyChange(() => ApiStatus);
                }
            }
        }

        public void CheckApiKey()
        {
            var testResult = OsuApi.OsuApi.CheckApiKey(apiKey);
            if (testResult)
                ApiStatus = "API works!";
            else
                ApiStatus = "API does not work";
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
            System.Diagnostics.Process.Start("https://osu.ppy.sh/p/irc");
        }

        private string ircStatus;
        public string IrcStatus
        {
            get { return ircStatus; }
            set
            {
                if (value != ircStatus)
                {
                    ircStatus = value;
                    NotifyOfPropertyChange(() => IrcStatus);
                }
            }
        }

        public void CheckIrc()
        {
            throw new NotImplementedException();
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

        private int mpTimerDuration;
        public int MpTimerDuration
        {
            get { return mpTimerDuration; }
            set
            {
                if (value != mpTimerDuration)
                {
                    mpTimerDuration = value;
                    NotifyOfPropertyChange(() => MpTimerDuration);
                    Dirty = true;
                }
            }
        }
        #endregion

        #region Constructor
        protected override void OnActivate()
        {
            Discard();
        }
        #endregion

        #region Actions
        public void Save()
        {
            Settings.Lang = lang;
            Settings.ApiKey = apiKey;
            Settings.IrcUsername = ircUsername;
            Settings.IrcPassword = ircPassword;
            Settings.IrcTimeout = ircTimeout;
            Settings.MpTimerDuration = mpTimerDuration;
            Dirty = false;
        }

        public void Discard()
        {
            SelectedLanguage = Settings.Lang;
            ApiKey = Settings.ApiKey;
            IrcUsername = Settings.IrcUsername;
            IrcPassword = Settings.IrcPassword;
            IrcTimeout = Settings.IrcTimeout;
            MpTimerDuration = Settings.MpTimerDuration;
            Dirty = false;
        }
        #endregion
    }
}
