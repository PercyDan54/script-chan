using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using script_chan2.Enums;

namespace script_chan2.GUI
{
    public class ModMultiplierPresetViewModel : Screen, IHandle<string>
    {
        private ILogger localLog = Log.ForContext<ModMultiplierPresetViewModel>();

        #region ModMultiplierPreset list
        public BindableCollection<ModMultiplierPresetListItemViewModel> ModMultiplierPresetViews
        {
            get
            {
                var list = new BindableCollection<ModMultiplierPresetListItemViewModel>();
                foreach (var modMultiplierPresets in Database.Database.ModMultiplierPresets)
                {
                    list.Add(new ModMultiplierPresetListItemViewModel(modMultiplierPresets));
                }
                return list;
            }
        }
        #endregion

        #region Constructor
        protected override void OnActivate()
        {
            Events.Aggregator.Subscribe(this);
        }
        #endregion

        #region Events
        public void Handle(string message)
        {
            if (message == "DeleteModMultiplierPreset")
                NotifyOfPropertyChange(() => ModMultiplierPresetViews);
            else if (message == "AddModMultiplierPreset")
                NotifyOfPropertyChange(() => ModMultiplierPresetViews);
        }
        #endregion

        #region Properties

        #endregion

        #region Actions
        public async void OpenNewModMultiplierPresetDialog()
        {
            localLog.Information("new modMultiplierPreset dialog open");
            var model = new EditModMultiplierPresetDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("save new modMultiplierPreset '{modMultiplierPreset}'", model.Name);
                var modMultiplierPreset = new ModMultiplierPreset
                {
                    Name = model.Name,
                    Multipliers = new List<ModMultiplier>
                    {
                        new ModMultiplier(GameMods.Easy, 2)
                    }
                };
                modMultiplierPreset.Save();
                NotifyOfPropertyChange(() => ModMultiplierPresetViews);
            }
        }

        public async void OpenDeleteAllOpenNewModMultiplierPresetsDialog()
        {
            localLog.Information("open delete all modMultiplierPreset dialog");
            var model = new DeleteAllModMultiplierPresetsDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("delete all modMultiplierPreset");
                var modMultiplierPresets = new List<ModMultiplierPreset>();
                foreach (var modMultiplierPreset in Database.Database.ModMultiplierPresets)
                {
                    modMultiplierPresets.Add(modMultiplierPreset);
                }
                foreach (var modMultiplierPreset in modMultiplierPresets)
                {
                    modMultiplierPreset.Delete();
                }
                NotifyOfPropertyChange(() => ModMultiplierPresetViews);
            }
        }
        #endregion
    }
}
