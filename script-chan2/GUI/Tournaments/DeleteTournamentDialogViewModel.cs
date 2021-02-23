using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;

namespace script_chan2.GUI
{
    public class DeleteTournamentDialogViewModel : Screen
    {
        #region Constructor
        public DeleteTournamentDialogViewModel(Tournament tournament)
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

        public string Label
        {
            get { return string.Format(Properties.Resources.DeleteTournamentDialogView_LabelText, Name); }
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
