using Caliburn.Micro;
using MaterialDesignThemes.Wpf;

namespace script_chan2.GUI
{
    public class ExportSuccessDialogViewModel : Screen
    {
        #region Constructor
        public ExportSuccessDialogViewModel(string title)
        {
            Title = title;
        }
        #endregion

        #region Properties
        public string Title { get; set; }
        #endregion

        #region Actions
        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
