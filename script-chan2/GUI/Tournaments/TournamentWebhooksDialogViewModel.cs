using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;

namespace script_chan2.GUI
{
    public class TournamentWebhooksDialogViewModel : Screen
    {
        #region Constructor
        public TournamentWebhooksDialogViewModel(Tournament tournament)
        {
            this.tournament = tournament;
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
        #endregion

        #region Actions
        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
