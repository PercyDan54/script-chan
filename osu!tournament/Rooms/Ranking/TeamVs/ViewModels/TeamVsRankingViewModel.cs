using Caliburn.Micro;
using Osu.Ircbot;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using Osu.Utils;
using Osu.Utils.Info;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Osu.Mvvm.Rooms.Ranking.TeamVs.ViewModels
{
    public class TeamVsRankingViewModel : Screen, IRankingViewModel
    {
        #region Attributes
        /// <summary>
        /// The room
        /// </summary>
        protected Room room;

        /// <summary>
        /// The team vs object linked
        /// </summary>
        protected Osu.Scores.TeamVs ranking;

        /// <summary>
        /// The current status
        /// </summary>
        protected string currentStatus;

        /// <summary>
        /// The mappool picker view model
        /// </summary>
        private MappoolPickerViewModel mpvm;

        /// <summary>
        /// The multiplayer commands view model
        /// </summary>
        private MultiplayerCommandsViewModel commandsVM;

        /// <summary>
        /// Boolean to check if one abort happened or not
        /// </summary>
        private bool abortHappened;

        /// <summary>
        /// Boolean to send the message after moving users in the osu! room (should not be used anymore)
        /// </summary>
        private bool isMovingWithMessage;

        private Dictionary<string, SwitchHandler> switchhandlers;
        #endregion

        #region Constructor
        public TeamVsRankingViewModel(Room room, Osu.Scores.TeamVs ranking)
        {
            this.room = room;
            this.ranking = ranking;
            abortHappened = false;
            switchhandlers = new Dictionary<string, SwitchHandler>();
            isMovingWithMessage = true;

            MappoolPicker = new MappoolPickerViewModel(room);

            MultiCommands = new MultiplayerCommandsViewModel(room, ranking.Red.Name, ranking.Blue.Name);

            Update();
        }
        #endregion

        #region Properties
        /// <summary>
        /// The RedTeamName property
        /// </summary>
        public string RedTeamName
        {
            get
            {
                return ranking.Red.Name;
            }
        }

        /// <summary>
        /// The BlueTeamName property
        /// </summary>
        public string BlueTeamName
        {
            get
            {
                return ranking.Blue.Name;
            }
        }

        /// <summary>
        /// The RedTeamScore property
        /// </summary>
        public string RedTeamScore
        {
            get
            {
                return (ranking.Red.Points + ranking.Red.PointAddition).ToString();
            }
        }

        /// <summary>
        /// The BlueTeamScore property
        /// </summary>
        public string BlueTeamScore
        {
            get
            {
                return (ranking.Blue.Points + ranking.Blue.PointAddition).ToString();
            }
        }

        /// <summary>
        /// The CurrentStatus property
        /// </summary>
        public string CurrentStatus
        {
            get
            {
                return currentStatus;
            }
            set
            {
                if (value != currentStatus)
                {
                    currentStatus = value;
                    NotifyOfPropertyChange(() => CurrentStatus);
                }
            }
        }

        /// <summary>
        /// The MappoolPicker property
        /// </summary>
        public MappoolPickerViewModel MappoolPicker
        {
            get
            {
                return mpvm;
            }
            set
            {
                if (value != mpvm)
                {
                    mpvm = value;
                    NotifyOfPropertyChange(() => MappoolPicker);
                }
            }
        }

        /// <summary>
        /// The MultiCommands property
        /// </summary>
        public MultiplayerCommandsViewModel MultiCommands
        {
            get { return commandsVM; }
            set
            {
                if (commandsVM != value)
                {
                    commandsVM = value;
                    NotifyOfPropertyChange(() => MultiCommands);
                }
            }
        }

        /// <summary>
        /// The IsMOvingWithMessage property
        /// </summary>
        public bool IsMovingWithMessage
        {
            get
            {
                return isMovingWithMessage;
            }
            set
            {
                if (value != isMovingWithMessage)
                    isMovingWithMessage = value;
            }
        }

        /// <summary>
        /// The IsControlVisible property
        /// </summary>
        public string IsControlVisible
        {
            get
            {
                if (room.Manual == false)
                    return "Visible";
                else
                    return "Hidden";
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates the status label
        /// </summary>
        private void UpdateStatusLabel()
        {
            Team current = abortHappened ? ranking.NextTeam : ranking.CurrentTeam;
            Team next = abortHappened ? ranking.CurrentTeam : ranking.NextTeam;

            currentStatus = Utils.Properties.Resources.TeamVsRankingView_TeamPicking + ": " + current.Name + " | " + Utils.Properties.Resources.TeamVsRankingView_NextTeam + " Next Team: " + next.Name;
            NotifyOfPropertyChange(() => CurrentStatus);
            
        }
        #endregion

        #region Public Methods
        public void Update()
        {
            mpvm.Update();

            NotifyOfPropertyChange(() => RedTeamScore);
            NotifyOfPropertyChange(() => BlueTeamScore);
            NotifyOfPropertyChange(() => IsControlVisible);
            UpdateStatusLabel();
        }

        /// <summary>
        /// Update the discord features on ranking tab
        /// </summary>
        public void DiscordUpdate()
        {
            MappoolPicker?.UpdateDiscord();
        }

        /// <summary>
        /// Function called to add points to blue team manually
        /// </summary>
        public void BlueAddPoint()
        {
            ranking.Blue.PointAddition++;
            Update();
        }

        /// <summary>
        /// Function called to add points to red team manually
        /// </summary>
        public void RedAddPoint()
        {
            ranking.Red.PointAddition++;
            Update();
        }

        /// <summary>
        /// Function called to remove points to blue team manually
        /// </summary>
        public void BlueRemovePoint()
        {
            ranking.Blue.PointAddition--;
            Update();
        }

        /// <summary>
        /// Function called to remove points to red team manually
        /// </summary>
        public void RedRemovePoint()
        {
            ranking.Red.PointAddition--;
            Update();
        }

        /// <summary>
        /// Function called to revert changes to score made manually
        /// </summary>
        public void RevertPointChanges()
        {
            ranking.Red.PointAddition = 0;
            ranking.Blue.PointAddition = 0;
            Update();
        }

        /// <summary>
        /// Function called when you want to abort the map
        /// </summary>
        public void AbortHappened()
        {
            OsuIrcBot.GetInstancePrivate().SendMessage("#mp_" + room.Id, "!mp abort");
            abortHappened = !abortHappened;
            ranking.DidAbortHappened = abortHappened;
            Dialog.ShowDialog(Utils.Properties.Resources.Error_Title, Utils.Properties.Resources.Error_Abort);
        }
        #endregion
    }
}
