using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Osu.Mvvm.Miscellaneous
{
    /// <summary>
    /// Represents a dialog manager
    /// </summary>
    public static class Dialog
    {
        #region Attributes
        /// <summary>
        /// The main metro window
        /// </summary>
        private static MetroWindow window;

        /// <summary>
        /// The progress dialog controller
        /// </summary>
        private static ProgressDialogController controller;
        #endregion

        #region Properties
        public static MetroWindow Window
        {
            get
            {
                return window;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes the window we need to display dialogs
        /// </summary>
        public static void Initialize()
        {

            window = Application.Current.Windows.OfType<MetroWindow>().First();
        }

        /// <summary>
        /// Shows a progress dialog.
        /// Warning: The progress dialog will be shown forever if not closed with the HideProgress method
        /// </summary>
        /// <param name="title">the dialog title</param>
        /// <param name="message">the dialog message</param>
        /// <returns>a task</returns>
        public static async Task ShowProgress(string title, string message)
        {
            if (controller != null)
                await controller.CloseAsync();

            controller = await window.ShowProgressAsync(title, message);
        }

        /// <summary>
        /// Hides the current progress dialog
        /// </summary>
        /// <returns></returns>
        public static async Task HideProgress()
        {
            if (controller != null)
            {
                await controller.CloseAsync();
                controller = null;
            }
        }

        /// <summary>
        /// Shows a message dialog
        /// </summary>
        /// <param name="title">the dialog title</param>
        /// <param name="message">the dialog message</param>
        public static async void ShowDialog(string title, string message)
        {
            await window.ShowMessageAsync(title, message);
        }

        /// <summary>
        /// Shows an input dialog
        /// </summary>
        /// <param name="title">the dialog title</param>
        /// <param name="message">the dialog message</param>
        /// <returns>the user input</returns>
        public static async Task<string> ShowInput(string title, string message)
        {
            return await window.ShowInputAsync(title, message);
        }

        /// <summary>
        /// Shows a confirmation dialog
        /// </summary>
        /// <param name="title">the dialog title</param>
        /// <param name="message">the dialog message</param>
        /// <returns>true/false</returns>
        public static async Task<bool> ShowConfirmation(string title, string message)
        {
            return await window.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative;
        }
        #endregion
    }
}
