using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        #endregion

        #region Actions
        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
