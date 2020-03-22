using Caliburn.Micro;
using script_chan2.Database;
using script_chan2.DataTypes;
using script_chan2.GUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace script_chan2
{
    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            Log.Information("AppBootstrapper: start");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs\\all.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            if (!DbCreator.DbExists)
                DbCreator.CreateDb();
            Database.Database.Initialize();

            Settings.Initialize();

            OsuIrc.OsuIrc.Login();

            Initialize();

            Log.Information("AppBootstrapper: app started");
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            Log.Information("AppBootstrapper: shutdown");
        }
    }
}
