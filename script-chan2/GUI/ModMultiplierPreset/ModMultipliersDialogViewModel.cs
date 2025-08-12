using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System.Threading.Tasks;
using System.Windows;
using script_chan2.Enums;

namespace script_chan2.GUI
{
    public class ModMultipliersDialogViewModel : Screen, IHandle<string>
    {
        private ILogger localLog = Log.ForContext<ModMultipliersDialogViewModel>();

        #region Constructor
        public ModMultipliersDialogViewModel(ModMultiplierPreset modMultiplierPreset)
        {
            this.modMultiplierPreset = modMultiplierPreset;
            Events.Aggregator.Subscribe(this);
        }
        #endregion

        #region Events
        public void Handle(string message)
        {
            if (message == "DeleteModMultiplier" || message == "UpdateModMultiplier")
                NotifyOfPropertyChange(() => ModMultiplierPresetViews);
        }
        #endregion

        #region Properties
        private ModMultiplierPreset modMultiplierPreset;

        public BindableCollection<GameMods> ModsList { get; set; } = ModMultiplierPresetItemViewModel.ModsListCache;
        public GameMods SelectedMod { get; set; } = GameMods.Easy;

        public BindableCollection<ModMultiplierPresetItemViewModel> ModMultiplierPresetViews
        {
            get
            {
                var list = new BindableCollection<ModMultiplierPresetItemViewModel>();
                foreach (var mod in modMultiplierPreset.Multipliers)
                {
                    list.Add(new ModMultiplierPresetItemViewModel(modMultiplierPreset, mod));
                }

                return list;
            }
        }

        #endregion

        #region Actions

        public void AddModMultiplier()
        {
            var item = new ModMultiplier(SelectedMod, 2);

            if (!modMultiplierPreset.Multipliers.Contains(item))
            {
                modMultiplierPreset.Multipliers.Add(item);
                NotifyOfPropertyChange(() => ModMultiplierPresetViews);
            }
        }

        public void Activate()
        {
            localLog.Information("list dialog of modMultiplierPreset '{modMultiplierPreset}' open", modMultiplierPreset.Name);
        }

        public void Deactivate()
        {
            localLog.Information("list dialog of modMultiplierPreset '{modMultiplierPreset}' close", modMultiplierPreset.Name);
            modMultiplierPreset.Save();
        }

        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
