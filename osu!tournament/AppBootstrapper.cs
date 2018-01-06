using Caliburn.Micro;
using log4net.Config;
using Osu.Api;
using Osu.Ircbot;
using Osu.Mvvm.General.ViewModels;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using Osu.Tournament.Properties;
using Osu.Utils;
using Osu.Utils.Bans;
using Osu.Utils.Info;
using osu_discord;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Osu.Mvvm
{
    /// <summary>
    /// Represents the application bootstrapper
    /// </summary>
    public class AppBootstrapper : BootstrapperBase
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public AppBootstrapper()
        {
            Initialize();
        }
        #endregion

        #region Handlers
        /// <summary>
        /// Called on application startup
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the arguments</param>
        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            Cache c = Cache.GetCache("osu!userdata.db");
            InfosHelper.UserDataInfos = c.GetObject<UserDataInfo>("infos", new UserDataInfo());

            // Use our configuration file to configure the logger
            XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(Osu.Tournament.Properties.Resources.LoggerConfig)));
            
            // Initialize the osu!ircbot
            bool isIrcInit = OsuIrcBot.Initialize();
            // Initialize the discord bot
            bool isDiscordInit = false;
            isDiscordInit = DiscordBot.Initialize();

            Cache c2 = Cache.GetCache("osu!matches.db");
            InfosHelper.TourneyInfos = c2.GetObject<TourneyInfo>("infos", new TourneyInfo());

            // Initialize the BanHelper
            RefereeMatchHelper.Initialize();

            // Initialize the osu!api
            OsuApi.Initialize();

            // Check the osu!api key
            await OsuApi.CheckKey();

            ObsBanHelper.Initialize();

            ObsBanHelper.CheckPath();

            // Initialize the mappools
            await Mappool.Initialize();

            // Initialize the rooms
            await Room.Initialize();

            // Initialize the timed counters
            Counter.Initialize();

            // Display the main view
            DisplayRootViewFor<MainViewModel>();

            // Initialize the dialogs
            Dialog.Initialize();

            if(!isDiscordInit)
            {
                await Dialog.ShowConfirmation("Error", "Discord initialization failed. Invalid data in the cache.");
                Application.Current.MainWindow.Close();
            }

            if (!isIrcInit)
            {
                await Dialog.ShowConfirmation("Error", "IRC initialization failed. Invalid data in the cache.");
                Application.Current.MainWindow.Close();
            }

            await Task.Delay(3000);

            foreach (var room in Room.Rooms)
            {
                OsuIrcBot.GetInstancePrivate().OnAddRoom(room.Value);
            }
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Fatal("SEND HELP  : " + e.Exception.InnerException + e.Exception.StackTrace);

            // Save the mappools
            Mappool.Save();
            Room.Save();
            RefereeMatchHelper.Save();

            // Exit the irc bot
            if(OsuIrcBot.GetInstancePrivate() != null)
            {
                OsuIrcBot.GetInstancePrivate().Disconnect();
                OsuIrcBot.GetInstancePublic().Disconnect();
            }

            // Save all the caches
            Cache.SaveAll();

            Log.Fatal("SAVED ALL");
            base.OnUnhandledException(sender, e);
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            Settings.Default.WindowSize = new System.Drawing.Size((int)Dialog.Window.Width, (int)Dialog.Window.Height);
            Settings.Default.WindowLocation = new System.Drawing.Point((int)Dialog.Window.Left, (int)Dialog.Window.Top);
            Settings.Default.Save();

            base.OnExit(sender, e);
        }
        #endregion
    }
}
