using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class WebhooksViewModel : Screen, IHandle<string>
    {
        public BindableCollection<Webhook> Webhooks { get; set; }

        public BindableCollection<WebhookListItemViewModel> WebhooksViews
        {
            get
            {
                var list = new BindableCollection<WebhookListItemViewModel>();
                foreach (var webhook in Webhooks)
                    list.Add(new WebhookListItemViewModel(webhook));
                return list;
            }
        }

        protected override void OnActivate()
        {
            Reload();
            Events.Aggregator.Subscribe(this);
        }

        public void Reload()
        {
            Webhooks = new BindableCollection<Webhook>();
            foreach (var webhook in Database.Database.Webhooks.OrderBy(x => x.Name))
            {
                Webhooks.Add(webhook);
            }
            NotifyOfPropertyChange(() => Webhooks);
            NotifyOfPropertyChange(() => WebhooksViews);
        }

        public void Handle(string message)
        {
            if (message.ToString() == "DeleteWebhook")
                Reload();
        }

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
                if (Webhooks.Any(x => x.Name == newWebhookName))
                    return false;
                if (string.IsNullOrEmpty(newWebhookUrl))
                    return false;
                return true;
            }
        }

        public void NewWebhookDialogOpened()
        {
            NewWebhookName = "";
            NewWebhookUrl = "";
        }

        public void NewWebhookDialogClosed()
        {
            var webhook = new Webhook(NewWebhookName, NewWebhookUrl);
            webhook.Save();
            Reload();
        }
    }
}
