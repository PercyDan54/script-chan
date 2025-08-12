using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using System.Linq;

namespace script_chan2.GUI
{
    public class EditModMultiplierPresetDialogViewModel : Screen
    {
        #region Constructor
        public EditModMultiplierPresetDialogViewModel(int id = 0)
        {
            this.id = id;
            if (id > 0)
            {
                var modMultiplierPreset = Database.Database.ModMultiplierPresets.First(x => x.Id == id);
                Name = modMultiplierPreset.Name;
            }
            else
            {
                Name = string.Empty;
            }
        }
        #endregion

        #region Properties
        private int id;

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    NotifyOfPropertyChange(() => Name);
                    NotifyOfPropertyChange(() => SaveEnabled);
                }
            }
        }

        public bool SaveEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return false;
                if (Database.Database.ModMultiplierPresets.Any(x => x.Name == Name && x.Id != id))
                    return false;
                return true;
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
