using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using script_chan2.Discord;
using Serilog;
using System;
using System.Linq;

namespace script_chan2.GUI
{
    public class WebhooksViewModel : Screen, IHandle<string>
    {
        private ILogger localLog = Log.ForContext<WebhooksViewModel>();

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
            localLog.Information("open new webhook dialog");
            var model = new EditWebhookDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("save new webhook '{webhook}'", model.Name);
                var webhook = new Webhook()
                {
                    Name = model.Name,
                    URL = model.Url,
                    MatchCreated = model.MatchCreated,
                    BanRecap = model.BanRecap,
                    PickRecap = model.PickRecap,
                    GameRecap = model.GameRecap,
                    FooterText = model.FooterText,
                    FooterIcon = model.FooterIcon,
                    WinImage = model.WinImage,
                    Username = model.Username,
                    Avatar = model.Avatar,
                    AuthorIcon = model.AuthorIcon
                };
                await DiscordApi.SetWebhookChannel(webhook);
                webhook.Save();
                NotifyOfPropertyChange(() => WebhookViews);
            }
        }
        #endregion
    }
}
