using Caliburn.Micro;
using MahApps.Metro.Controls;
using Osu.Ircbot;
using Osu.Mvvm.Mappools.ViewModels;
using Osu.Mvvm.Ov.ViewModels;
using Osu.Mvvm.Rooms.ViewModels;
using Osu.Scores;
using Osu.Utils;
using System.Windows.Media;
using Osu.Mvvm.Preparation.ViewModels;
using Osu.Tournament.Miscellaneous;
using Osu.Tournament.Properties;
using Osu.Utils.Info;
using System.Threading.Tasks;
using Osu.Mvvm.Miscellaneous;
using Osu.Mvvm.Teams.ViewModels;
using Osu.Utils.TeamsOv;
using System.Windows;

namespace Osu.Mvvm.General.ViewModels
{
    /// <summary>
    /// Represents the main view model if API is not answering
    /// </summary>
    public class ErrorViewModel : Conductor<IScreen>.Collection.OneActive, IScreen
    {
       
        public ErrorViewModel()
        {
            DisplayName = "osu!tournament";
        }

        /// <summary>
        /// Icon property
        /// </summary>
        public ImageSource Icon
        {
            get
            {
                return IconUtilities.ToImageSource(Osu.Tournament.Properties.Resources.Icon);
            }
        }

        /// <summary>
        /// Closing the application
        /// </summary>
        public void QuitApplication()
        {
            if (Application.Current.MainWindow != null) Application.Current.MainWindow.Close();
        }
    }
}
