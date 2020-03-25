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
        #endregion

        #region Actions
        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
