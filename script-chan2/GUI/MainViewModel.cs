using Caliburn.Micro;
using Serilog;
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
        #region Constructor
        public MainViewModel()
        {
            if (Database.Database.Tournaments.Count == 0)
                ShowTournaments();
            else
                ShowMatches();
            Log.Information("GUI main view loaded");
        }
        #endregion

        #region Properties
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
        #endregion

        #region Actions
        public void ShowMatches()
        {
            Log.Information("GUI show match list");
            ActivateItem(new MatchesViewModel());
            Title = "Matches";
            DrawerExpanded = false;
        }

        public void ShowTournaments()
        {
            Log.Information("GUI show tournament list");
            ActivateItem(new TournamentsViewModel());
            Title = "Tournaments";
            DrawerExpanded = false;
        }

        public void ShowTeams()
        {
            Log.Information("GUI show team list");
            ActivateItem(new TeamsViewModel());
            Title = "Teams";
            DrawerExpanded = false;
        }

        public void ShowMappools()
        {
            Log.Information("GUI show mappool list");
            ActivateItem(new MappoolsViewModel());
            Title = "Mappools";
            DrawerExpanded = false;
        }

        public void ShowWebhooks()
        {
            Log.Information("GUI show webhook list");
            ActivateItem(new WebhooksViewModel());
            Title = "Webhooks";
            DrawerExpanded = false;
        }

        public void ShowSettings()
        {
            Log.Information("GUI show settings");
            ActivateItem(new SettingsViewModel());
            Title = "Settings";
            DrawerExpanded = false;
        }
        #endregion
    }
}
