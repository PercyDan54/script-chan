using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class ModMultiplierPresetListItemViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<ModMultiplierPresetListItemViewModel>();

        #region Constructor
        public ModMultiplierPresetListItemViewModel(ModMultiplierPreset modMultiplierPreset)
        {
            this.modMultiplierPreset = modMultiplierPreset;
            Events.Aggregator.Subscribe(this);
        }
        #endregion

        #region Properties
        private ModMultiplierPreset modMultiplierPreset;

        public string Name => $"{modMultiplierPreset.Name}";

        private bool hover = false;
        public SolidColorBrush Background
        {
            get
            {
                if (hover)
                    return Brushes.LightGray;
                return Brushes.Transparent;
            }
        }

        public SolidColorBrush Foreground
        {
            get
            {
                if (hover)
                    return Brushes.Black;
                return Brushes.White;
            }
        }
        #endregion

        #region Actions
        public async void Edit()
        {
            localLog.Information("edit dialog of modMultiplierPreset '{modMultiplierPreset}' open", modMultiplierPreset.Name);
            var model = new EditModMultiplierPresetDialogViewModel(modMultiplierPreset.Id);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("save modMultiplierPreset '{modMultiplierPreset}'", modMultiplierPreset.Name);
                modMultiplierPreset.Name = model.Name;
                modMultiplierPreset.Save();
                NotifyOfPropertyChange(() => Name);
            }
        }

        public async void EditMaps()
        {
            localLog.Information("modMultiplierPreset list dialog of modMultiplierPreset '{modMultiplierPreset}' open", modMultiplierPreset.Name);
            var model = new ModMultipliersDialogViewModel(modMultiplierPreset);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            model.Activate();
            await DialogHost.Show(view, "MainDialogHost");
            model.Deactivate();
        }

        public async void Delete()
        {
            localLog.Information("delete dialog of modMultiplierPreset '{modMultiplierPreset}' open", modMultiplierPreset.Name);
            var model = new DeleteModMultiplierPresetDialogViewModel(modMultiplierPreset);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("delete modMultiplierPreset '{modMultiplierPreset}'", modMultiplierPreset.Name);
                modMultiplierPreset.Delete();
                Events.Aggregator.PublishOnUIThread("DeleteModMultiplierPreset");
            }
        }

        public void MouseEnter()
        {
            hover = true;
            NotifyOfPropertyChange(() => Background);
            NotifyOfPropertyChange(() => Foreground);
        }

        public void MouseLeave()
        {
            hover = false;
            NotifyOfPropertyChange(() => Background);
            NotifyOfPropertyChange(() => Foreground);
        }
        #endregion
    }
}
