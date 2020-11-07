using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using System.Linq;
using System.Windows;

namespace script_chan2.GUI
{
    public class TournamentWebhooksDialogViewModel : Screen, IHandle<string>
    {
        #region Constructor
        public TournamentWebhooksDialogViewModel(Tournament tournament)
        {
            this.tournament = tournament;
            Events.Aggregator.Subscribe(this);
        }

        public void OnDeactivate()
        {
            Events.Aggregator.Unsubscribe(this);
        }
        #endregion

        #region Events
        public void Handle(string message)
        {
            if (message == "WebhookActiveToggle")
            {
                NotifyOfPropertyChange(() => DuplicateVisibility);
            }
        }
        #endregion

        #region Properties
        private Tournament tournament;

        public string Name
        {
            get { return tournament.Name; }
        }

        public BindableCollection<TournamentWebhookListItemViewModel> WebhookViews
        {
            get
            {
                var list = new BindableCollection<TournamentWebhookListItemViewModel>();
                foreach (var webhook in Database.Database.Webhooks)
                    list.Add(new TournamentWebhookListItemViewModel(tournament, webhook));
                return list;
            }
        }

        public Visibility DuplicateVisibility
        {
            get
            {
                if (tournament.Webhooks.Where(x => x.MatchCreated).GroupBy(x => x.Channel).Any(x => x.Count() > 1))
                    return Visibility.Visible;
                if (tournament.Webhooks.Where(x => x.BanRecap).GroupBy(x => x.Channel).Any(x => x.Count() > 1))
                    return Visibility.Visible;
                if (tournament.Webhooks.Where(x => x.PickRecap).GroupBy(x => x.Channel).Any(x => x.Count() > 1))
                    return Visibility.Visible;
                if (tournament.Webhooks.Where(x => x.GameRecap).GroupBy(x => x.Channel).Any(x => x.Count() > 1))
                    return Visibility.Visible;
                return Visibility.Collapsed;
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
