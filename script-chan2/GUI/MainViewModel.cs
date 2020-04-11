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
        private ILogger localLog = Log.ForContext<MainViewModel>();

        #region Constructor
        public MainViewModel()
        {
            if (Database.Database.Tournaments.Count == 0)
                ShowTournaments();
            else
                ShowMatches();
            localLog.Information("main view loaded");
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
            localLog.Information("show match list");
            ActivateItem(new MatchesViewModel());
            Title = "Matches";
            DrawerExpanded = false;
        }

        public void ShowTournaments()
        {
            localLog.Information("show tournament list");
            ActivateItem(new TournamentsViewModel());
            Title = "Tournaments";
            DrawerExpanded = false;
        }

        public void ShowTeams()
        {
            localLog.Information("show team list");
            ActivateItem(new TeamsViewModel());
            Title = "Teams";
            DrawerExpanded = false;
        }

        public void ShowMappools()
        {
            localLog.Information("show mappool list");
            ActivateItem(new MappoolsViewModel());
            Title = "Mappools";
            DrawerExpanded = false;
        }

        public void ShowWebhooks()
        {
            localLog.Information("show webhook list");
            ActivateItem(new WebhooksViewModel());
            Title = "Webhooks";
            DrawerExpanded = false;
        }

        public void ShowSettings()
        {
            localLog.Information("show settings");
            ActivateItem(new SettingsViewModel());
            Title = "Settings";
            DrawerExpanded = false;
        }

        public void ShowColors()
        {
            localLog.Information("show colors");
            ActivateItem(new ColorsViewModel());
            Title = "Colors";
            DrawerExpanded = false;
        }

        public void ShowCommands()
        {
            localLog.Information("show commands");
            ActivateItem(new CustomCommandsViewModel());
            Title = "Commands";
            DrawerExpanded = false;
        }

        public void ShowChat()
        {
            localLog.Information("show chat");
            ActivateItem(new ChatViewModel());
            Title = "Chat";
            DrawerExpanded = false;
        }

        public void ShowExport()
        {
            localLog.Information("show export");
            ActivateItem(new ExportViewModel());
            Title = "Export";
            DrawerExpanded = false;
        }
        #endregion
    }
}
