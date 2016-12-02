using Caliburn.Micro;
using MahApps.Metro.Controls;

namespace Osu.Mvvm.Miscellaneous
{
    /// <summary>
    /// Represents a base flyout view model
    /// </summary>
    public abstract class Flyout : PropertyChangedBase
    {
        #region Attributes
        /// <summary>
        /// The flyout header
        /// </summary>
        private string header;

        /// <summary>
        /// If the flyout is open
        /// </summary>
        private bool isOpen;

        /// <summary>
        /// The flyout position
        /// </summary>
        private Position position;
        #endregion

        #region Properties
        /// <summary>
        /// Header property
        /// </summary>
        public string Header
        {
            get
            {
                return this.header;
            }

            set
            {
                if (value != this.header)
                {
                    this.header = value;
                    this.NotifyOfPropertyChange(() => this.Header);
                }
            }
        }

        /// <summary>
        /// IsOpen property
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return this.isOpen;
            }

            set
            {
                if (value != this.isOpen)
                {
                    this.isOpen = value;
                    this.NotifyOfPropertyChange(() => this.IsOpen);
                }
            }
        }

        /// <summary>
        /// Position property
        /// </summary>
        public Position Position
        {
            get
            {
                return this.position;
            }

            set
            {
                if (value != this.position)
                {
                    this.position = value;
                    this.NotifyOfPropertyChange(() => this.Position);
                }
            }
        }
        #endregion
    }
}
