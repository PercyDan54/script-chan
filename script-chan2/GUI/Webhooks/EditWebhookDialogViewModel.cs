using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            }
            else
            {
                Name = "";
                Url = "";
                MatchCreated = true;
                BanRecap = true;
                PickRecap = true;
                GameRecap = true;
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
