using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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

        private bool hover = false;
        public SolidColorBrush Background
        {
            get
            {
                if (hover)
                    return Brushes.LightGray;
                return Brushes.Transparent;
            }
        }
        #endregion

        #region Actions
        public async void Edit()
        {
            Log.Information("WebhookListItemViewModel: webhook '{name}' edit dialog open", webhook.Name);
            var model = new EditWebhookDialogViewModel(webhook.Id);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view));

            if (result)
            {
                webhook.Name = model.Name;
                webhook.URL = model.Url;
                webhook.Save();
                NotifyOfPropertyChange(() => Name);
            }
        }

        public async void Delete()
        {
            Log.Information("WebhookListItemViewModel: webhook '{name}' delete dialog open", webhook.Name);
            var model = new DeleteWebhookDialogViewModel(webhook);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view));

            if (result)
            {
                webhook.Delete();
                Events.Aggregator.PublishOnUIThread("DeleteWebhook");
            }
        }

        public void MouseEnter()
        {
            hover = true;
            NotifyOfPropertyChange(() => Background);
        }

        public void MouseLeave()
        {
            hover = false;
            NotifyOfPropertyChange(() => Background);
        }
        #endregion
    }
}
