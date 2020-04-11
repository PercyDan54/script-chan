using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class CustomCommandListItemViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<CustomCommandListItemViewModel>();

        #region Constructor
        public CustomCommandListItemViewModel(CustomCommand customCommand)
        {
            this.customCommand = customCommand;
            Events.Aggregator.Subscribe(this);
        }
        #endregion

        #region Properties
        private CustomCommand customCommand;

        public string Name
        {
            get
            {
                if (customCommand.Tournament != null)
                    return $"{customCommand.Name} ({customCommand.Tournament.Name})";
                return customCommand.Name;
            }
        }

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
        #endregion

        #region Actions
        public async void Edit()
        {
            localLog.Information("edit dialog of custom command '{name}' open", customCommand.Name);
            var model = new EditCustomCommandDialogViewModel(customCommand.Id);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("save custom command '{name}'", customCommand.Name);
                customCommand.Name = model.Name;
                customCommand.Command = model.Command;
                customCommand.Tournament = model.Tournament;
                customCommand.Save();
                NotifyOfPropertyChange(() => Name);
            }
        }

        public async void Delete()
        {
            localLog.Information("delete dialog of custom command '{name}' open", customCommand.Name);
            var model = new DeleteCustomCommandDialogViewModel(customCommand);
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("delete custom command '{name}'", customCommand.Name);
                customCommand.Delete();
                Events.Aggregator.PublishOnUIThread("DeleteCustomCommand");
            }
        }

        public void MouseEnter()
        {
            hover = true;
            NotifyOfPropertyChange(() => Background);
        }

        public void MouseLeave()
        {
            hover = false;
            NotifyOfPropertyChange(() => Background);
        }
        #endregion
    }
}
