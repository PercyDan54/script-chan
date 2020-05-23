using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;

namespace script_chan2.GUI
{
    public class DeleteMappoolDialogViewModel : Screen
    {
        #region Constructor
        public DeleteMappoolDialogViewModel(Mappool mappool)
        {
            this.mappool = mappool;
        }
        #endregion

        #region Properties
        private Mappool mappool;

        public string Name
        {
            get { return mappool.Name; }
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
