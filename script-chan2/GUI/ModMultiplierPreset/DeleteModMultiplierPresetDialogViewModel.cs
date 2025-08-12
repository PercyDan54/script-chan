using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;

namespace script_chan2.GUI
{
    public class DeleteModMultiplierPresetDialogViewModel : Screen
    {
        #region Constructor
        public DeleteModMultiplierPresetDialogViewModel(ModMultiplierPreset modMultiplierPreset)
        {
            this.modMultiplierPreset = modMultiplierPreset;
        }
        #endregion

        #region Properties
        private ModMultiplierPreset modMultiplierPreset;

        public string Name
        {
            get { return modMultiplierPreset.Name; }
        }

        public string Label
        {
            get
            {
                return string.Format(Properties.Resources.DeleteMappoolDialogView_LabelText, Name);
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
