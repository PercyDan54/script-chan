using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;

namespace script_chan2.GUI
{
    public class DeleteTeamDialogViewModel : Screen
    {
        #region Constructor
        public DeleteTeamDialogViewModel(Team team)
        {
            this.team = team;
        }
        #endregion

        #region Properties
        private Team team;

        public string Name
        {
            get { return team.Name; }
        }

        public string Label
        {
            get { return string.Format(Properties.Resources.DeleteTeamDialogView_LabelText, Name); }
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
