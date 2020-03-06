using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace script_chan2.GUI
{
    public class MainViewModel : Conductor<object>
    {
        public MainViewModel()
        {
            ShowMatches();
        }

        private bool drawerExpanded = false;
        public bool DrawerExpanded
        {
            get { return drawerExpanded; }
            set
            {
                drawerExpanded = value;
                NotifyOfPropertyChange(() => DrawerExpanded);
            }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                if (value != title)
                {
                    title = value;
                    NotifyOfPropertyChange(() => Title);
                }
            }
        }

        public void ShowMatches()
        {
            ActivateItem(new MatchesViewModel());
            Title = "Matches";
            DrawerExpanded = false;
        }

        public void ShowTournaments()
        {
            ActivateItem(new TournamentsViewModel());
            Title = "Tournaments";
            DrawerExpanded = false;
        }

        public void ShowTeams()
        {
            ActivateItem(new TeamsViewModel());
            Title = "Teams";
            DrawerExpanded = false;
        }

        public void ShowMappools()
        {
            ActivateItem(new MappoolsViewModel());
            Title = "Mappools";
            DrawerExpanded = false;
        }

        public void ShowWebhooks()
        {
            ActivateItem(new WebhooksViewModel());
            Title = "Webhooks";
            DrawerExpanded = false;
        }

        public void ShowSettings()
        {
            ActivateItem(new SettingsViewModel());
            Title = "Settings";
            DrawerExpanded = false;
        }
    }
}
