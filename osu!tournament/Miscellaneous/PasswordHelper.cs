using System.Windows;
using System.Windows.Controls;

namespace Osu.Mvvm.Miscellaneous
{
    /// <summary>
    /// Represents a password helper
    /// </summary>
    public static class PasswordHelper
    {
        #region Attributes
        /// <summary>
        /// The password property
        /// </summary>
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordHelper), new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

        /// <summary>
        /// The attach property
        /// </summary>
        public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PasswordHelper), new PropertyMetadata(false, Attach));

        /// <summary>
        /// The is updating property
        /// </summary>
        private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordHelper));
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the attach property
        /// </summary>
        /// <param name="dp">the dependency object</param>
        /// <returns>the value</returns>
        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }

        /// <summary>
        /// Sets the Attach property
        /// </summary>
        /// <param name="dp">the dependency object</param>
        /// <param name="value">the new value</param>
        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }

        /// <summary>
        /// Gets the password property
        /// </summary>
        /// <param name="dp">the dependency object</param>
        /// <returns>the value</returns>
        public static string GetPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(PasswordProperty);
        }

        /// <summary>
        /// Sets the password property
        /// </summary>
        /// <param name="dp">the dependency object</param>
        /// <param name="value">the new value</param>
        public static void SetPassword(DependencyObject dp, string value)
        {
            dp.SetValue(PasswordProperty, value);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets the is updating property
        /// </summary>
        /// <param name="dp">the dependency object</param>
        /// <returns>the value</returns>
        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

        /// <summary>
        /// Sets the is updating property
        /// </summary>
        /// <param name="dp">the dependency object</param>
        /// <param name="value">the new value</param>
        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        /// <summary>
        /// Called when the password property is changed
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the arguments</param>
        private static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            passwordBox.PasswordChanged -= PasswordChanged;

            if (!(bool)GetIsUpdating(passwordBox))
                passwordBox.Password = (string)e.NewValue;

            passwordBox.PasswordChanged += PasswordChanged;
        }

        /// <summary>
        /// Called when the attach property is changed
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the arguments</param>
        private static void Attach(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;

            if (passwordBox == null)
                return;

            if ((bool)e.OldValue)
                passwordBox.PasswordChanged -= PasswordChanged;

            if ((bool)e.NewValue)
                passwordBox.PasswordChanged += PasswordChanged;
        }

        /// <summary>
        /// Called when the password changed
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the arguments</param>
        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            SetIsUpdating(passwordBox, true);
            SetPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
        }
        #endregion
    }
}
