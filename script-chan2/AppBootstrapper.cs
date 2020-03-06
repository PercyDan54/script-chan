using Caliburn.Micro;
using script_chan2.Database;
using script_chan2.GUI;
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
            if (!DbCreator.DbExists)
                DbCreator.CreateDb();
            Database.Database.Initialize();
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }
    }
}
