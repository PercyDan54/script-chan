using Caliburn.Micro;
using MaterialDesignThemes.Wpf;

namespace script_chan2.GUI
{
    public class MatchSyncScoreDialogViewModel : Screen
    {
        #region Properties
        private int skipRound;
        public int SkipRound
        {
            get { return skipRound; }
            set
            {
                if (value != skipRound)
                {
                    skipRound = value;
                    NotifyOfPropertyChange(() => SkipRound);
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
