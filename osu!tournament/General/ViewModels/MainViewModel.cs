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
using System;
using System.ComponentModel;

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
        /// The teams view model
        /// </summary>
        private TeamsViewModel teams;

        /// <summary>
        /// The options view model
        /// </summary>
        private OptionsViewModel options;

        /// <summary>
        /// The flyouts control
        /// </summary>
        private FlyoutsControl flyoutsControl;

        /// <summary>
        /// Windows informations (size and position)
        /// </summary>
        private WindowInfo windowInfo;
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

            options = new OptionsViewModel();
            options.ChangedWebhookEvent += new EventHandler(ChangedWebhookEvent);

            bool alreadyExist;

            foreach (var room in Room.Rooms)
            {
                overviewRooms.addOverview(room.Value);
            }

            if(InfosHelper.TourneyInfos.Matches != null)
            {
                foreach (var game in InfosHelper.TourneyInfos.Matches)
                {
                    alreadyExist = false;
                    foreach (var kv in Room.Rooms)
                    {
                        if (kv.Value.Ranking.GetType() == typeof(TeamVs))
                        {
                            if (((TeamVs)kv.Value.Ranking).Blue.Name == game.TeamBlueName && ((TeamVs)kv.Value.Ranking).Red.Name == game.TeamRedName)
                            {
                                alreadyExist = true;
                            }
                        }
                    }

                    if (!alreadyExist)
                        overviewRooms.addOverview(game.TeamBlueName, game.TeamRedName, game.Batch);
                    else
                        overviewRooms.UpdateBatch(game.TeamBlueName, game.TeamRedName, game.Batch);
                }
            }

            rooms = new RoomsViewModel(overviewRooms, this);

            mappools = new MappoolsViewModel();

            teams = new TeamsViewModel(overviewRooms);

            windowInfo = new WindowInfo();

            if (Settings.Default.WindowLocation != null)
            {
                windowInfo.Left = Settings.Default.WindowLocation.X;
                windowInfo.Top = Settings.Default.WindowLocation.Y;
            }

            if(Settings.Default.WindowSize != null)
            {
                windowInfo.Width = Settings.Default.WindowSize.Width;
                windowInfo.Height = Settings.Default.WindowSize.Height;
            }

            if(!IsBlockedOnOption)
            {
                ShowRooms();
            }
            else
            {
                ShowOptions();
            }
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

        public FlyoutsControl ControlFlyouts { get; set; }

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
                    if (active_item_name != null && active_item_name == Resources.MainView_Rooms && rooms.ActiveItem != null && rooms.ActiveItem.SelectedTab.Header.ToString() == "Chat")
                    {
                        rooms.ActiveItem.RemoveNewMessageLine();
                    }
                    if (active_item_name != Resources.MainView_Mappools)
                    {
                        mappools.Update();
                    }
                    active_item_name = value;
                    NotifyOfPropertyChange(() => ActiveItemName);
                }
            }
        }

        /// <summary>
        /// WindowInfo property
        /// </summary>
        public WindowInfo WindowInfo
        {
            get
            {
                return windowInfo;
            }
            set
            {
                if(value != windowInfo)
                {
                    windowInfo = value;
                    NotifyOfPropertyChange(() => WindowInfo);
                }
            }
        }

        /// <summary>
        /// IsBlockedOnOption property if the api and irc is not connected
        /// </summary>
        private bool IsBlockedOnOption
        {
            get
            {
                return string.IsNullOrEmpty(options.ApiKey) || !options.CanConnect || !options.IsConnected;
            }
        }
        #endregion

        #region Public Methods
      
        /// <summary>
        /// Shows the overview screen
        /// </summary>
        public void ShowOverview()
        {
            if (!IsBlockedOnOption)
            {
                if (Transition == TransitionType.RightReplace)
                    Transition = TransitionType.LeftReplace;
                else
                    Transition = TransitionType.RightReplace;
                
                ActiveItemName = Resources.MainView_Overview;

                ActivateItem(overviewRooms);
            }
            else
            {
                ShowOptionTabDialog();
            }
        }

        /// <summary>
        /// Shows the rooms screen
        /// </summary>
        public void ShowRooms()
        {
            if (!IsBlockedOnOption)
            {
                if (Transition == TransitionType.RightReplace)
                    Transition = TransitionType.LeftReplace;
                else
                    Transition = TransitionType.RightReplace;

                ActiveItemName = Resources.MainView_Rooms;

                ActivateItem(rooms);
            }
            else
            {
                ShowOptionTabDialog();
            }
        }

        /// <summary>
        /// Shows the mappools screen
        /// </summary>
        public void ShowMappools()
        {
            if (!IsBlockedOnOption)
            {
                if (Transition == TransitionType.RightReplace)
                    Transition = TransitionType.LeftReplace;
                else
                    Transition = TransitionType.RightReplace;

                ActiveItemName = Resources.MainView_Mappools;

                ActivateItem(mappools);
            }
            else
            {
                ShowOptionTabDialog();
            }
        }

        /// <summary>
        /// Shows the mappools screen
        /// </summary>
        public void ShowTeams()
        {
            if (!IsBlockedOnOption)
            {
                if (Transition == TransitionType.RightReplace)
                    Transition = TransitionType.LeftReplace;
                else
                    Transition = TransitionType.RightReplace;

                ActiveItemName = Resources.MainView_Teams;

                ActivateItem(teams);
            }
            else
            {
                ShowOptionTabDialog();
            }
        }

        /// <summary>
        /// Shows the options screen
        /// </summary>
        public void ShowOptions()
        {
            Transition = TransitionType.LeftReplace;

            ActiveItemName = Resources.MainView_Options;

            ActivateItem(options);
        }
        #endregion

        #region Private Methods
        private void ShowOptionTabDialog()
        {
            Dialog.ShowDialog("Error", "You need to add the api key and connect to IRC before doing anything else!");
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
            InfosHelper.TourneyInfos.Save();
            InfosHelper.UserDataInfos.Save();
            TeamManager.Save();
            Room.Save();
            RefereeMatchHelper.Save();

            // Exit the irc bot
            OsuIrcBot.GetInstancePrivate().Disconnect();
            OsuIrcBot.GetInstancePublic().Disconnect();
            // Save all the caches
            Cache.SaveAll();
        }
        #endregion

        #region Events
        private void ChangedWebhookEvent(object o, EventArgs e)
        {
            if (rooms != null)
            {
                foreach (RoomViewModel rmv in rooms.Items)
                    rmv.DiscordActivationChanged();
            }
        }
        #endregion
    }
}
