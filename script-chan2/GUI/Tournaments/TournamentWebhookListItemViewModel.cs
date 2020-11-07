using Caliburn.Micro;
using script_chan2.DataTypes;
using Serilog;

namespace script_chan2.GUI
{
    public class TournamentWebhookListItemViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<TournamentWebhookListItemViewModel>();

        #region Constructor
        public TournamentWebhookListItemViewModel(Tournament tournament, Webhook webhook)
        {
            this.tournament = tournament;
            this.webhook = webhook;
        }
        #endregion

        #region Properties
        private Tournament tournament;

        private Webhook webhook;

        public string WebhookName
        {
            get { return webhook.Name; }
        }

        public bool Active
        {
            get { return tournament.Webhooks.Contains(webhook); }
            set
            {
                if (value)
                {
                    localLog.Information("add webhook '{webhook}' to tournament '{tournament}'", webhook.Name, tournament.Name);
                    tournament.AddWebhook(webhook);
                }
                else
                {
                    localLog.Information("remove webhook '{webhook}' from tournament '{tournament}'", webhook.Name, tournament.Name);
                    tournament.RemoveWebhook(webhook);
                }
                NotifyOfPropertyChange(() => Active);
                Events.Aggregator.PublishOnUIThread("WebhookActiveToggle");
            }
        }
        #endregion
    }
}
