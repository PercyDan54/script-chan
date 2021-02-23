using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;

namespace script_chan2.GUI
{
    public class DeleteMatchDialogViewModel : Screen
    {
        #region Constructor
        public DeleteMatchDialogViewModel(Match match)
        {
            this.match = match;
        }
        #endregion

        #region Properties
        private Match match;

        public string Name
        {
            get { return match.Name; }
        }

        public string Label
        {
            get { return string.Format(Properties.Resources.DeleteMatchDialogView_LabelText, Name); }
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
