using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Linq;

namespace script_chan2.GUI
{
    public class CustomCommandsViewModel : Screen, IHandle<string>
    {
        private ILogger localLog = Log.ForContext<CustomCommandsViewModel>();

        #region Custom command list
        public BindableCollection<CustomCommandListItemViewModel> CustomCommandViews
        {
            get
            {
                var list = new BindableCollection<CustomCommandListItemViewModel>();
                foreach (var customCommand in Database.Database.CustomCommands)
                {
                    if (FilterTournament != null && customCommand.Tournament != FilterTournament)
                        continue;
                    list.Add(new CustomCommandListItemViewModel(customCommand));
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
            if (message == "DeleteCustomCommand")
                NotifyOfPropertyChange(() => CustomCommandViews);
            else if (message == "UpdateDefaultTournament")
                NotifyOfPropertyChange(() => FilterTournament);
            else if (message == "AddCustomCommand")
                NotifyOfPropertyChange(() => CustomCommandViews);
        }
        #endregion

        #region Filter
        public BindableCollection<Tournament> Tournaments
        {
            get
            {
                var list = new BindableCollection<Tournament>();
                foreach (var tournament in Database.Database.Tournaments.OrderBy(x => x.Name))
                    list.Add(tournament);
                return list;
            }
        }

        public Tournament FilterTournament
        {
            get { return Settings.DefaultTournament; }
            set
            {
                if (value != Settings.DefaultTournament)
                {
                    localLog.Information("set tournament filter");
                    Settings.DefaultTournament = value;
                    NotifyOfPropertyChange(() => FilterTournament);
                    NotifyOfPropertyChange(() => CustomCommandViews);
                }
            }
        }
        #endregion

        #region Actions
        public async void OpenNewCustomCommandDialog()
        {
            localLog.Information("new custom command dialog open");
            var model = new EditCustomCommandDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("save new custom command '{name}'", model.Name);
                var customCommand = new CustomCommand()
                {
                    Name = model.Name,
                    Tournament = model.Tournament,
                    Command = model.Command
                };
                customCommand.Save();
                NotifyOfPropertyChange(() => CustomCommandViews);
            }
        }
        #endregion
    }
}
