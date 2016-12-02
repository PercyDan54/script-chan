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

namespace Osu.Mvvm.General.ViewModels
{
    /// <summary>
    /// Represents the main view model
    /// </summary>
    public class MainViewModel : Conductor<IScreen>.Collection.OneActive, IScreen
    {
        #region Attributes
        /// <summary>
        /// The current transition
        /// </summary>
        private TransitionType transition;

        /// <summary>
        /// The active item name
        /// </summary>
        private string active_item_name;

        /// <summary>
        /// The overview view model
        /// </summary>
        private OvViewModel overviewRooms;

        /// <summary>
        /// The rooms view model
        /// </summary>
        private RoomsViewModel rooms;

        /// <summary>
        /// The mappools view model
        /// </summary>
        private MappoolsViewModel mappools;

        /// <summary>
        /// The options view model
        /// </summary>
        private OptionsViewModel options;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public MainViewModel()
        {
            DisplayName = "osu!tournament";

            active_item_name = "Nothing selected";

            overviewRooms = new OvViewModel();

            rooms = new RoomsViewModel(overviewRooms);

            mappools = new MappoolsViewModel();

            options = new OptionsViewModel();

            ShowRooms();
        }
        #endregion

        #region Properties
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
        /// Transition property
        /// </summary>
        public TransitionType Transition
        {
            get
            {
                return transition;
            }
            set
            {
                if (value != transition)
                {
                    transition = value;
                    NotifyOfPropertyChange(() => Transition);
                }
            }
        }

        /// <summary>
        /// Active item name property
        /// </summary>
        public string ActiveItemName
        {
            get
            {
                return active_item_name;
            }
            set
            {
                if (value != active_item_name)
                {
                    active_item_name = value;
                    NotifyOfPropertyChange(() => ActiveItemName);
                }
            }
        }
        #endregion

        #region Public Methods
      
        /// <summary>
        /// Shows the overview screen
        /// </summary>
        public void ShowOverview()
        {
            if (Transition == TransitionType.RightReplace)
                Transition = TransitionType.LeftReplace;
            else
                Transition = TransitionType.RightReplace;

            ActiveItemName = "Overview";

            ActivateItem(overviewRooms);
        }

        /// <summary>
        /// Shows the rooms screen
        /// </summary>
        public void ShowRooms()
        {
            if (Transition == TransitionType.RightReplace)
                Transition = TransitionType.LeftReplace;
            else
                Transition = TransitionType.RightReplace;

            ActiveItemName = "Rooms";

            ActivateItem(rooms);
        }

        /// <summary>
        /// Shows the mappools screen
        /// </summary>
        public void ShowMappools()
        {
            if (Transition == TransitionType.RightReplace)
                Transition = TransitionType.LeftReplace;
            else
                Transition = TransitionType.RightReplace;

            ActiveItemName = "Mappools";

            ActivateItem(mappools);
        }

        /// <summary>
        /// Shows the options screen
        /// </summary>
        public void ShowOptions()
        {
            Transition = TransitionType.LeftReplace;

            ActiveItemName = "Options";

            ActivateItem(options);
        }
        #endregion

        #region Handlers
        /// <summary>
        /// Called when the linked view is deactivated
        /// </summary>
        /// <param name="close">if the operation is a close operation</param>
        protected override void OnDeactivate(bool close)
        {
            // Save the mappools
            Mappool.Save();

            // Exit the irc bot
            OsuIrcBot.GetInstance().Disconnect();
            // Save all the caches
            Cache.SaveAll();
        }
        #endregion
    }
}
