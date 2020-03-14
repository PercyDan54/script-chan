using Caliburn.Micro;
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
        #region Mappool list
        public BindableCollection<MappoolListItemViewModel> MappoolsViews
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
            if (message.ToString() == "DeleteMappool")
                NotifyOfPropertyChange(() => MappoolsViews);
        }
        #endregion

        #region Filter
        public Tournament FilterTournament
        {
            get { return Settings.DefaultTournament; }
            set
            {
                if (value != Settings.DefaultTournament)
                {
                    Log.Information("GUI mappool list set filter");
                    Settings.DefaultTournament = value;
                    NotifyOfPropertyChange(() => FilterTournament);
                    NotifyOfPropertyChange(() => MappoolsViews);
                }
            }
        }
        #endregion

        #region New mappool dialog
        private string newMappoolName;
        public string NewMappoolName
        {
            get { return newMappoolName; }
            set
            {
                if (value != newMappoolName)
                {
                    newMappoolName = value;
                    NotifyOfPropertyChange(() => NewMappoolName);
                    NotifyOfPropertyChange(() => NewMappoolSaveEnabled);
                }
            }
        }

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

        private Tournament newMappoolTournament;
        public Tournament NewMappoolTournament
        {
            get { return newMappoolTournament; }
            set
            {
                if (value != newMappoolTournament)
                {
                    newMappoolTournament = value;
                    NotifyOfPropertyChange(() => NewMappoolTournament);
                    NotifyOfPropertyChange(() => NewMappoolSaveEnabled);
                }
            }
        }

        public bool NewMappoolSaveEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(NewMappoolName))
                    return false;
                if (NewMappoolTournament == null)
                    return false;
                if (Database.Database.Mappools.Any(x => x.Name == NewMappoolName && x.Tournament == NewMappoolTournament))
                    return false;
                return true;
            }
        }

        public void NewMappoolDialogOpened()
        {
            Log.Information("GUI new mappool dialog open");
            NewMappoolName = "";
            NewMappoolTournament = Settings.DefaultTournament;
        }

        public void NewMappoolDialogClosed()
        {
            Log.Information("GUI new mappool '{mappool}' save", NewMappoolName);
            var mappool = new Mappool(NewMappoolName, NewMappoolTournament);
            mappool.Save();
            Settings.DefaultTournament = NewMappoolTournament;
            NotifyOfPropertyChange(() => NewMappoolTournament);
            NotifyOfPropertyChange(() => MappoolsViews);
        }
        #endregion
    }
}
