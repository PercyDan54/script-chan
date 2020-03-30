using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class MappoolsViewModel : Screen, IHandle<string>
    {
        private ILogger localLog = Log.ForContext<MappoolsViewModel>();

        #region Mappool list
        public BindableCollection<MappoolListItemViewModel> MappoolViews
        {
            get
            {
                var list = new BindableCollection<MappoolListItemViewModel>();
                foreach (var mappool in Database.Database.Mappools)
                {
                    if (Settings.DefaultTournament != null && mappool.Tournament != Settings.DefaultTournament)
                        continue;
                    list.Add(new MappoolListItemViewModel(mappool));
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
            if (message == "DeleteMappool")
                NotifyOfPropertyChange(() => MappoolViews);
            else if (message == "UpdateDefaultTournament")
                NotifyOfPropertyChange(() => FilterTournament);
            else if (message == "AddMappool")
                NotifyOfPropertyChange(() => MappoolViews);
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
                    NotifyOfPropertyChange(() => MappoolViews);
                    NotifyOfPropertyChange(() => ImportEnabled);
                }
            }
        }
        #endregion

        #region Properties
        public bool ImportEnabled
        {
            get { return FilterTournament != null; }
        }
        #endregion

        #region Actions
        public async void OpenNewMappoolDialog()
        {
            localLog.Information("new mappool dialog open");
            var model = new EditMappoolDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("save new mappool '{mappool}'", model.Name);
                var mappool = new Mappool()
                {
                    Name = model.Name,
                    Tournament = model.Tournament
                };
                mappool.Save();
                Settings.DefaultTournament = model.Tournament;
                NotifyOfPropertyChange(() => MappoolViews);
            }
        }

        public async void OpenWikiImportDialog()
        {
            localLog.Information("wiki import dialog open");
            var model = new MappoolWikiImportDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            await DialogHost.Show(view, "MainDialogHost");
        }
        #endregion
    }
}
