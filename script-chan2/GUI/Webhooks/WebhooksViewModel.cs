using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class WebhooksViewModel : Screen, IHandle<string>
    {
        #region Webhooks list
        public BindableCollection<WebhookListItemViewModel> WebhooksViews
        {
            get
            {
                var list = new BindableCollection<WebhookListItemViewModel>();
                foreach (var webhook in Database.Database.Webhooks.OrderBy(x => x.Name))
                    list.Add(new WebhookListItemViewModel(webhook));
                return list;
            }
        }
        #endregion

        #region Constructor
        protected override void OnActivate()
        {
            Events.Aggregator.Subscribe(this);
        }
        #endregion

        #region Events
        public void Handle(string message)
        {
            if (message.ToString() == "DeleteWebhook")
                NotifyOfPropertyChange(() => WebhooksViews);
        }
        #endregion

        #region New webhook dialog
        private string newWebhookName;
        public string NewWebhookName
        {
            get { return newWebhookName; }
            set
            {
                if (value != newWebhookName)
                {
                    newWebhookName = value;
                    NotifyOfPropertyChange(() => NewWebhookName);
                    NotifyOfPropertyChange(() => NewWebhookSaveEnabled);
                }
            }
        }

        private string newWebhookUrl;
        public string NewWebhookUrl
        {
            get { return newWebhookUrl; }
            set
            {
                if (value != newWebhookUrl)
                {
                    newWebhookUrl = value;
                    NotifyOfPropertyChange(() => NewWebhookUrl);
                    NotifyOfPropertyChange(() => NewWebhookSaveEnabled);
                }
            }
        }

        public bool NewWebhookSaveEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(newWebhookName))
                    return false;
                if (Database.Database.Webhooks.Any(x => x.Name == newWebhookName))
                    return false;
                if (string.IsNullOrEmpty(newWebhookUrl))
                    return false;
                return true;
            }
        }

        public void NewWebhookDialogOpened()
        {
            Log.Information("GUI new webhook dialog open");
            NewWebhookName = "";
            NewWebhookUrl = "";
        }

        public void NewWebhookDialogClosed()
        {
            Log.Information("GUI new webhook '{name}' save", NewWebhookName);
            var webhook = new Webhook(NewWebhookName, NewWebhookUrl);
            webhook.Save();
            NotifyOfPropertyChange(() => WebhooksViews);
        }
        #endregion
    }
}
