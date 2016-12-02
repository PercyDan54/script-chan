using Caliburn.Micro;
using log4net.Config;
using Osu.Api;
using Osu.Ircbot;
using Osu.Mvvm.General.ViewModels;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using Osu.Utils;
using osu_discord;
using System;
using System.IO;
using System.Text;
using System.Windows;

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
            // Use our configuration file to configure the logger
            XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(Osu.Tournament.Properties.Resources.LoggerConfig)));

            // Initialize the osu!ircbot
            OsuIrcBot.Initialize();

            // Initialize the discord bot
            DiscordBot.Initialize();

            // Initialize the BanHelper
            RefereeMatchHelper.Initialize();

            // Initialize the osu!api
            OsuApi.Initialize();

            // Check the osu!api key
            await OsuApi.CheckKey();

            // Initialize the mappools
            await Mappool.Initialize();

            // Initialize the rooms
            Room.Initialize();

            // Initialize the timed counters
            Counter.Initialize();

            // Display the main view
            DisplayRootViewFor<MainViewModel>();

            // Initialize the dialogs
            Dialog.Initialize();
        }
        #endregion
    }
}
