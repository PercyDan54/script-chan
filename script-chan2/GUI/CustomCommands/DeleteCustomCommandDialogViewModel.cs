using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;

namespace script_chan2.GUI
{
    public class DeleteCustomCommandDialogViewModel : Screen
    {
        #region Constructor
        public DeleteCustomCommandDialogViewModel(CustomCommand customCommand)
        {
            this.customCommand = customCommand;
        }
        #endregion

        #region Properties
        private CustomCommand customCommand;

        public string Name
        {
            get { return customCommand.Name; }
        }

        public string Label
        {
            get
            {
                return string.Format(Properties.Resources.DeleteCustomCommandDialogViewModel_LabelText, Name);
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
