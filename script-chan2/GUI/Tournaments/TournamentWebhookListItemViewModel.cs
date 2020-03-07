using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class TournamentWebhookListItemViewModel : Screen
    {
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

        public string Name
        {
            get { return webhook.Name; }
        }

        public bool Active
        {
            get { return tournament.Webhooks.Contains(webhook); }
            set
            {
                if (value)
                    tournament.AddWebhook(webhook);
                else
                    tournament.RemoveWebhook(webhook);
                NotifyOfPropertyChange(() => Active);
            }
        }
        #endregion
    }
}
