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
        public BindableCollection<WebhookListItemViewModel> WebhookViews
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
                NotifyOfPropertyChange(() => WebhookViews);
        }
        #endregion

        #region Actions
        public async void OpenNewWebhookDialog()
        {
            var model = new EditWebhookDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view));

            if (result)
            {
                var webhook = new Webhook()
                {
                    Name = model.Name,
                    URL = model.Url,
                    MatchCreated = model.MatchCreated,
                    BanRecap = model.BanRecap,
                    PickRecap = model.PickRecap,
                    GameRecap = model.GameRecap
                };
                webhook.Save();
                NotifyOfPropertyChange(() => WebhookViews);
            }
        }
        #endregion
    }
}
