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
                {
                    Log.Information("GUI add webhook '{webhook}' to tournament '{tournament}'", webhook.Name, tournament.Name);
                    tournament.AddWebhook(webhook);
                }
                else
                {
                    Log.Information("GUI remove webhook '{webhook}' from tournament '{tournament}'", webhook.Name, tournament.Name);
                    tournament.RemoveWebhook(webhook);
                }
                NotifyOfPropertyChange(() => Active);
            }
        }
        #endregion
    }
}
