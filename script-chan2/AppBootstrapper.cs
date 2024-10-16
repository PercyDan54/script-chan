﻿using Caliburn.Micro;
using script_chan2.Database;
using script_chan2.DataTypes;
using script_chan2.Discord;
using script_chan2.GUI;
using Serilog;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace script_chan2
{
    public class AppBootstrapper : BootstrapperBase
    {
        private ILogger localLog;

        public AppBootstrapper()
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File("logs\\all.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:u} [{Level}] ({SourceContext:1}) {Message}{NewLine}{Exception}", encoding: Encoding.UTF8)
                    .CreateLogger();

                localLog = Log.ForContext<AppBootstrapper>();
                localLog.Information("Logger initialized");

                Application.Current.DispatcherUnhandledException += Application_DispatcherUnhandledException;

                localLog.Information("Initialize DB");
                if (!DbCreator.DbExists)
                    DbCreator.CreateDb();
                DbUpgrader.Upgrade();
                Database.Database.Initialize();

                localLog.Information("Initialize settings");
                Settings.Initialize();

                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(Settings.Lang);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(Settings.Lang);

                localLog.Information("Login to irc");
                OsuIrc.OsuIrc.Login();

                localLog.Information("Initialize app");
                Initialize();

                DiscordApi.SetRichPresence(Properties.Resources.DiscordRPC_DefaultStatus);

                localLog.Information("app started");
            }
            catch (Exception ex)
            {
                if (!(bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)
                {
                    Log.Error(ex, "Bootstrapper exception");
                    MessageBox.Show(Properties.Resources.AppBootstrapper_BootstrapperException);
                }
                DiscordApi.StopRichPresence();
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Unhandled exception");
            MessageBox.Show(Properties.Resources.AppBootstrapper_UnhandledException);
            DiscordApi.StopRichPresence();
        }

        protected override void Configure()
        {
            var defaultCreateTrigger = Parser.CreateTrigger;

            Parser.CreateTrigger = (target, triggerText) =>
            {
                if (triggerText == null)
                {
                    return defaultCreateTrigger(target, null);
                }

                var triggerDetail = triggerText.Replace("[", string.Empty).Replace("]", string.Empty);

                var splits = triggerDetail.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                switch (splits[0])
                {
                    case "Key":
                        var key = (Key)Enum.Parse(typeof(Key), splits[1], true);
                        return new KeyTrigger { Key = key };

                    case "Gesture":
                        var mkg = (MultiKeyGesture)(new MultiKeyGestureConverter()).ConvertFrom(splits[1]);
                        return new KeyTrigger { Modifiers = mkg.KeySequences[0].Modifiers, Key = mkg.KeySequences[0].Keys[0] };
                }

                return defaultCreateTrigger(target, triggerText);
            };
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            localLog.Information("Display root view");
            DisplayRootViewFor<MainViewModel>();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            OsuIrc.OsuIrc.Shutdown();
            Settings.SaveConfig();
            DiscordApi.StopRichPresence();
            localLog.Information("app shutdown");
        }
    }
}
