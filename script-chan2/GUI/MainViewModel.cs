using Caliburn.Micro;
using Serilog;

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

        private string mainTitle;
        public string MainTitle
        {
            get { return mainTitle; }
            set
            {
                if (value != mainTitle)
                {
                    mainTitle = value;
                    NotifyOfPropertyChange(() => MainTitle);
                }
            }
        }
        #endregion

        #region Actions
        public void ShowMatches()
        {
            localLog.Information("show match list");
            ActivateItem(new MatchesViewModel());
            MainTitle = "Matches";
            DrawerExpanded = false;
        }

        public void ShowTournaments()
        {
            localLog.Information("show tournament list");
            ActivateItem(new TournamentsViewModel());
            MainTitle = "Tournaments";
            DrawerExpanded = false;
        }

        public void ShowTeams()
        {
            localLog.Information("show team list");
            ActivateItem(new TeamsViewModel());
            MainTitle = "Teams";
            DrawerExpanded = false;
        }

        public void ShowMappools()
        {
            localLog.Information("show mappool list");
            ActivateItem(new MappoolsViewModel());
            MainTitle = "Mappools";
            DrawerExpanded = false;
        }

        public void ShowWebhooks()
        {
            localLog.Information("show webhook list");
            ActivateItem(new WebhooksViewModel());
            MainTitle = "Webhooks";
            DrawerExpanded = false;
        }

        public void ShowSettings()
        {
            localLog.Information("show settings");
            ActivateItem(new SettingsViewModel());
            MainTitle = "Settings";
            DrawerExpanded = false;
        }

        public void ShowColors()
        {
            localLog.Information("show colors");
            ActivateItem(new ColorsViewModel());
            MainTitle = "Colors";
            DrawerExpanded = false;
        }

        public void ShowCommands()
        {
            localLog.Information("show commands");
            ActivateItem(new CustomCommandsViewModel());
            MainTitle = "Commands";
            DrawerExpanded = false;
        }

        public void ShowChat()
        {
            localLog.Information("show chat");
            ActivateItem(new ChatViewModel());
            MainTitle = "Chat";
            DrawerExpanded = false;
        }

        public void ShowExport()
        {
            localLog.Information("show export");
            ActivateItem(new ExportViewModel());
            MainTitle = "Export";
            DrawerExpanded = false;
        }
        #endregion
    }
}
