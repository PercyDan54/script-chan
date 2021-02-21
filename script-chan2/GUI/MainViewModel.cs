using Caliburn.Micro;
using Serilog;
using System.Windows;
using System.Windows.Input;

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
            MainTitle = Properties.Resources.MainView_MatchesMenuItem;
            DrawerExpanded = false;
        }

        public void ShowTournaments()
        {
            localLog.Information("show tournament list");
            ActivateItem(new TournamentsViewModel());
            MainTitle = Properties.Resources.MainView_TournamentsMenuItem;
            DrawerExpanded = false;
        }

        public void ShowTeams()
        {
            localLog.Information("show team list");
            ActivateItem(new TeamsViewModel());
            MainTitle = Properties.Resources.MainView_TeamsMenuItem;
            DrawerExpanded = false;
        }

        public void ShowMappools()
        {
            localLog.Information("show mappool list");
            ActivateItem(new MappoolsViewModel());
            MainTitle = Properties.Resources.MainView_MappoolsMenuItem;
            DrawerExpanded = false;
        }

        public void ShowWebhooks()
        {
            localLog.Information("show webhook list");
            ActivateItem(new WebhooksViewModel());
            MainTitle = Properties.Resources.MainView_WebhooksMenuItem;
            DrawerExpanded = false;
        }

        public void ShowSettings()
        {
            localLog.Information("show settings");
            ActivateItem(new SettingsViewModel());
            MainTitle = Properties.Resources.MainView_SettingsMenuItem;
            DrawerExpanded = false;
        }

        public void ShowColors()
        {
            localLog.Information("show colors");
            ActivateItem(new ColorsViewModel());
            MainTitle = Properties.Resources.MainView_ColorsMenuItem;
            DrawerExpanded = false;
        }

        public void ShowCommands()
        {
            localLog.Information("show commands");
            ActivateItem(new CustomCommandsViewModel());
            MainTitle = Properties.Resources.MainView_CommandsMenuItem;
            DrawerExpanded = false;
        }

        public void ShowChat()
        {
            localLog.Information("show chat");
            ActivateItem(new ChatViewModel());
            MainTitle = Properties.Resources.MainView_ChatMenuItem;
            DrawerExpanded = false;
        }

        public void ShowExport()
        {
            localLog.Information("show export");
            ActivateItem(new ExportViewModel());
            MainTitle = Properties.Resources.MainView_ExportMenuItem;
            DrawerExpanded = false;
        }
        #endregion

        #region Window Events
        public void Drag(MouseButtonEventArgs e)
        {
            localLog.Information("drag main window");
            if (e.ChangedButton != MouseButton.Left)
                return;
            ((MainView)GetView()).DragMove();
        }

        public void MinimizeWindow()
        {
            localLog.Information("minimize main window");
            ((MainView)GetView()).WindowState = WindowState.Minimized;
        }

        public Visibility WindowMaximizeVisible
        {
            get
            {
                if (((MainView)GetView()).WindowState != WindowState.Maximized)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public void MaximizeWindow()
        {
            localLog.Information("maximize main window");
            ((MainView)GetView()).WindowState = WindowState.Maximized;
            NotifyOfPropertyChange(() => WindowMaximizeVisible);
            NotifyOfPropertyChange(() => WindowRestoreVisible);
        }

        public Visibility WindowRestoreVisible
        {
            get
            {
                if (((MainView)GetView()).WindowState == WindowState.Maximized)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public void RestoreWindow()
        {
            localLog.Information("restore main window");
            ((MainView)GetView()).WindowState = WindowState.Normal;
            NotifyOfPropertyChange(() => WindowMaximizeVisible);
            NotifyOfPropertyChange(() => WindowRestoreVisible);
        }

        public void CloseWindow()
        {
            localLog.Information("close main window");
            TryClose();
        }
        #endregion
    }
}
