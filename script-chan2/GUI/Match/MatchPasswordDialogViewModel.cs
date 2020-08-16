using Caliburn.Micro;
using MaterialDesignThemes.Wpf;

namespace script_chan2.GUI
{
    public class MatchPasswordDialogViewModel : Screen
    {
        #region Constructor
        public MatchPasswordDialogViewModel()
        {
            Password = "";
        }
        #endregion

        #region Properties
        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                if (value != password)
                {
                    password = value;
                    NotifyOfPropertyChange(() => Password);
                }
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
