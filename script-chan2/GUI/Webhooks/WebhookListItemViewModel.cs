using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class WebhookListItemViewModel : PropertyChangedBase
    {
        public WebhookListItemViewModel(Webhook webhook)
        {
            this.webhook = webhook;
        }

        private Webhook webhook;

        public string Name
        {
            get { return webhook.Name; }
        }

        private string editName;
        public string EditName
        {
            get { return editName; }
            set
            {
                if (value != editName)
                {
                    editName = value;
                    NotifyOfPropertyChange(() => EditName);
                    NotifyOfPropertyChange(() => EditWebhookSaveEnabled);
                }
            }
        }

        private string editUrl;
        public string EditUrl
        {
            get { return editUrl; }
            set
            {
                if (value != editUrl)
                {
                    editUrl = value;
                    NotifyOfPropertyChange(() => EditUrl);
                }
            }
        }

        public bool EditWebhookSaveEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(editName))
                    return false;
                if (Database.Database.Webhooks.Any(x => x.Name == editName && x.Id != webhook.Id))
                    return false;
                if (string.IsNullOrEmpty(editUrl))
                    return false;
                return true;
            }
        }

        public void Edit()
        {
            EditName = webhook.Name;
            EditUrl = webhook.URL;
        }

        public void Save()
        {
            if (EditWebhookSaveEnabled)
            {
                webhook.Name = EditName;
                webhook.URL = EditUrl;
                webhook.Save();
                NotifyOfPropertyChange(() => Name);
            }
        }

        public void Delete()
        {
            webhook.Delete();
            Events.Aggregator.PublishOnUIThread("DeleteWebhook");
        }
    }
}
