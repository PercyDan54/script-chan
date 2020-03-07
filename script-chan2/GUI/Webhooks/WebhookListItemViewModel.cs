using Caliburn.Micro;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class WebhookListItemViewModel : PropertyChangedBase
    {
        #region Constructor
        public WebhookListItemViewModel(Webhook webhook)
        {
            this.webhook = webhook;
        }
        #endregion

        #region Properties
        private Webhook webhook;

        public string Name
        {
            get { return webhook.Name; }
        }
        #endregion

        #region Edit webhook dialog
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
            Log.Information("GUI edit webhook dialog open");
            EditName = webhook.Name;
            EditUrl = webhook.URL;
        }

        public void Save()
        {
            if (EditWebhookSaveEnabled)
            {
                Log.Information("GUI edit webhook save");
                webhook.Name = EditName;
                webhook.URL = EditUrl;
                webhook.Save();
                NotifyOfPropertyChange(() => Name);
            }
        }
        #endregion

        #region Actions
        public void Delete()
        {
            Log.Information("GUI delete webhook");
            webhook.Delete();
            Events.Aggregator.PublishOnUIThread("DeleteWebhook");
        }
        #endregion
    }
}
