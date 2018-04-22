using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows;
using Osu.Mvvm.Miscellaneous;
using Osu.Utils.AutoUpdate;

namespace Osu.Tournament.AutoUpdate
{
    /// <summary>
    /// The UpdateManager class is a singleton handling update verification and process
    /// </summary>
    public sealed class UpdateManager
    {
        private static UpdateManager _instance;
        private AutoUpdateService _service;

        private UpdateManager()
        {
            _service = new AutoUpdateService(@"https://git.cartooncraft.fr/api/v4/projects/104/repository/tags");
            UpdateAvailable = false;
            HasUpdated = false;
        }

        public bool UpdateAvailable { get; private set; }

        public bool HasUpdated { get; private set; }

        public async Task CheckForUpdatesAsync()
        {
            Task<string> res = _service.GetNewVersion();
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string lastVersion = await res;

            if (!version.StartsWith(lastVersion))
            {
                UpdateAvailable = true;
                DownloadLatestVersion();
            }
        }

        public async void DownloadLatestVersion()
        {
            if (await Dialog.ShowConfirmation(Tournament.Properties.Resources.Update_NewVersionTitle,
                Tournament.Properties.Resources.Update_NewVersionMessage))
            {
                if (_service.DownloadNewVersion())
                {
                    HasUpdated = true;
                    System.Windows.Application.Current.Shutdown();
                }
                else
                {
                    Dialog.ShowDialog(Tournament.Properties.Resources.Error_Title, Tournament.Properties.Resources.Update_NewVersionError);
                }
            }
        }

        #region Singleton
        public static UpdateManager GetInstance()
        {
            return _instance ?? (_instance = new UpdateManager());
        }
        #endregion
    }
}
