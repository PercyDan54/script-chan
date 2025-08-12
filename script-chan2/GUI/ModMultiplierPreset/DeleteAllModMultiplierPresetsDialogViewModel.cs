using Caliburn.Micro;
using MaterialDesignThemes.Wpf;

namespace script_chan2.GUI
{
    class DeleteAllModMultiplierPresetsDialogViewModel : Screen
    {
        #region Actions
        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
