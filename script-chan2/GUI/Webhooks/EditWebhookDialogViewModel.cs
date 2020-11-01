using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using System.Linq;

namespace script_chan2.GUI
{
    public class EditWebhookDialogViewModel : Screen
    {
        #region Constructor
        public EditWebhookDialogViewModel(int id = 0)
        {
            this.id = id;
            if (id > 0)
            {
                var webhook = Database.Database.Webhooks.First(x => x.Id == id);
                Name = webhook.Name;
                Url = webhook.URL;
                MatchCreated = webhook.MatchCreated;
                BanRecap = webhook.BanRecap;
                PickRecap = webhook.PickRecap;
                GameRecap = webhook.GameRecap;
                FooterText = webhook.FooterText;
                FooterIcon = webhook.FooterIcon;
                WinImage = webhook.WinImage;
                Username = webhook.Username;
                Avatar = webhook.Avatar;
            }
            else
            {
                Name = "";
                Url = "";
                MatchCreated = true;
                BanRecap = true;
                PickRecap = true;
                GameRecap = true;
                FooterText = "Woah! So cool!";
                FooterIcon = "https://cdn.frankerfacez.com/emoticon/243789/4";
                WinImage = "https://78.media.tumblr.com/b94193615145d12bfb64aa77b677269e/tumblr_njzqukOpBP1ti1gm1o1_500.gif";
                Username = "Script-chan";
                Avatar = "https://cdn.discordapp.com/attachments/130304896581763072/400723356283961354/d366ce5fdd90f4e4471da04db380c378.png";
            }
        }
        #endregion

        #region Properties
        private int id;

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    NotifyOfPropertyChange(() => Name);
                    NotifyOfPropertyChange(() => SaveEnabled);
                }
            }
        }

        private string url;
        public string Url
        {
            get { return url; }
            set
            {
                if (value != url)
                {
                    url = value;
                    NotifyOfPropertyChange(() => Url);
                    NotifyOfPropertyChange(() => SaveEnabled);
                }
            }
        }

        private bool matchCreated;
        public bool MatchCreated
        {
            get { return matchCreated; }
            set
            {
                if (value != matchCreated)
                {
                    matchCreated = value;
                    NotifyOfPropertyChange(() => MatchCreated);
                }
            }
        }

        private bool banRecap;
        public bool BanRecap
        {
            get { return banRecap; }
            set
            {
                if (value != banRecap)
                {
                    banRecap = value;
                    NotifyOfPropertyChange(() => BanRecap);
                }
            }
        }

        private bool pickRecap;
        public bool PickRecap
        {
            get { return pickRecap; }
            set
            {
                if (value != pickRecap)
                {
                    pickRecap = value;
                    NotifyOfPropertyChange(() => PickRecap);
                }
            }
        }

        private bool gameRecap;
        public bool GameRecap
        {
            get { return gameRecap; }
            set
            {
                if (value != gameRecap)
                {
                    gameRecap = value;
                    NotifyOfPropertyChange(() => GameRecap);
                }
            }
        }

        private string footerText;
        public string FooterText
        {
            get { return footerText; }
            set
            {
                if (value != footerText)
                {
                    footerText = value;
                    NotifyOfPropertyChange(() => FooterText);
                }
            }
        }

        private string footerIcon;
        public string FooterIcon
        {
            get { return footerIcon; }
            set
            {
                if (value != footerIcon)
                {
                    footerIcon = value;
                    NotifyOfPropertyChange(() => FooterIcon);
                }
            }
        }

        private string winImage;
        public string WinImage
        {
            get { return winImage; }
            set
            {
                if (value != winImage)
                {
                    winImage = value;
                    NotifyOfPropertyChange(() => WinImage);
                }
            }
        }

        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                if (value != username)
                {
                    username = value;
                    NotifyOfPropertyChange(() => Username);
                }
            }
        }

        private string avatar;
        public string Avatar
        {
            get { return avatar; }
            set
            {
                if (value != avatar)
                {
                    avatar = value;
                    NotifyOfPropertyChange(() => Avatar);
                }
            }
        }

        public bool SaveEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return false;
                if (Database.Database.Webhooks.Any(x => x.Name == Name && x.Id != id))
                    return false;
                if (string.IsNullOrEmpty(Url))
                    return false;
                return true;
            }
        }
        #endregion

        #region Actions
        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
