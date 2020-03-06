using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class MappoolsViewModel : Screen, IHandle<string>
    {
        public BindableCollection<Mappool> Mappools { get; set; }

        public BindableCollection<MappoolListItemViewModel> MappoolsViews
        {
            get
            {
                var list = new BindableCollection<MappoolListItemViewModel>();
                foreach (var mappool in Mappools)
                    list.Add(new MappoolListItemViewModel(mappool));
                return list;
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

        protected override void OnActivate()
        {
            Reload();
            Events.Aggregator.Subscribe(this);
        }

        public void Reload()
        {
            Mappools = new BindableCollection<Mappool>();
            foreach (var mappool in Database.Database.Mappools.OrderBy(x => x.Name))
            {
                if (Settings.DefaultTournament != null && mappool.Tournament != Settings.DefaultTournament)
                    continue;
                Mappools.Add(mappool);
            }
            NotifyOfPropertyChange(() => Mappools);
            NotifyOfPropertyChange(() => MappoolsViews);
        }

        public void Handle(string message)
        {
            if (message.ToString() == "DeleteMappool")
                Reload();
        }

        public Tournament FilterTournament
        {
            get { return Settings.DefaultTournament; }
            set
            {
                if (value != Settings.DefaultTournament)
                {
                    Settings.DefaultTournament = value;
                    NotifyOfPropertyChange(() => FilterTournament);
                    Reload();
                }
            }
        }

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
                if (string.IsNullOrEmpty(newMappoolName))
                    return false;
                if (Mappools.Any(x => x.Name == newMappoolName && x.Tournament == newMappoolTournament))
                    return false;
                return true;
            }
        }

        public void NewMappoolDialogOpened()
        {
            NewMappoolName = "";
            NewMappoolTournament = Settings.DefaultTournament;
        }

        public void NewMappoolDialogClosed()
        {
            var mappool = new Mappool(NewMappoolName, NewMappoolTournament);
            mappool.Save();
            Settings.DefaultTournament = NewMappoolTournament;
            NotifyOfPropertyChange(() => NewMappoolTournament);
            Reload();
        }
    }
}
