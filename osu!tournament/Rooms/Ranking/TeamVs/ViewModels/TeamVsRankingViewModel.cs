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

        private MappoolPickerViewModel mpvm;

        private MultiplayerCommandsViewModel commandsVM;

        private bool abortHappened;

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
        public string RedTeamName
        {
            get
            {
                return ranking.Red.Name;
            }
        }

        public string BlueTeamName
        {
            get
            {
                return ranking.Blue.Name;
            }
        }

        public string RedTeamScore
        {
            get
            {
                return (ranking.Red.Points + ranking.Red.PointAddition).ToString();
            }
        }

        public string BlueTeamScore
        {
            get
            {
                return (ranking.Blue.Points + ranking.Blue.PointAddition).ToString();
            }
        }

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
                    NotifyOfPropertyChange("CurrentStatus");
                }
            }
        }

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

        public MultiplayerCommandsViewModel MultiCommands
        {
            get
            {
                return commandsVM;
            }
            set
            {
                if (value != commandsVM)
                {
                    commandsVM = value;
                    NotifyOfPropertyChange(() => MultiCommands);
                }
            }
        }

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

            currentStatus = "Team picking: " + current.Name + " | Next Team: " + next.Name;
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

        public void BlueAddPoint()
        {
            ranking.Blue.PointAddition++;
            Update();
        }

        public void RedAddPoint()
        {
            ranking.Red.PointAddition++;
            Update();
        }

        public void BlueRemovePoint()
        {
            ranking.Blue.PointAddition--;
            Update();
        }

        public void RedRemovePoint()
        {
            ranking.Red.PointAddition--;
            Update();
        }

        public void RevertPointChanges()
        {
            ranking.Red.PointAddition = 0;
            ranking.Blue.PointAddition = 0;
            Update();
        }

        public void AbortHappened()
        {
            OsuIrcBot.GetInstancePrivate().SendMessage("#mp_" + room.Id, "!mp abort");
            abortHappened = !abortHappened;
            ranking.DidAbortHappened = abortHappened;
            Dialog.ShowDialog("Whoops!", "Abort taken in consideration!");
        }
        #endregion
    }
}
